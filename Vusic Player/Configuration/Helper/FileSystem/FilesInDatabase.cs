using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;
using Windows.Storage;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class FilesInDatabase
    {
        public static ConcurrentBag<SongModel> discoveredSongs = new();
        public static event Action<IEnumerable<SongModel>>? SongsDiscovered;
        public static List<AudioTrackLite> rawSongs = new();
        public static List<string> FilterRedundantSubdirectories(IEnumerable<string> paths)
        {
            // Normalize and trim trailing directory separators
            var cleanPaths = paths
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => Path.GetFullPath(p).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p.Length) // Shorter paths (parent directories) come first
                .ToList();

            var finalPaths = new List<string>();

            foreach (var path in cleanPaths)
            {
                // If this path is a subfolder of an already accepted parent folder, ignore it
                bool isSubdirOfExisting = finalPaths.Any(parent =>
                    path.StartsWith(parent + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase));

                if (!isSubdirOfExisting)
                {
                    finalPaths.Add(path);
                }
            }

            return finalPaths;
        }
        public static async Task DisableFolderAsync(string folderPath)
        {
            // 1. Purge from SQLite
            var removedPaths = await DatabaseService.RemoveFolderFilesAsync(folderPath);
            var removedSet = new HashSet<string>(removedPaths, StringComparer.OrdinalIgnoreCase);

            // 2. Purge from in-memory collection
            App.MainWindowInstance?.DispatcherQueue.TryEnqueue(() =>
            {
                rawSongs.RemoveAll(song => removedSet.Contains(song.FilePath));
            });

            // 3. Re-initialize watcher with active folders
            await InitializeDatabaseFunctionsAsync();
        }

        public static async Task EnableFolderAsync(string folderPath)
        {
            // 1. Re-initialize watcher to start monitoring the new folder
            await InitializeDatabaseFunctionsAsync();

            // 2. Scan only the newly enabled folder and insert into SQLite
            var existingPaths = rawSongs
                .Select(s => Path.GetFullPath(s.FilePath))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            await DatabaseService.ScanAndSyncDiskAsync(
                new[] { folderPath },
                existingPaths,
                dispatcher: null
            );

            // 3. Re-read SQLite into rawSongs
            await LoadAllFiles();
        }
        // Helper: Reads enabled folders from settings
        public static async Task<List<string>> GetAllowedFolderPathsAsync()
        {
            var allowedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            System.Diagnostics.Debug.WriteLine($"[STARTUP SCAN] IsPicturesEnabled: {currentSettings.IsPicturesEnabled}");
            UserDataPaths paths = UserDataPaths.GetDefault();
            System.Diagnostics.Debug.WriteLine($"[DEBUG PATH] paths.Music: {paths.Music}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG PATH] paths.Downloads: {paths.Downloads}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG PATH] paths.Pictures: {paths.Pictures}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG PATH] paths.Videos: {paths.Videos}");
            System.Diagnostics.Debug.WriteLine($"[DEBUG PATH] paths.Documents: {paths.Documents}");
            var systemFolders = new (bool IsEnabled, string Path)[]
            {
            (currentSettings.IsMusicEnabled, paths.Music),
            (currentSettings.IsDownloadsEnabled, paths.Downloads),
            (currentSettings.IsPicturesEnabled, paths.Pictures),
            (currentSettings.IsVideosEnabled, paths.Videos),
            (currentSettings.IsDocumentEnabled, paths.Documents)
            };

            foreach (var (isEnabled, path) in systemFolders)
            {
                if (isEnabled && !string.IsNullOrWhiteSpace(path) && Directory.Exists(path))
                {
                    allowedPaths.Add(Path.GetFullPath(path));
                }
            }

            if (currentSettings.SavedFoldersOpened != null)
            {
                foreach (var item in currentSettings.SavedFoldersOpened)
                {
                    if (item.Show == true && !string.IsNullOrWhiteSpace(item.FolderPath) && Directory.Exists(item.FolderPath))
                    {
                        allowedPaths.Add(Path.GetFullPath(item.FolderPath));
                    }
                }
            }
            var filtered = FilterRedundantSubdirectories(allowedPaths);

            System.Diagnostics.Debug.WriteLine("=== FINAL SCAN TARGETS ===");
            foreach (var target in filtered)
            {
                System.Diagnostics.Debug.WriteLine($"[TARGET]: {target}");
            }

            return filtered;
        }

        public static async Task InitializeDatabaseFunctionsAsync()
        {
            discoveredSongs.Clear();
            List<string> targetFolders = await GetAllowedFolderPathsAsync();

            // Update the file watcher to watch only the allowed folders
            FileSystemWatch.WatchFolders(targetFolders);
        }

        public static Task RunBackgroundServiceAsync()
        {
            return Task.Run(async () =>
            {
                try
                {
                    await RunBackgroundScannerAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[BackgroundScanner Error]: {ex}");
                }
            });
        }

        public static async Task RunBackgroundScannerAsync()
        {
            try
            {
                var allowedFolders = await GetAllowedFolderPathsAsync();

                var existingPaths = rawSongs
                    .Select(s => Path.GetFullPath(s.FilePath))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // Pass the user-allowed folders into the scanner
                await DatabaseService.ScanAndSyncDiskAsync(
                    allowedFolders,
                    existingPaths,
                    dispatcher: null,
                    onSongDiscovered: newSong =>
                    {
                        discoveredSongs.Add(new SongModel
                        {
                            Title = newSong.Title,
                            Artist = newSong.Artist,
                            AlbumName = newSong.AlbumName,
                            FilePath = newSong.FilePath,
                            SongDuration = newSong.SongDuration
                        });
                    }
                );

                if (!discoveredSongs.IsEmpty)
                {
                    SongsDiscovered?.Invoke(discoveredSongs);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error in RunBackgroundScannerAsync: {ex.Message}");
            }
        }

        public static async Task LoadAllFiles()
        {
            try
            {
                // 1. Purge files belonging to disabled/removed folders
                var allowedFolders = await GetAllowedFolderPathsAsync();
                await DatabaseService.RemoveDisallowedFolderFilesAsync(allowedFolders);

                // 2. Fetch fresh, authorized records from SQLite
                var songsFromDb = await Task.Run(() => DatabaseService.GetAllSongs());

                // 3. Sync rawSongs
                lock (rawSongs)
                {
                    rawSongs.Clear();
                    if (songsFromDb != null)
                    {
                        rawSongs.AddRange(songsFromDb);
                    }
                }

                // 4. Check for disk deletions/tag changes
                await DatabaseService.CheckForModifiedOrDeletedFilesAsync(rawSongs.ToList());
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AN UNEXPECTED ERROR OCCURED: " + ex.Message);
                Logger.Log(ex.Message, "FilesInDataBase.LoadAllFiles", Logger.LogLevelType.Error);
            }
        }
        public static async Task ResetLibraryAsync()
        {
            await DatabaseService.ClearDatabaseAsync();
            rawSongs.Clear();
            discoveredSongs.Clear();
        }
    }
}

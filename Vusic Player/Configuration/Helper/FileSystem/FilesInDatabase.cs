using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Windows.Storage;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class FilesInDatabase
    {
        public static readonly string[] SearchPaths = new string[]
{
    Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + @"\Downloads",
    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
};
        public static void InitializeDatabaseFunctions()
        {
            discoveredSongs.Clear();

            List<string> fpaths = new();
            foreach (var item in SearchPaths)
            {
                fpaths.Add(item);
            }

            FileSystemWatch.WatchFolders(fpaths);
        }
        public static Task RunBackgroundServiceAsync()
        {
            // Fire-and-forget safely wrapped inside Task.Run
            return Task.Run(async () =>
            {
                try
                {
                    await RunBackgroundScannerAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[BackgroundScanner Error]: {ex}");
                }
            });
        }

        public static async Task RunBackgroundScannerAsync()
        {
            
            try
            {
                var FoundSongs = rawSongs;
                var existingPaths = FoundSongs
                    .Select(s => Path.GetFullPath(s.FilePath))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                await DatabaseService.ScanAndSyncDiskAsync(
                    existingPaths,
                    dispatcher: null, // Pass null for dispatcher so scanning runs 100% in background!
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
                System.Diagnostics.Debug.WriteLine($"Error in RunBackgroundScannerAsync: {ex.Message}");
            }
        }
        public static ConcurrentBag<SongModel> discoveredSongs = new();
        public static event Action<IEnumerable<SongModel>>? SongsDiscovered;
        public static List<AudioTrackLite> rawSongs = new();
        public static async Task LoadAllFiles()
        {
            try
            {
                rawSongs.Clear();
                rawSongs = await Task.Run(() => DatabaseService.GetAllSongs());

                await DatabaseService.CheckForModifiedOrDeletedFilesAsync(rawSongs);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AN UNEXPECTED ERROR OCCURED: " + ex.Message);
                Logger.Log(ex.Message, "FilesInDataBase.LoadAllFiles", Logger.LogLevelType.Error);
            }
        }
        public static async Task<List<AudioTrackLite>> GetAllFiles()
        {
            rawSongs.Clear();
            rawSongs = await Task.Run(() => DatabaseService.GetAllSongs());

            await DatabaseService.CheckForModifiedOrDeletedFilesAsync(rawSongs);
            return rawSongs;
        }
    }
}

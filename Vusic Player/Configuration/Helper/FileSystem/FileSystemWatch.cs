using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Extensions;
using Windows.System;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class FileSystemWatch
    {
        private static readonly List<FileSystemWatcher> _watchers = new();
        private static readonly ConcurrentDictionary<string, CancellationTokenSource> _debounceTokens = new();
        public static void WatchFolders(List<string> FolderPaths)
        {
            StopWatching();
            foreach (var path in FolderPaths)
            {
                if (!Directory.Exists(path)) continue;

                var watcher = new FileSystemWatcher(path)
                {
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    IncludeSubdirectories = true // Automatically watches all nested subfolders!
                };

                // Attach the exact same event handlers to all watchers
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed;
                watcher.EnableRaisingEvents = true;

                _watchers.Add(watcher);
            }
        }
        public static void StopWatching()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            _watchers.Clear();
        }
        private static void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            string extension = Path.GetExtension(e.FullPath).ToLower();
            if (extension != ".mp3" && extension != ".flac" && extension != ".m4a" && extension != ".wav")
            {
                return;
            }

            string oldPath = e.OldFullPath;
            string newPath = e.FullPath;

            // 2. Update Database & UI immediately for file path changes
            Task.Run(async () =>
            {
                //await DatabaseService.UpdateSongMetadataAsync(oldPath, newPath);


            });
        }

        private static async Task ProcessFileMetadataChangeAsync(string filePath)
        {
            // Simple retry loop in case File Explorer still holds a brief lock on the file
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    using var file = TagLib.File.Create(filePath);
                    string updatedAlbum = file.Tag.Album ?? "Unknown Album";
                    string updatedTitle = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath);
                    string updatedArtist =string.IsNullOrWhiteSpace(string.Join(", ", file.Tag.AlbumArtists))
                                    ? (string.IsNullOrWhiteSpace(file.Tag.FirstPerformer) ? "Unknown Artist" : file.Tag.FirstPerformer)
                                    : string.Join(", ", file.Tag.AlbumArtists);

                    FileModified?.Invoke(filePath, updatedAlbum, updatedArtist, updatedTitle);

                    // 1. Update SQLite DB
                    await DatabaseService.UpdateSongMetadataAsync(filePath, updatedTitle, updatedArtist, updatedAlbum);



                    break; // Success! Exit retry loop.
                }
                catch (IOException)
                {
                    await Task.Delay(200); // File locked, wait 200ms and retry
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to read updated tags: {ex.Message}");
                    break;
                }
            }
        }
        public static event Action<string, string, string,string>? FileModified;
        private static void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            string extension = Path.GetExtension(e.FullPath).ToLower();
            if (AudioExtensions.List.Contains(extension))
            {
                Debug.WriteLine("PATH BEING UPDATEDD " + e.FullPath);
                if (_debounceTokens.TryRemove(e.FullPath, out var existingToken))
                {
                    existingToken.Cancel();
                }
                var cts = new CancellationTokenSource();
                _debounceTokens[e.FullPath] = cts;
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(300, cts.Token);
                        await ProcessFileMetadataChangeAsync(e.FullPath);
                    }
                    catch (TaskCanceledException)
                    {
                        // Ignored intentional cancellation
                    }
                });
            }
        }
    }
}

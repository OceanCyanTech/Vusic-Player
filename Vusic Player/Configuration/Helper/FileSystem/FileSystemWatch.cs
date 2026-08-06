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
        public static void Pause()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = false;
            }
            Debug.WriteLine("[Watcher] All FileSystemWatchers PAUSED.");
        }

        /// <summary>
        /// Resumes all FileSystemWatchers after batch edits complete.
        /// </summary>
        public static void Resume()
        {
            foreach (var watcher in _watchers)
            {
                watcher.EnableRaisingEvents = true;
            }
            Debug.WriteLine("[Watcher] All FileSystemWatchers RESUMED.");
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

        public static async Task ProcessFileMetadataChangeAsync(string filePath)
        {
            // Retry up to 5 times if File Explorer holds a brief file lock
            for (int attempt = 0; attempt < 5; attempt++)
            {
                try
                {
                    // Open file stream with Read sharing mode to prevent COM/IO locking issues
                    using var file = TagLib.File.Create(filePath);
                    var tag = file.Tag;
                    string updatedTitle = file.Tag.Title ?? Path.GetFileNameWithoutExtension(filePath);
                    string updatedArtist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
                                    ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
                                    : string.Join(", ", tag.AlbumArtists);
                    string updatedAlbum = file.Tag.Album ?? "Unknown Album";
                    TimeSpan duration = file.Properties.Duration;
                    string genre = string.IsNullOrEmpty(string.Join(", ", file.Tag.Genres)) ? "Unknown Genre" : string.Join(", ", file.Tag.Genres);
                    // 1. Update SQLite DB
                    await DatabaseService.UpdateSongMetadataAsync(filePath, updatedTitle, updatedArtist, updatedAlbum);

                    FileModified?.Invoke(filePath, updatedAlbum, updatedArtist, updatedTitle, duration, genre);

                    break; // Success! Exit the retry loop
                }
                catch (Exception ex) when (ex is System.IO.IOException || ex is System.Runtime.InteropServices.COMException)
                {
                    // File is locked by Explorer or WinRT handle; wait 200ms and try again
                    await Task.Delay(200);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Watcher Error] Failed reading tags for '{filePath}': {ex.Message}");
                    break;
                }
            }
        }
        public static event Action<string, string, string, string, TimeSpan, string>? FileModified;
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

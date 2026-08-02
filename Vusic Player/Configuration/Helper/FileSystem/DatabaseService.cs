using Microsoft.Data.Sqlite;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Extensions;
using Windows.Storage;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public static class DatabaseService
    {
        private static readonly string DbPath = Path.Combine(
            ApplicationData.Current.LocalFolder.Path, "vusicplayer_library.db");

        private static readonly string ConnectionString = $"Data Source={DbPath};";

        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // Enable WAL Mode for multi-threaded performance
            using var walCmd = new SqliteCommand("PRAGMA journal_mode=WAL;", connection);
            walCmd.ExecuteNonQuery();

            string createTableQuery = @"
    CREATE TABLE IF NOT EXISTS Songs (
        FilePath TEXT PRIMARY KEY,
        Title TEXT,
        Artist TEXT,
        AlbumName TEXT,
        DurationTicks INTEGER,
        IsFavourite INTEGER,
        LastModified INTEGER DEFAULT 0
    );
    CREATE INDEX IF NOT EXISTS idx_artist ON Songs(Artist);";

            using var command = new SqliteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }
        public static async Task<List<SongModel>> GetSongsByAlbumAsync(string albumName)
        {
            var songs = new List<SongModel>();

            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // Query SQLite directly filtered by AlbumName
                string query = @"SELECT FilePath, Title, Artist, AlbumName, DurationTicks, IsFavourite, LastModified 
                        FROM Songs 
                        WHERE AlbumName = @AlbumName";

                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@AlbumName", albumName);

                using var reader = command.ExecuteReader();

                int colFilePath = reader.GetOrdinal("FilePath");
                int colTitle = reader.GetOrdinal("Title");
                int colArtist = reader.GetOrdinal("Artist");
                int colAlbum = reader.GetOrdinal("AlbumName");
                int colDuration = reader.GetOrdinal("DurationTicks");

                while (reader.Read())
                {
                    long? durationTicks = reader.IsDBNull(colDuration) ? null : reader.GetInt64(colDuration);

                    songs.Add(new SongModel
                    {
                        FilePath = reader.GetString(colFilePath),
                        Title = reader.IsDBNull(colTitle) ? "" : reader.GetString(colTitle),
                        Artist = reader.IsDBNull(colArtist) ? "Unknown Artist" : reader.GetString(colArtist),
                        AlbumName = reader.IsDBNull(colAlbum) ? "Unknown Album" : reader.GetString(colAlbum),
                        SongDuration = durationTicks.HasValue && durationTicks.Value > 0
                            ? TimeSpan.FromTicks(durationTicks.Value)
                            : null
                    });
                }
            });

            return songs;
        }

        public static List<AudioTrackLite> GetAllSongs()
        {
            var songs = new List<AudioTrackLite>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // ADD LastModified TO THE SELECT QUERY BELOW:
            string selectQuery = "SELECT FilePath, Title, Artist, AlbumName, DurationTicks, IsFavourite, LastModified FROM Songs";
            using var command = new SqliteCommand(selectQuery, connection);
            using var reader = command.ExecuteReader();

            int colFilePath = reader.GetOrdinal("FilePath");
            int colTitle = reader.GetOrdinal("Title");
            int colArtist = reader.GetOrdinal("Artist");
            int colAlbum = reader.GetOrdinal("AlbumName");
            int colDuration = reader.GetOrdinal("DurationTicks");
            int colFav = reader.GetOrdinal("IsFavourite");
            int colModified = reader.GetOrdinal("LastModified"); // <-- Works now!

            while (reader.Read())
            {
                long? durationTicks = reader.IsDBNull(colDuration) ? null : reader.GetInt64(colDuration);

                songs.Add(new AudioTrackLite
                {
                    FilePath = reader.GetString(colFilePath),
                    Title = reader.IsDBNull(colTitle) ? "" : reader.GetString(colTitle),
                    Artist = reader.IsDBNull(colArtist) ? "Unknown Artist" : reader.GetString(colArtist),
                    AlbumName = reader.IsDBNull(colAlbum) ? "Unknown Album" : reader.GetString(colAlbum),
                    SongDuration = durationTicks.HasValue && durationTicks.Value > 0
                        ? TimeSpan.FromTicks(durationTicks.Value)
                        : null,
                    IsFavourite = !reader.IsDBNull(colFav) && reader.GetInt32(colFav) == 1,
                    LastModifiedTicks = reader.IsDBNull(colModified) ? 0L : reader.GetInt64(colModified)
                });
            }

            return songs;
        }
        public static async Task CheckForModifiedOrDeletedFilesAsync(List<AudioTrackLite> cachedSongs)
        {
            await Task.Run(() =>
            {
                var songsToUpdate = new System.Collections.Concurrent.ConcurrentBag<AudioTrackLite>();
                var missingPaths = new System.Collections.Concurrent.ConcurrentBag<string>();

                // Parallelize disk I/O checks across multi-core CPUs
                Parallel.ForEach(cachedSongs, song =>
                {
                    if (!File.Exists(song.FilePath))
                    {
                        missingPaths.Add(song.FilePath);
                        return;
                    }

                    // OS timestamp check
                    long currentDiskTicks = File.GetLastWriteTimeUtc(song.FilePath).Ticks;

                    if (currentDiskTicks > song.LastModifiedTicks)
                    {
                        try
                        {
                            using var stream = new FileStream(song.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            var abstraction = new SimpleStreamAbstraction(song.FilePath, stream);
                            using var tagFile = TagLib.File.Create(abstraction);
                            var tag = tagFile.Tag;

                            song.Title = string.IsNullOrWhiteSpace(tag.Title)
                                ? Path.GetFileNameWithoutExtension(song.FilePath)
                                : tag.Title;
                            song.Artist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
                                ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
                                : string.Join(", ", tag.AlbumArtists);
                            song.AlbumName = string.IsNullOrWhiteSpace(tag.Album)
                                ? "Unknown Album"
                                : tag.Album;
                            song.LastModifiedTicks = currentDiskTicks;

                            songsToUpdate.Add(song);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Failed re-reading modified file {song.FilePath}: {ex.Message}");
                        }
                    }
                });

                // Clean up deleted files from SQLite
                if (!missingPaths.IsEmpty)
                {
                    RemoveDeletedSongs(missingPaths.ToList());
                }

                // Batch update modified tags in SQLite
                if (!songsToUpdate.IsEmpty)
                {
                    SaveSongs(songsToUpdate.ToList());
                }
            });
        }
        private static void RemoveDeletedSongs(IEnumerable<string> filePaths)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            using var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Songs WHERE FilePath = $path";
            var pPath = command.Parameters.Add("$path", SqliteType.Text);

            foreach (var path in filePaths)
            {
                pPath.Value = path;
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private static readonly string[] SearchPaths =
        {
            UserDataPaths.GetDefault().Music,
            UserDataPaths.GetDefault().Downloads,
            UserDataPaths.GetDefault().Documents,
            UserDataPaths.GetDefault().Videos,
            UserDataPaths.GetDefault().Pictures
        };

        public static async Task<bool> UpdateSongMetadataAsync(string filePath, string newTitle, string newArtist, string newAlbum)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using var connection = new SqliteConnection(ConnectionString);
                    connection.Open();

                    string query = @"
                    UPDATE Songs 
                    SET Title = @Title, 
                        Artist = @Artist, 
                        AlbumName = @AlbumName,
                        LastModified = @LastModified
                    WHERE FilePath = @FilePath;";

                    using var command = new SqliteCommand(query, connection);

                    command.Parameters.AddWithValue("@Title", string.IsNullOrWhiteSpace(newTitle) ? "Unknown Title" : newTitle);
                    command.Parameters.AddWithValue("@Artist", string.IsNullOrWhiteSpace(newArtist) ? "Unknown Artist" : newArtist);
                    command.Parameters.AddWithValue("@AlbumName", string.IsNullOrWhiteSpace(newAlbum) ? "Unknown Album" : newAlbum);

                    // Fetch real file write time or fallback to UtcNow ticks
                    long diskTicks = File.Exists(filePath)
                        ? File.GetLastWriteTimeUtc(filePath).Ticks
                        : DateTime.UtcNow.Ticks;

                    command.Parameters.AddWithValue("@LastModified", diskTicks);
                    command.Parameters.AddWithValue("@FilePath", filePath);

                    int rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[DatabaseService] Error updating song metadata for '{filePath}': {ex.Message}");
                    return false;
                }
            });
        }

        public static async Task ScanAndSyncDiskAsync(
            HashSet<string> existingPaths,
            DispatcherQueue? dispatcher = null,
            Action<AudioTrackLite>? onSongDiscovered = null)
        {
            // 1. Resolve search paths using pure .NET System APIs (100% thread-safe)
            List<string> safePaths = new()
    {
        Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
        Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
    };

            await Task.Run(async () =>
            {
                var newlyDiscoveredSongs = new List<AudioTrackLite>();
                int filesProcessedThisRun = 0;

                foreach (var path in safePaths)
                {
                    if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) continue;

                    try
                    {
                        // EnumerationOptions prevents crashing on locked/system/reparse directories
                        var enumOptions = new EnumerationOptions
                        {
                            IgnoreInaccessible = true,
                            RecurseSubdirectories = true,
                            AttributesToSkip = System.IO.FileAttributes.Hidden | System.IO.FileAttributes.System
                        };

                        var directoryInfo = new DirectoryInfo(path);
                        var files = directoryInfo.EnumerateFiles("*.*", enumOptions)
                            .Where(f => AudioExtensions.List.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

                        foreach (var file in files)
                        {
                            string normalizedPath = Path.GetFullPath(file.FullName);

                            if (existingPaths.Contains(normalizedPath)) continue;

                            try
                            {
                                //using var stream = new FileStream(
                                //    normalizedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

                                //var abstraction = new SimpleStreamAbstraction(normalizedPath, stream);
                                //using var tagFile = TagLib.File.Create(abstraction);
                                using var tagFile = TagLib.File.Create(normalizedPath);
                                var tag = tagFile.Tag;

                                string artist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
                                    ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
                                    : string.Join(", ", tag.AlbumArtists);

                                string title = string.IsNullOrWhiteSpace(tag.Title)
                                    ? Path.GetFileNameWithoutExtension(normalizedPath)
                                    : tag.Title;

                                string album = string.IsNullOrWhiteSpace(tag.Album)
                                    ? "Unknown Album"
                                    : tag.Album;

                                var newSong = new AudioTrackLite
                                {
                                    FilePath = normalizedPath,
                                    Title = title,
                                    Artist = artist,
                                    AlbumName = album,
                                    SongDuration = tagFile.Properties.Duration
                                };

                                newlyDiscoveredSongs.Add(newSong);
                                existingPaths.Add(normalizedPath);

                                if (newlyDiscoveredSongs.Count % 15 == 0 && onSongDiscovered != null)
                                {
                                    var batch = newlyDiscoveredSongs.TakeLast(15).ToList();

                                    if (dispatcher != null)
                                    {
                                        dispatcher.TryEnqueue(() =>
                                        {
                                            foreach (var song in batch) onSongDiscovered(song);
                                        });
                                    }
                                    else
                                    {
                                        foreach (var song in batch) onSongDiscovered(song);
                                    }
                                }
                            }
                            catch (TagLib.UnsupportedFormatException) { }
                            catch (TagLib.CorruptFileException) { }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"TagLib error for {normalizedPath}: {ex.Message}");
                            }

                            filesProcessedThisRun++;
                            if (filesProcessedThisRun % 10 == 0)
                            {
                                await Task.Delay(5);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Directory scan error for {path}: {ex.Message}");
                    }
                }

                int remainingCount = newlyDiscoveredSongs.Count % 15;
                if (remainingCount > 0 && onSongDiscovered != null)
                {
                    var finalBatch = newlyDiscoveredSongs.TakeLast(remainingCount).ToList();
                    if (dispatcher != null)
                    {
                        dispatcher.TryEnqueue(() =>
                        {
                            foreach (var song in finalBatch) onSongDiscovered(song);
                        });
                    }
                    else
                    {
                        foreach (var song in finalBatch) onSongDiscovered(song);
                    }
                }

                if (newlyDiscoveredSongs.Count > 0)
                {
                    SaveSongs(newlyDiscoveredSongs);
                }
            });
        }
        public static void SaveSongs(IEnumerable<AudioTrackLite> songs)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            string insertQuery = @"
            INSERT OR REPLACE INTO Songs (FilePath, Title, Artist, AlbumName, DurationTicks, IsFavourite, LastModified)
            VALUES ($filePath, $title, $artist, $album, $duration, $isFav, $lastModified);";

            using var command = connection.CreateCommand();
            command.CommandText = insertQuery;

            var pPath = command.Parameters.Add("$filePath", SqliteType.Text);
            var pTitle = command.Parameters.Add("$title", SqliteType.Text);
            var pArtist = command.Parameters.Add("$artist", SqliteType.Text);
            var pAlbum = command.Parameters.Add("$album", SqliteType.Text);
            var pDuration = command.Parameters.Add("$duration", SqliteType.Integer);
            var pFav = command.Parameters.Add("$isFav", SqliteType.Integer);
            var pLastModified = command.Parameters.Add("$lastModified", SqliteType.Integer);

            foreach (var song in songs)
            {
                pPath.Value = song.FilePath;
                pTitle.Value = song.Title ?? "";
                pArtist.Value = song.Artist ?? "Unknown Artist";
                pAlbum.Value = song.AlbumName ?? "Unknown Album";
                pDuration.Value = song.SongDuration.HasValue ? song.SongDuration.Value.Ticks : 0L;
                pFav.Value = song.IsFavourite ? 1 : 0;

                // Get physical file's write time in UTC ticks for startup sync
                pLastModified.Value = File.Exists(song.FilePath)
                    ? File.GetLastWriteTimeUtc(song.FilePath).Ticks
                    : DateTime.UtcNow.Ticks;

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }
}
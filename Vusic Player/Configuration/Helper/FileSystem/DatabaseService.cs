using Microsoft.Data.Sqlite;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vortice.Direct2D1.Effects;
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
Genre TEXT,
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
                string query = @"SELECT FilePath, Title, Artist, AlbumName, Genre, DurationTicks, IsFavourite, LastModified 
                        FROM Songs 
                        WHERE AlbumName = @AlbumName";

                using var command = new SqliteCommand(query, connection);
                command.Parameters.AddWithValue("@AlbumName", albumName);

                using var reader = command.ExecuteReader();

                int colFilePath = reader.GetOrdinal("FilePath");
                int colTitle = reader.GetOrdinal("Title");
                int colArtist = reader.GetOrdinal("Artist");
                int colAlbum = reader.GetOrdinal("AlbumName");
                int colGenre = reader.GetOrdinal("Genre");
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
        public static async Task<List<SongModel>> GetSongsByGenreAsync(string genreName)
        {
            var groupedSongs = new List<SongModel>();
            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();
                using var command = connection.CreateCommand();
                command.CommandText = @"SELECT FilePath, Title, Artist, AlbumName, COALESCE(NULLIF(Genre, ' '), 'Unknown') AS Genre, DurationTicks FROM Songs ORDER BY Genre, Title";
                using var reader = command.ExecuteReader();
                int colPath = reader.GetOrdinal("FilePath");
                int colTitle = reader.GetOrdinal("Title");
                int colArtist = reader.GetOrdinal("Artist");
                int colAlbum = reader.GetOrdinal("AlbumName");
                int colGenre = reader.GetOrdinal("Genre");
                int colDurationTicks = reader.GetOrdinal("DurationTicks");

                while (reader.Read())
                {
                    string path = reader.GetString(colPath);
                    string artist = reader.IsDBNull(colArtist) ? "Unknown Artist" : reader.GetString(colArtist);
                    string album = reader.IsDBNull(colAlbum) ? "Unknown Album" : reader.GetString(colAlbum);
                    string genrestring = reader.IsDBNull(colGenre) ? "Unknown Genre" : reader.GetString(colGenre);
                    string title = reader.IsDBNull(colTitle) ? Path.GetFileNameWithoutExtension(path) : reader.GetString(colTitle);
                    long duration = reader.IsDBNull(colDurationTicks) ? 0 : reader.GetInt64(colDurationTicks);
                    var song = new SongModel
                    {
                        FilePath = path,
                        Artist = artist,
                        AlbumName = album,
                        Genre = genrestring,
                        SongDuration = TimeSpan.FromTicks(duration),
                        Title = title
                    };
                    groupedSongs.Add(song);
                }

            });
            return groupedSongs;
        }
        public static async Task<Dictionary<string, TimeSpan>> GetDurationsFromDatabase(List<string> filePaths)
        {
            var durations = new Dictionary<string, TimeSpan>(StringComparer.OrdinalIgnoreCase);

            if (filePaths == null || filePaths.Count == 0)
                return durations;

            await Task.Run(() =>
            {
                using var connection = new SqliteConnection(ConnectionString);
                connection.Open();

                // Batch in chunks of 500 to stay safely under SQLite query limits
                foreach (var chunk in filePaths.Chunk(500))
                {
                    using var command = connection.CreateCommand();

                    // Build dynamic SQL IN parameters ($p0, $p1, $p2...)
                    var paramList = new List<string>();
                    for (int i = 0; i < chunk.Length; i++)
                    {
                        string paramName = $"$p{i}";
                        paramList.Add(paramName);
                        command.Parameters.AddWithValue(paramName, chunk[i]);
                    }

                    command.CommandText = $"SELECT FilePath, DurationTicks FROM Songs WHERE FilePath IN ({string.Join(",", paramList)})";

                    using var reader = command.ExecuteReader();
                    int colPath = reader.GetOrdinal("FilePath");
                    int colDuration = reader.GetOrdinal("DurationTicks");

                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(colDuration))
                        {
                            string path = reader.GetString(colPath);
                            long ticks = reader.GetInt64(colDuration);

                            if (ticks > 0)
                            {
                                durations[path] = TimeSpan.FromTicks(ticks);
                            }
                        }
                    }
                }
            });

            return durations;
        }
        public static List<AudioTrackLite> GetAllSongs()
        {
            var songs = new List<AudioTrackLite>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            // ADD LastModified TO THE SELECT QUERY BELOW:
            string selectQuery = "SELECT FilePath, Title, Artist, AlbumName, Genre, DurationTicks, IsFavourite, LastModified FROM Songs";
            using var command = new SqliteCommand(selectQuery, connection);
            using var reader = command.ExecuteReader();

            int colFilePath = reader.GetOrdinal("FilePath");
            int colTitle = reader.GetOrdinal("Title");
            int colArtist = reader.GetOrdinal("Artist");
            int colAlbum = reader.GetOrdinal("AlbumName");
            int colGenre = reader.GetOrdinal("Genre");
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
                    Genre = reader.IsDBNull(colGenre) ? "Unknown Genre" : reader.GetString(colGenre),
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
                            song.Genre = string.Join(", ", tag.Genres);

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


        public static async Task<bool> UpdateSongMetadataAsync(string filePath, string newTitle, string newArtist, string newAlbum, string genre = "Unknown Genre")
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
                        LastModified = @LastModified,
                       Genre = @Genre
                    WHERE FilePath = @FilePath;";

                    using var command = new SqliteCommand(query, connection);

                    command.Parameters.AddWithValue("@Title", string.IsNullOrWhiteSpace(newTitle) ? "Unknown Title" : newTitle);
                    command.Parameters.AddWithValue("@Artist", string.IsNullOrWhiteSpace(newArtist) ? "Unknown Artist" : newArtist);
                    command.Parameters.AddWithValue("@AlbumName", string.IsNullOrWhiteSpace(newAlbum) ? "Unknown Album" : newAlbum);
                    command.Parameters.AddWithValue("@Genre", string.IsNullOrWhiteSpace(genre) ? "Unknown Genre" : genre);

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
                    var enumOptions = new EnumerationOptions
                    {
                        IgnoreInaccessible = true,
                        RecurseSubdirectories = true,
                        AttributesToSkip = System.IO.FileAttributes.Hidden | System.IO.FileAttributes.System
                    };

                    var directoryInfo = new DirectoryInfo(path);
                    var files = directoryInfo.EnumerateFiles("*.*", enumOptions)
                        .Where(f => AudioExtensions.List.Contains(f.Extension, StringComparer.OrdinalIgnoreCase) || VideoExtensions.List.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

                        foreach (var file in files)
                        {
                            string normalizedPath = Path.GetFullPath(file.FullName);

                            if (existingPaths.Contains(normalizedPath)) continue;

                            try
                            {
                                string fileExtension = Path.GetExtension(normalizedPath).ToLowerInvariant();

                                using var stream = new FileStream(normalizedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                                var abstraction = new SimpleStreamAbstraction(normalizedPath, stream);
                                using var tagFile = TagLib.File.Create(abstraction);

                                var tag = tagFile.Tag;
                                string title = string.IsNullOrWhiteSpace(tag.Title)
                                     ? Path.GetFileNameWithoutExtension(normalizedPath)
                                     : tag.Title;
                                string genre = string.IsNullOrWhiteSpace(string.Join(", ", tag.Genres)) ? "Unknown Genre" : string.Join(", ", tag.Genres);

                                // Use standard FileStream + SimpleStreamAbstraction to bypass WinRT COM marshaling
                                if (AudioExtensions.List.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                                {

                                    string artist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
                                        ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
                                        : string.Join(", ", tag.AlbumArtists);

                                 

                                    string album = string.IsNullOrWhiteSpace(tag.Album)
                                        ? "Unknown Album"
                                        : tag.Album;

                                    var newSong = new AudioTrackLite
                                    {
                                        FilePath = normalizedPath,
                                        Title = title,
                                        Artist = artist,
                                        AlbumName = album,
                                        SongDuration = tagFile.Properties.Duration,
                                        Genre = genre
                                    };

                                    newlyDiscoveredSongs.Add(newSong);
                                }
                                else if(VideoExtensions.List.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
                                {
                                    var newSong = new AudioTrackLite
                                    {
                                        FilePath = normalizedPath,
                                        Title = title,
                                        SongDuration = tagFile.Properties.Duration,
                                        Genre = genre
                                    };

                                    newlyDiscoveredSongs.Add(newSong);
                                }
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
        //    public static async Task ScanAndSyncDiskAsync(
        //        HashSet<string> existingPaths,
        //        DispatcherQueue? dispatcher = null,
        //        Action<AudioTrackLite>? onSongDiscovered = null)
        //    {
        //        // 1. Resolve search paths using pure .NET System APIs (100% thread-safe)
        //        List<string> safePaths = new()
        //{
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyMusic),
        //    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"),
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
        //    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
        //};

        //        await Task.Run(async () =>
        //        {
        //            var newlyDiscoveredSongs = new List<AudioTrackLite>();
        //            int filesProcessedThisRun = 0;

        //            foreach (var path in safePaths)
        //            {
        //                if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) continue;

        //                try
        //                {
        //                    // EnumerationOptions prevents crashing on locked/system/reparse directories
        //                    var enumOptions = new EnumerationOptions
        //                    {
        //                        IgnoreInaccessible = true,
        //                        RecurseSubdirectories = true,
        //                        AttributesToSkip = System.IO.FileAttributes.Hidden | System.IO.FileAttributes.System
        //                    };

        //                    var directoryInfo = new DirectoryInfo(path);
        //                    var files = directoryInfo.EnumerateFiles("*.*", enumOptions)
        //                        .Where(f => AudioExtensions.List.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

        //                    foreach (var file in files)
        //                    {
        //                        string normalizedPath = Path.GetFullPath(file.FullName);

        //                        if (existingPaths.Contains(normalizedPath)) continue;

        //                        try
        //                        {
        //                            //using var stream = new FileStream(
        //                            //    normalizedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

        //                            //var abstraction = new SimpleStreamAbstraction(normalizedPath, stream);
        //                            //using var tagFile = TagLib.File.Create(abstraction);
        //                            using var tagFile = TagLib.File.Create(normalizedPath);
        //                            var tag = tagFile.Tag;

        //                            string artist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
        //                                ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
        //                                : string.Join(", ", tag.AlbumArtists);

        //                            string title = string.IsNullOrWhiteSpace(tag.Title)
        //                                ? Path.GetFileNameWithoutExtension(normalizedPath)
        //                                : tag.Title;

        //                            string album = string.IsNullOrWhiteSpace(tag.Album)
        //                                ? "Unknown Album"
        //                                : tag.Album;

        //                            string genre = string.IsNullOrWhiteSpace(string.Join(", ", tag.Genres)) ? "" : string.Join(", ", tag.Genres);

        //                            var newSong = new AudioTrackLite
        //                            {
        //                                FilePath = normalizedPath,
        //                                Title = title,
        //                                Artist = artist,
        //                                AlbumName = album,
        //                                SongDuration = tagFile.Properties.Duration,
        //                                Genre = genre
        //                            };

        //                            newlyDiscoveredSongs.Add(newSong);
        //                            existingPaths.Add(normalizedPath);

        //                            if (newlyDiscoveredSongs.Count % 15 == 0 && onSongDiscovered != null)
        //                            {
        //                                var batch = newlyDiscoveredSongs.TakeLast(15).ToList();

        //                                if (dispatcher != null)
        //                                {
        //                                    dispatcher.TryEnqueue(() =>
        //                                    {
        //                                        foreach (var song in batch) onSongDiscovered(song);
        //                                    });
        //                                }
        //                                else
        //                                {
        //                                    foreach (var song in batch) onSongDiscovered(song);
        //                                }
        //                            }
        //                        }
        //                        catch (TagLib.UnsupportedFormatException) { }
        //                        catch (TagLib.CorruptFileException) { }
        //                        catch (Exception ex)
        //                        {
        //                            Debug.WriteLine($"TagLib error for {normalizedPath}: {ex.Message}");
        //                        }

        //                        filesProcessedThisRun++;
        //                        if (filesProcessedThisRun % 10 == 0)
        //                        {
        //                            await Task.Delay(5);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Debug.WriteLine($"Directory scan error for {path}: {ex.Message}");
        //                }
        //            }

        //            int remainingCount = newlyDiscoveredSongs.Count % 15;
        //            if (remainingCount > 0 && onSongDiscovered != null)
        //            {
        //                var finalBatch = newlyDiscoveredSongs.TakeLast(remainingCount).ToList();
        //                if (dispatcher != null)
        //                {
        //                    dispatcher.TryEnqueue(() =>
        //                    {
        //                        foreach (var song in finalBatch) onSongDiscovered(song);
        //                    });
        //                }
        //                else
        //                {
        //                    foreach (var song in finalBatch) onSongDiscovered(song);
        //                }
        //            }

        //            if (newlyDiscoveredSongs.Count > 0)
        //            {
        //                SaveSongs(newlyDiscoveredSongs);
        //            }
        //        });
        //    }
        public static void SaveSongs(IEnumerable<AudioTrackLite> songs)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            string insertQuery = @"
            INSERT OR REPLACE INTO Songs (FilePath, Title, Artist, AlbumName, Genre, DurationTicks, IsFavourite, LastModified)
            VALUES ($filePath, $title, $artist, $album, $genre, $duration, $isFav, $lastModified);";

            using var command = connection.CreateCommand();
            command.CommandText = insertQuery;

            var pPath = command.Parameters.Add("$filePath", SqliteType.Text);
            var pTitle = command.Parameters.Add("$title", SqliteType.Text);
            var pArtist = command.Parameters.Add("$artist", SqliteType.Text);
            var pAlbum = command.Parameters.Add("$album", SqliteType.Text);
            var pGenre = command.Parameters.Add("$genre", SqliteType.Text);
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
                pGenre.Value = song.Genre ?? "";
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
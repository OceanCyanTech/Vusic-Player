using Microsoft.Data.Sqlite;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        /// <summary>
        /// Loads all cached tracks from SQLite into memory as AudioTrackLite (~10-30ms)
        /// </summary>
        public static List<AudioTrackLite> GetAllSongs()
        {
            // REMOVED: Process.Start("explorer.exe", ...) - This was opening File Explorer!

            var songs = new List<AudioTrackLite>();
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string selectQuery = "SELECT FilePath, Title, Artist, AlbumName, DurationTicks, IsFavourite FROM Songs";
            using var command = new SqliteCommand(selectQuery, connection);
            using var reader = command.ExecuteReader();

            // Cache column ordinals to speed up the loop
            int colFilePath = reader.GetOrdinal("FilePath");
            int colTitle = reader.GetOrdinal("Title");
            int colArtist = reader.GetOrdinal("Artist");
            int colAlbum = reader.GetOrdinal("AlbumName");
            int colDuration = reader.GetOrdinal("DurationTicks");
            int colFav = reader.GetOrdinal("IsFavourite");

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
                    IsFavourite = !reader.IsDBNull(colFav) && reader.GetInt32(colFav) == 1
                });
            }

            return songs;
        }
        private static readonly string[] SearchPaths =
    {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos,
        UserDataPaths.GetDefault().Pictures
    };
        public static async Task ScanAndSyncDiskAsync(
        HashSet<string> existingPaths,
        DispatcherQueue? dispatcher= null,
        Action<AudioTrackLite>? onSongDiscovered = null)
        {
            await Task.Run(() =>
            {
                var newlyDiscoveredSongs = new List<AudioTrackLite>();

                foreach (var path in SearchPaths)
                {
                    if (!Directory.Exists(path)) continue;

                    try
                    {
                        var directoryInfo = new DirectoryInfo(path);
                        var files = directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories)
                            .Where(f => AudioExtensions.List.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

                        foreach (var file in files)
                        {
                            string normalizedPath = Path.GetFullPath(file.FullName);

                            // Skip files that are already cached or loaded
                            if (existingPaths.Contains(normalizedPath)) continue;

                            try
                            {
                                // Use standard FileStream to bypass WinRT COM restrictions and file locks
                                using var stream = new FileStream(
                                    normalizedPath,
                                    FileMode.Open,
                                    FileAccess.Read,
                                    FileShare.ReadWrite);

                                // Tell TagLib to parse using the read-only stream abstraction
                                var abstraction = new SimpleStreamAbstraction(normalizedPath, stream)
                                {
                                    // Force WriteStream to null so TagLib knows this is strictly read-only
                                };
                                using var tagFile = TagLib.File.Create(abstraction);
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
                                    SongDuration = tagFile.Properties.Duration,
                                    //      Glyph = "\uEC4F"
                                };

                                newlyDiscoveredSongs.Add(newSong);
                                existingPaths.Add(normalizedPath);

                                if (newlyDiscoveredSongs.Count % 15 == 0 && dispatcher != null && onSongDiscovered != null)
                                {
                                    var batch = newlyDiscoveredSongs.TakeLast(15).ToList();
                                    dispatcher.TryEnqueue(() =>
                                    {
                                        foreach (var song in batch)
                                        {
                                            onSongDiscovered(song);
                                        }
                                    });
                                }
                            }
                            catch (TagLib.UnsupportedFormatException)
                            {
                                // Expected for files without metadata containers (e.g. raw .pcm, unsupported codecs)
                            }
                            catch (TagLib.CorruptFileException)
                            {
                                // Expected for corrupted or incomplete audio downloads
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine($"TagLib error for {normalizedPath}: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Directory scan error for {path}: {ex.Message}");
                    }
                }
                int remainingCount = newlyDiscoveredSongs.Count % 15;
                if (remainingCount > 0 && dispatcher != null && onSongDiscovered != null)
                {
                    var finalBatch = newlyDiscoveredSongs.TakeLast(remainingCount).ToList();
                    dispatcher.TryEnqueue(() =>
                    {
                        foreach (var song in finalBatch)
                        {
                            onSongDiscovered(song);
                        }
                    });
                }
                // Save all newly scanned tracks to SQLite in one batch transaction
                if (newlyDiscoveredSongs.Count > 0)
                {
                    SaveSongs(newlyDiscoveredSongs);
                }
            });
        }
        /// <summary>
        /// Saves newly scanned tracks to SQLite in a single ultra-fast transaction.
        /// </summary>
        public static void SaveSongs(IEnumerable<AudioTrackLite> songs)
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            string insertQuery = @"
            INSERT OR REPLACE INTO Songs (FilePath, Title, Artist, AlbumName, DurationTicks, IsFavourite)
            VALUES ($filePath, $title, $artist, $album, $duration, $isFav);";

            using var command = connection.CreateCommand();
            command.CommandText = insertQuery;

            var pPath = command.Parameters.Add("$filePath", SqliteType.Text);
            var pTitle = command.Parameters.Add("$title", SqliteType.Text);
            var pArtist = command.Parameters.Add("$artist", SqliteType.Text);
            var pAlbum = command.Parameters.Add("$album", SqliteType.Text);
            var pDuration = command.Parameters.Add("$duration", SqliteType.Integer);
            var pFav = command.Parameters.Add("$isFav", SqliteType.Integer);

            foreach (var song in songs)
            {
                pPath.Value = song.FilePath;
                pTitle.Value = song.Title ?? "";
                pArtist.Value = song.Artist ?? "Unknown Artist";
                pAlbum.Value = song.AlbumName ?? "Unknown Album";
                // Change this line in SaveSongs:
                pDuration.Value = song.SongDuration.HasValue
                    ? song.SongDuration.Value.Ticks
                    : 0L; // Or (object)DBNull.Value                pFav.Value = song.IsFavourite ? 1 : 0;

                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
        public static void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS Songs (
                FilePath TEXT PRIMARY KEY,
                Title TEXT,
                Artist TEXT,
                AlbumName TEXT,
                DurationTicks INTEGER,
                IsFavourite INTEGER
            );

            -- Index for lightning-fast artist searches
            CREATE INDEX IF NOT EXISTS idx_artist ON Songs(Artist);
        ";

            using var command = new SqliteCommand(createTableQuery, connection);
            command.ExecuteNonQuery();
        }
    }
}

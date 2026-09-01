using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Extensions;
using Vusic_Player.Pages.Views;

namespace Vusic_Player.Configuration.Helper.FileSystem
{

    public class MasterSearchIndex
    {
        public enum Filters
        {
            All,
            Music,
            Videos,
            Artist,
            Playlist,
            Album,
            Settings,
            Pages,
            Playlists,
            Shows,
            Genres
        }
        public static ObservableCollection<PlaylistItem> PlaylistsMaster = new ObservableCollection<PlaylistItem>();
        public static ObservableCollection<Show> ShowsMaster = new ObservableCollection<Show>();
        public static ObservableCollection<FolderModel> FoldersOpenedMaster = new ObservableCollection<FolderModel>();
        public static ObservableCollection<string> Pages = new ObservableCollection<string>();

        //public class EntityComparer : IEqualityComparer<(ClassModels.Filters Filter, string Name)>
        //{
        //    public bool Equals((ClassModels.Filters Filter, string Name) x, (ClassModels.Filters Filter, string Name) y)
        //    {
        //        return x.Filter == y.Filter && string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        //    }

        //    public int GetHashCode((ClassModels.Filters Filter, string Name) obj)
        //    {
        //        return HashCode.Combine(obj.Filter, obj.Name?.ToLowerInvariant());
        //    }
        //}
        //public static void TestMethod(string query, Filters filters = Filters.All, int ResultCount = 5, bool AllResults = false)
        //{
        //    ObservableCollection<MasterSearchModel> SearchResultsMain = new();
        //    var rawMedia = FilesInDatabase.rawSongs;
        //    string cleanQuery = query.Trim();
        //    if (string.IsNullOrWhiteSpace(cleanQuery)) return;
        //    Debug.WriteLine("Method Called");
        //    var splitquery = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        //    for (int i = 0; i <= splitquery.Length; i++)
        //    {
        //        var wordmatchesmedia = rawMedia.Where(p => p.Title.Length > 2 && p.Title.Contains(splitquery[i], StringComparison.OrdinalIgnoreCase) || splitquery[i].Contains(p.Title, StringComparison.OrdinalIgnoreCase));
        //        foreach (var item in wordmatchesmedia)
        //        {
        //            Debug.WriteLine("Matching media paths: " + item.FilePath);
        //        }
        //    }
        //    //  var exactTitleMatches = rawMedia
        //    //         .Where(p => string.Equals(cleanQuery, p.Title, StringComparison.OrdinalIgnoreCase))
        //    //         .ToList();
        //    //  var partialTitleMatches = rawMedia
        //    //.Where(p => p.Title.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase) || cleanQuery.Contains(p.Title, StringComparison.OrdinalIgnoreCase) || cleanQuery.StartsWith(p.Title))
        //    //.ToList();
        //    //  if (exactTitleMatches.Count > 0)
        //    //  {
        //    //      Debug.WriteLine($"Found exact title matches(testm): {cleanQuery}");
        //    //      var matchesToTake = AllResults ? exactTitleMatches : exactTitleMatches.Take(ResultCount);
        //    //      var firstitem = exactTitleMatches[0];

        //    //      foreach (var item in matchesToTake)
        //    //      {
        //    //          bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
        //    //          if (File.Exists(item.FilePath))
        //    //          {
        //    //              SearchResultsMain.Add(new MasterSearchModel
        //    //              {
        //    //                  FilePath = item.FilePath,
        //    //                  ResultMain = item.Title,
        //    //                  SubInformation = isVideo ? "Video" : "Song",
        //    //                  ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
        //    //              });
        //    //          }
        //    //      }
        //    //  }
        //    //  else if (partialTitleMatches.Count > 0)
        //    //  {
        //    //      Debug.WriteLine($"Found partial title matches(testm): {cleanQuery}");
        //    //      var matchesToTake = AllResults ? partialTitleMatches : partialTitleMatches.Take(ResultCount);
        //    //      var firstitem = partialTitleMatches[0];

        //    //      foreach (var item in matchesToTake)
        //    //      {
        //    //          bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
        //    //          if (File.Exists(item.FilePath))
        //    //          {
        //    //              SearchResultsMain.Add(new MasterSearchModel
        //    //              {
        //    //                  FilePath = item.FilePath,
        //    //                  ResultMain = item.Title,
        //    //                  SubInformation = isVideo ? "Video" : "Song",
        //    //                  ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
        //    //              });
        //    //          }
        //    //      }
        //    //      cleanQuery = cleanQuery.Replace(firstitem.Title, "", StringComparison.OrdinalIgnoreCase);

        //    //  }
        //    //  if (!string.IsNullOrWhiteSpace(cleanQuery))
        //    //  {
        //    //      var exactArtistMatches = rawMedia
        //    //       .Where(p => string.Equals(cleanQuery, p.Artist, StringComparison.OrdinalIgnoreCase))
        //    //       .ToList();
        //    //      Debug.WriteLine("Clean Query Further Evaluation: " + cleanQuery);
        //    //  }
        //    //  //  First check if any media matches Title, Artist, Album, etc...


        //}
        //private static int CalculateRelevanceScore(AudioTrackLite media, string firstToken, string remainingQuery, string fullQuery)
        //{
        //    bool hasRemaining = !string.IsNullOrEmpty(remainingQuery);
        //   // Debug.WriteLine(remainingQuery + "  Remaining Query");
        //    bool firstMatchesTitle = media.Title?.Contains(firstToken, StringComparison.OrdinalIgnoreCase) ?? false;
        //    bool firstMatchesArtistOrAlbum = (media.Artist?.Contains(firstToken, StringComparison.OrdinalIgnoreCase) ?? false) ||
        //                                     (media.AlbumName?.Contains(firstToken, StringComparison.OrdinalIgnoreCase) ?? false);

        //    bool remainingMatchesArtist = hasRemaining && (media.Artist?.Contains(remainingQuery, StringComparison.OrdinalIgnoreCase) ?? false);
        //    bool remainingMatchesAlbum = hasRemaining && (media.AlbumName?.Contains(remainingQuery, StringComparison.OrdinalIgnoreCase) ?? false);
        //    bool remainingMatchesTitle = hasRemaining && (media.Title?.Contains(remainingQuery, StringComparison.OrdinalIgnoreCase) ?? false);

        //    // Rule 1: First word matches Title
        //    if (firstMatchesTitle)
        //    {
        //        if (remainingMatchesArtist) return 6; // Title + Artist (Highest)
        //        if (remainingMatchesAlbum) return 5; // Title + Album
        //        return 3;                             // Title Only (Single-word or no artist match)
        //    }

        //    // Rule 2: First word matches Artist/Album AND remaining words match Title
        //    if (firstMatchesArtistOrAlbum && remainingMatchesTitle)
        //        return 2;

        //    // Rule 3: Full query fallback match
        //    if ((media.Title?.Contains(fullQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
        //        (media.Artist?.Contains(fullQuery, StringComparison.OrdinalIgnoreCase) ?? false))
        //        return 1;

        //    return 0;
        //}
        private static string NormalizeForSearch(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Fast span/array strip of spaces and punctuation
            return string.Create(input.Length, input, (span, src) =>
            {
                int index = 0;
                foreach (char c in src)
                {
                    if (!char.IsWhiteSpace(c) && !char.IsPunctuation(c))
                    {
                        span[index++] = c;
                    }
                }
            }).TrimEnd('\0');
        }
        /*Score Ranking
         Titles: 
        -Exact Match - 100
        -Starts With - 70
        -Contains - 40

        Artist: 
        -Exact Match - 95
        -Starts With - 65
        -Contains -35

        Album:
        -Exact Match - 90
        -Starts With - 60
        -Contains - 30

        Playlists:
        -Exact Match - 85
        -Starts With - 55
        -Contains - 25

        Shows:
        -Exact Match - 83
        -Starts With - 53
        -Contains - 23

         Genres:
        -Exact Match - 80
        -Starts With - 50
        -Contains - 20

         Folders Opened:
        -Exact Match - 75
        -Starts With - 45
        -Contains - 15

         Pages Opened:
        -Exact Match - 70
        -Starts With - 40
        -Contains - 10

        Settings:
        -Exact Match - 65
        -Starts With - 35
        -Contains - 5
        */
        private static IEnumerable<(MasterSearchModel, int)> SearchTracks(string query, string cleanQuery, string[] tokens)
        {
            foreach (var track in FilesInDatabase.rawSongs)
            {
                int score = 0;
                if (string.Equals(track.NormalizedTitle, cleanQuery, StringComparison.OrdinalIgnoreCase)) score = 100;
                else if (track.NormalizedTitle.StartsWith(cleanQuery, StringComparison.OrdinalIgnoreCase)) score = 60;
                else if (track.NormalizedTitle.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase)) score = 30;

                // Boost score slightly if track has high play counts
                //if (score > 0 && track.PlayCount > 0)
                //{
                //    score += Math.Min(track.PlayCount, 5); // cap play count boost
                //}

                if (score > 0)


                {
                    //Debug.WriteLine("Search Result Score: " + score);
                    bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(track.FilePath).ToLowerInvariant());
                    yield return (new MasterSearchModel
                    {
                        ResultMain = track.Title,
                        FilePath = track.FilePath,
                        SubInformation = isVideo ? "Video" : "Song",
                        ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
                    }, score);
                }
            }
        }
        //+1 Bonus
        private static IEnumerable<(MasterSearchModel, int)> SearchAlbums(string rawQuery, string[] tokens)
        {
            var distinctAlbums = FilesInDatabase.rawSongs
        .Where(s => !string.IsNullOrWhiteSpace(s.AlbumName) && s.AlbumName != "Unknown Album")
        .Select(s => s.AlbumName)
        .Distinct(StringComparer.OrdinalIgnoreCase);
            
            foreach (var album in distinctAlbums)
            {
                int score = 0;
                if (string.Equals(album, rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 100;
                }
                else if (album.StartsWith(rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 70;
                }
                // All individual tokens match somewhere in the album name (Score: 50) -> e.g. "Conan Gray" in "Conan Lee Gray"
                else if (tokens.Length > 0 && tokens.All(t => album.Contains(t, StringComparison.OrdinalIgnoreCase)))
                {
                    score = 50;
                }

              
                if (score > 0)
                {
                    yield return (new MasterSearchModel
                    {
                        ResultMain = album,
                        Album = album,
                        SubInformation = "Album",
                        ImageThumbnail = $"ms-appx:///Assets/defaultalbum.png"
                    }, score + 1);
                }
            }
        }
     
        //+2 Bonus
        private static IEnumerable<(MasterSearchModel, int)> SearchShows(string rawQuery, string[] tokens)
        {

            foreach (var show in ShowsMaster)
            {
                int score = 0;
                if (string.Equals(show.Name, rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 100;
                }
                else if (show.Name.StartsWith(rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 70;
                }
                // All individual tokens match somewhere in the show name (Score: 50) -> e.g. "Conan Gray" in "Conan Lee Gray"
                else if (tokens.Length > 0 && tokens.All(t => show.Name.Contains(t, StringComparison.OrdinalIgnoreCase)))
                {
                    score = 50;
                }


                if (score > 0)
                {
                    yield return (new MasterSearchModel
                    {
                        ResultMain = show.Name,
                        ShowID = show.ShowID,
                        SubInformation = "Show",
                        ImageThumbnail = $"ms-appx:///Assets/appicon.png"
                    }, score + 2);
                }
            }
        }
        //+3 Bonus
        private static IEnumerable<(MasterSearchModel, int)> SearchPlaylists(string rawQuery, string[] tokens)
        {

            foreach (var playlist in PlaylistsMaster)
            {
                int score = 0;
                if (string.Equals(playlist.PlaylistName, rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 100;
                }
                else if (playlist.PlaylistName.StartsWith(rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 70;
                }
                // All individual tokens match somewhere in the playlist name (Score: 50) -> e.g. "Conan Gray" in "Conan Lee Gray"
                else if (tokens.Length > 0 && tokens.All(t => playlist.PlaylistName.Contains(t, StringComparison.OrdinalIgnoreCase)))
                {
                    score = 50;
                }


                if (score > 0)
                {
                    yield return (new MasterSearchModel
                    {
                        ResultMain = playlist.PlaylistName,
                        PlaylistID = playlist.PlaylistId,
                        SubInformation = "Playlist",
                        ImageThumbnail = $"ms-appx:///Assets/playlistdefaultdark.png"
                    }, score + 3);
                }
            }
        }
        //+4 Bonus
        private static IEnumerable<(MasterSearchModel, int)> SearchArtists(string rawQuery, string[] tokens)
        {
            var distinctArtists = FilesInDatabase.rawSongs
        .Where(s => !string.IsNullOrWhiteSpace(s.Artist) && s.Artist != "Unknown Artist")
        .Select(s => s.Artist)
        .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var artist in distinctArtists)
            {
                int score = 0;
                if (string.Equals(artist, rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 100;
                }
                else if (artist.StartsWith(rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 70;
                }
                // All individual tokens match somewhere in the artist name (Score: 50) -> e.g. "Conan Gray" in "Conan Lee Gray"
                else if (tokens.Length > 0 && tokens.All(t => artist.Contains(t, StringComparison.OrdinalIgnoreCase)))
                {
                    score = 50;
                }

                // Boost score slightly if track has high play counts
                //if (score > 0 && track.PlayCount > 0)
                //{
                //    score += Math.Min(track.PlayCount, 5); // cap play count boost
                //}

                if (score > 0)
                {
                    yield return (new MasterSearchModel
                    {
                        ResultMain = artist,
                        Artist = artist,
                        SubInformation = "Artist",
                        ImageThumbnail = $"ms-appx:///Assets/defaultartist.png"
                    }, score + 4);
                }
            }
        }
        private static IEnumerable<(MasterSearchModel, int)> SearchGenres(string rawQuery, string[] tokens)
        {
            var distinctgenres = FilesInDatabase.rawSongs
        .Where(s => !string.IsNullOrWhiteSpace(s.Genre) && s.Genre != "Unknown genre")
        .Select(s => s.Genre)
        .Distinct(StringComparer.OrdinalIgnoreCase);

            foreach (var genre in distinctgenres)
            {
                int score = 0;
                if (string.Equals(genre, rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 100;
                }
                else if (genre.StartsWith(rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 70;
                }
                // All individual tokens match somewhere in the genre name (Score: 50) -> e.g. "Conan Gray" in "Conan Lee Gray"
                else if (tokens.Length > 0 && tokens.All(t => genre.Contains(t, StringComparison.OrdinalIgnoreCase)))
                {
                    score = 50;
                }

                // Boost score slightly if track has high play counts
                //if (score > 0 && track.PlayCount > 0)
                //{
                //    score += Math.Min(track.PlayCount, 5); // cap play count boost
                //}

                if (score > 0)
                {
                    yield return (new MasterSearchModel
                    {
                        ResultMain = genre,
                        SubInformation = "Genre",
                        ImageThumbnail = $"ms-appx:///Assets/defaultgenre.png"
                    }, score );
                }
            }
        }
        //+6 Bonus
        private static IEnumerable<(MasterSearchModel, int)> SearchPages(string rawQuery, string[] tokens)
        {

            var pages = Pages;
            foreach (var page in pages)
            {
                int score = 0;
                if (string.Equals(page, rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 100;
                }
                else if (page.StartsWith(rawQuery, StringComparison.OrdinalIgnoreCase))
                {
                    score = 70;
                }
                // All individual tokens match somewhere in the page name (Score: 50) -> e.g. "Conan Gray" in "Conan Lee Gray"
                else if (tokens.Length > 0 && tokens.All(t => page.Contains(t, StringComparison.OrdinalIgnoreCase)))
                {
                    score = 50;
                }

                // Boost score slightly if track has high play counts
                //if (score > 0 && track.PlayCount > 0)
                //{
                //    score += Math.Min(track.PlayCount, 5); // cap play count boost
                //}

                if (score > 0)
                {
                    yield return (new MasterSearchModel
                    {
                        ResultMain = page,
                        SubInformation = "Page",
                        ImageThumbnail = $"ms-appx:///Assets/appicon.png"
                    }, score + 6);
                }
            }
        }
        public static async Task<List<MasterSearchModel>> ProcessFindQueryFullAsync(string query)
        {
            var cleanQuery = NormalizeForSearch(query.Trim());
            if (string.IsNullOrWhiteSpace(cleanQuery)) return new List<MasterSearchModel>();
            var queryTokens = query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return await Task.Run(() =>
            {
                var rawMedia = FilesInDatabase.rawSongs;
                var candidates = new List<(MasterSearchModel Model, int Score)>();
                candidates.AddRange(SearchTracks(query.Trim(), cleanQuery, queryTokens));
                candidates.AddRange(SearchArtists(query.Trim(), queryTokens));
                candidates.AddRange(SearchAlbums(query.Trim(), queryTokens));
                candidates.AddRange(SearchPlaylists(query.Trim(), queryTokens));
                candidates.AddRange(SearchShows(query.Trim(), queryTokens));
                candidates.AddRange(SearchGenres(query.Trim(), queryTokens));
                candidates.AddRange(SearchPages(query.Trim(), queryTokens));
                return candidates.OrderByDescending(x => x.Score).Select(x => x.Model).ToList();
            });
        }
        public static async Task<List<MasterSearchModel>> ProcessFindQueryAsync(string query)
        {
            var cleanQuery = NormalizeForSearch(query.Trim());
            if (string.IsNullOrWhiteSpace(cleanQuery)) return new List<MasterSearchModel>();
            var queryTokens = query.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            return await Task.Run(() =>
            {
                var rawMedia = FilesInDatabase.rawSongs;
                var candidates = new List<(MasterSearchModel Model, int Score)>();
                candidates.AddRange(SearchTracks(query.Trim(), cleanQuery, queryTokens));
                candidates.AddRange(SearchArtists(query.Trim(), queryTokens));
                candidates.AddRange(SearchAlbums(query.Trim(), queryTokens));
                candidates.AddRange(SearchPlaylists(query.Trim(), queryTokens));
                candidates.AddRange(SearchShows(query.Trim(), queryTokens));
                candidates.AddRange(SearchGenres(query.Trim(), queryTokens));
                return candidates.OrderByDescending(x => x.Score).Select(x => x.Model).Take(6).ToList();
                //               //First Result is Direct Match/Starts with (and preferred with most played)
                //               var firstResult = rawMedia.FirstOrDefault(p =>
                //    !string.IsNullOrEmpty(p.NormalizedTitle) && (
                //        string.Equals(p.NormalizedTitle, cleanQuery, StringComparison.OrdinalIgnoreCase) ||
                //        p.NormalizedTitle.StartsWith(cleanQuery, StringComparison.OrdinalIgnoreCase)
                //    )
                //);

                //               // 3. Safe Distinct Subsequent Matches (excluding firstResult)
                //               var nextTwoTitles = rawMedia
                //                   .Where(p => !string.IsNullOrEmpty(p.NormalizedTitle) &&
                //                               p != firstResult &&
                //                               p.NormalizedTitle.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase))
                //                   .Take(2)
                //                   .ToList();
                //          //     var queryTokens = query.Trim()
                //          //.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                //               var Artist = rawMedia.FirstOrDefault(p =>
                //                   !string.IsNullOrEmpty(p.Artist) &&
                //                   queryTokens.All(token => p.Artist.Contains(token, StringComparison.OrdinalIgnoreCase))
                //               );
                //               var Album = rawMedia.FirstOrDefault(p => !string.IsNullOrEmpty(p.AlbumName) && queryTokens.All(token => p.AlbumName.Contains(token, StringComparison.OrdinalIgnoreCase)));

                //               var rankedResults = new List<MasterSearchModel>();
                //               if (firstResult != null)
                //               {
                //                   //Adding of first result
                //                   bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(firstResult.FilePath).ToLowerInvariant());

                //                   rankedResults.Add(new MasterSearchModel { ResultMain = firstResult.Title, FilePath = firstResult.FilePath, SubInformation = isVideo ? "Video" : "Song", ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png" });
                //               }
                //               foreach (var item in nextTwoTitles)
                //               {
                //                   bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());

                //                   rankedResults.Add(new MasterSearchModel { ResultMain = item.Title, FilePath = item.FilePath, SubInformation = isVideo ? "Video" : "Song", ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png" });
                //               }
                //               if (Artist != null)
                //               {
                //                   Debug.WriteLine("ARTIST NOT NULL " + Artist.Artist + "  " + Artist.Title);
                //                   rankedResults.Add(new MasterSearchModel { ResultMain = Artist.Artist, SearchFilter = ClassModels.Filters.Artist, Artist = Artist.Artist, SubInformation = "Artist", ImageThumbnail = $"ms-appx:///Assets/defaultartist.png" });
                //               }
                //               if (Album != null)
                //               {
                //                   rankedResults.Add(new MasterSearchModel { ResultMain = Album.AlbumName, SearchFilter = ClassModels.Filters.Album, Album = Album.AlbumName, SubInformation = "Album", ImageThumbnail = $"ms-appx:///Assets/defaultalbum.png" });
                //               }
                //               return rankedResults;
            });
        }

        public static async Task<List<MasterSearchModel>> FindMediaAsync(string query)
        {
            string cleanQuery = NormalizeForSearch(query.Trim());
            if (string.IsNullOrWhiteSpace(cleanQuery)) return new List<MasterSearchModel>();

            return await Task.Run(() =>
            {
                var rawMedia = FilesInDatabase.rawSongs;

                //First Result is Direct Match/Starts with (and preferred with most played)
                var firstResult = rawMedia.FirstOrDefault(p =>
     !string.IsNullOrEmpty(p.NormalizedTitle) && (
         string.Equals(p.NormalizedTitle, cleanQuery, StringComparison.OrdinalIgnoreCase) ||
         p.NormalizedTitle.StartsWith(cleanQuery, StringComparison.OrdinalIgnoreCase)
     )
 );

                // 3. Safe Distinct Subsequent Matches (excluding firstResult)
                var nextTwoTitles = rawMedia
                    .Where(p => !string.IsNullOrEmpty(p.NormalizedTitle) &&
                                p != firstResult &&
                                p.NormalizedTitle.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase))
                    .Take(2)
                    .ToList();
                var queryTokens = query.Trim()
           .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var Artist = rawMedia.FirstOrDefault(p =>
                    !string.IsNullOrEmpty(p.Artist) &&
                    queryTokens.All(token => p.Artist.Contains(token, StringComparison.OrdinalIgnoreCase))
                );
                var Album = rawMedia.FirstOrDefault(p => !string.IsNullOrEmpty(p.AlbumName) && queryTokens.All(token => p.AlbumName.Contains(token, StringComparison.OrdinalIgnoreCase)));

                var rankedResults = new List<MasterSearchModel>();
                if (firstResult != null)
                {
                    //Adding of first result
                    bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(firstResult.FilePath).ToLowerInvariant());

                    rankedResults.Add(new MasterSearchModel { ResultMain = firstResult.Title, FilePath = firstResult.FilePath, SubInformation = isVideo ? "Video" : "Song", ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png" });
                }
                foreach (var item in nextTwoTitles)
                {
                    bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());

                    rankedResults.Add(new MasterSearchModel { ResultMain = item.Title, FilePath = item.FilePath, SubInformation = isVideo ? "Video" : "Song", ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png" });
                }
                if (Artist != null)
                {
                    Debug.WriteLine("ARTIST NOT NULL " + Artist.Artist + "  " + Artist.Title);
                    rankedResults.Add(new MasterSearchModel { ResultMain = Artist.Artist, SearchFilter = ClassModels.Filters.Artist, Artist = Artist.Artist, SubInformation = "Artist", ImageThumbnail = $"ms-appx:///Assets/defaultartist.png" });
                }
                if (Album != null)
                {
                    rankedResults.Add(new MasterSearchModel { ResultMain = Album.AlbumName, SearchFilter = ClassModels.Filters.Album, Album = Album.AlbumName, SubInformation = "Album", ImageThumbnail = $"ms-appx:///Assets/defaultalbum.png" });
                }
                return rankedResults;
            });
        }
        //public record SearchResult<T>(T Item, int Score);
        //public static async  Task<List<MasterSearchModel>> SearchMediaAsync(string query)
        //{
        //    string cleanQuery = query.Trim();
        //    if (string.IsNullOrWhiteSpace(cleanQuery)) return new List<MasterSearchModel>();

        //    return await Task.Run(() =>
        //    {
        //        var rawMedia = FilesInDatabase.rawSongs;
        //        var queryTokens = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //        var rankedResults = new List<MasterSearchModel>();

        //        // -------------------------------------------------------------
        //        // 1. Identify Artist / Album Matches Across the DB
        //        // -------------------------------------------------------------
        //        // Requiring token length > 1 prevents single-letter false matches like "C"
        //        string matchedArtist = rawMedia
        //            .Where(m => !string.IsNullOrWhiteSpace(m.Artist))
        //            .FirstOrDefault(m => queryTokens.Any(t => t.Length > 1 && m.Artist.Contains(t, StringComparison.OrdinalIgnoreCase)))?.Artist;

        //        string matchedAlbum = rawMedia
        //            .Where(m => !string.IsNullOrWhiteSpace(m.AlbumName))
        //            .FirstOrDefault(m => queryTokens.Any(t => t.Length > 1 && m.AlbumName.Contains(t, StringComparison.OrdinalIgnoreCase)))?.AlbumName;

        //        // -------------------------------------------------------------
        //        // 2. Filter & Rank Tracks (Requires ALL tokens to match for 2+ word queries)
        //        // -------------------------------------------------------------
        //        foreach (var media in rawMedia)
        //        {
        //            string title = media.Title ?? string.Empty;
        //            string artist = !string.IsNullOrWhiteSpace(media.Artist) ? media.Artist : (matchedArtist ?? string.Empty);

        //            // If query is multiple tokens, ALL tokens must match across Title/Artist combined
        //            bool isMatch = queryTokens.All(t =>
        //                title.Contains(t, StringComparison.OrdinalIgnoreCase) ||
        //                artist.Contains(t, StringComparison.OrdinalIgnoreCase)
        //            );

        //            if (isMatch)
        //            {
        //                bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(media.FilePath).ToLowerInvariant());
        //                string mediaType = isVideo ? "Video" : "Song";

        //                // Score higher if title matches the full query or first token
        //                int score = title.Contains(queryTokens[0], StringComparison.OrdinalIgnoreCase) ? 100 : 50;

        //                rankedResults.Add(new MasterSearchModel
        //                {
        //                    ResultMain = media.Title,
        //                    SubInformation = !string.IsNullOrEmpty(artist) ? $"{mediaType} • {artist}" : mediaType,
        //                    ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png",
        //                    Score = score,
        //                    FilePath = media.FilePath // Use unique key for DistinctBy below
        //                });
        //            }
        //        }

        //        bool titleDetected = rankedResults.Any();

        //        // -------------------------------------------------------------
        //        // 3. Add Dedicated Artist/Album Cards based on Priority Rules
        //        // -------------------------------------------------------------
        //        if (!string.IsNullOrEmpty(matchedArtist))
        //        {
        //            // If titles were detected, artist goes #2 (Score 80). If no titles, artist goes #1 (Score 150)
        //            int artistScore = titleDetected ? 80 : 150;

        //            rankedResults.Add(new MasterSearchModel
        //            {
        //                ResultMain = matchedArtist,
        //                SubInformation = "Artist",
        //                ImageThumbnail = "ms-appx:///Assets/default.png",
        //                Score = artistScore,
        //                FilePath = $"ARTIST_{matchedArtist}"
        //            });
        //        }

        //        if (!string.IsNullOrEmpty(matchedAlbum))
        //        {
        //            int albumScore = titleDetected ? 70 : 140;

        //            rankedResults.Add(new MasterSearchModel
        //            {
        //                ResultMain = matchedAlbum,
        //                SubInformation = "Album",
        //                ImageThumbnail = "ms-appx:///Assets/default.png",
        //                Score = albumScore,
        //                FilePath = $"ALBUM_{matchedAlbum}"
        //            });
        //        }

        //        // -------------------------------------------------------------
        //        // 4. Return Top 5 Results (Deduplicating by FilePath, NOT Title)
        //        // -------------------------------------------------------------
        //        return rankedResults
        //            .OrderByDescending(x => x.Score)
        //            .DistinctBy(x => x.FilePath) // Allows multiple tracks with the same Title!
        //            .Take(5)
        //            .ToList();
        //    });
        //}
        //public static async Task<List<MasterSearchModel>> GetSearchResults(string query, Filters filters = Filters.All, int ResultCount = 5, bool AllResults = false)
        //{
        //    ObservableCollection<AudioTrackLite> SearchResultsMain = new();

        //    string cleanQuery = query.Trim();
        //    if (string.IsNullOrWhiteSpace(cleanQuery)) return new List<MasterSearchModel>();
        //    return await Task.Run(() =>
        //    {
        //        var rawMedia = FilesInDatabase.rawSongs;
        //        var tokens = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //        string firstToken = tokens[0];
        //        string remainingQuery = string.Join(" ", tokens.Skip(1));

        //        var rankedResults = new List<MasterSearchModel>();
        //        foreach (var media in rawMedia)
        //        {
        //            int score = CalculateRelevanceScore(media, firstToken, remainingQuery, cleanQuery);
        //            if (score > 0)
        //            {
        //                bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(media.FilePath).ToLowerInvariant());
        //                string mediaType = isVideo ? "Video" : "Song";

        //                string resultMain = media.Title;
        //                string subInfo = mediaType;
        //                string ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png";
        //                if (score == 6)
        //                {
        //                    Debug.WriteLine("YES artist: " + media.Artist);
        //                    // Matched Artist specifically
        //                    resultMain = media.Artist;
        //                    subInfo = $"Artist";
        //                }
        //                else if (score == 5)
        //                {
        //                    Debug.WriteLine("YES album: " + media.AlbumName);

        //                    // Matched Album specifically
        //                    resultMain = media.AlbumName;
        //                    subInfo = $"Album";
        //                }

        //                rankedResults.Add(new MasterSearchModel
        //                {
        //                    ResultMain = resultMain,
        //                    SubInformation = subInfo,
        //                    ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png",
        //                    Score = score
        //                });
        //            }
        //        }

        //        // Sort by highest score first
        //        return rankedResults
        //            .OrderByDescending(r => r.Score)
        //            .Take(5)
        //            .ToList();
        //    });
        //    //   if (filters == Filters.All)
        //    //   {
        //    //       Debug.WriteLine($"Processing Query: {query}");
        //    //       Debug.WriteLine($"Raw Media Count: {rawMedia.Count}");

        //    //       string cleanQuery = query.Trim();
        //    //       if (string.IsNullOrWhiteSpace(cleanQuery)) return SearchResultsMain;

        //    //       // 1. Exact Title Matches
        //    //       var exactTitleMatches = rawMedia
        //    //           .Where(p => string.Equals(cleanQuery, p.Title, StringComparison.OrdinalIgnoreCase))
        //    //           .ToList();
        //    //       var existingEntities = new HashSet<(ClassModels.Filters Filter, string Name)>(
        //    //    SearchResultsMain.Select(p => (p.SearchFilter, p.ResultMain)),
        //    //    new EntityComparer());

        //    //       if (exactTitleMatches.Count > 0)
        //    //       {
        //    //           Debug.WriteLine($"Found exact title matches: {cleanQuery}");
        //    //           var matchesToTake = AllResults ? exactTitleMatches : exactTitleMatches.Take(ResultCount);

        //    //           foreach (var item in matchesToTake)
        //    //           {
        //    //               bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
        //    //               if (File.Exists(item.FilePath))
        //    //               {
        //    //                   SearchResultsMain.Add(new MasterSearchModel
        //    //                   {
        //    //                       FilePath = item.FilePath,
        //    //                       ResultMain = item.Title,
        //    //                       SubInformation = isVideo ? "Video" : "Song",
        //    //                       ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
        //    //                   });
        //    //               }
        //    //           }
        //    //       }
        //    //       var fullArtistMatch = rawMedia.FirstOrDefault(p =>
        //    //!string.IsNullOrWhiteSpace(p.Artist) &&
        //    //p.Artist.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));

        //    //       if (fullArtistMatch != null)
        //    //       {
        //    //           if (existingEntities.Add((ClassModels.Filters.Artist, fullArtistMatch.Artist)))
        //    //           {
        //    //               SearchResultsMain.Add(new MasterSearchModel
        //    //               {
        //    //                   ResultMain = fullArtistMatch.Artist,
        //    //                   SubInformation = "Artist",
        //    //                   Artist = fullArtistMatch.Artist,
        //    //                   ImageThumbnail = "ms-appx:///Assets/defaultartist.png",
        //    //                   SearchFilter = ClassModels.Filters.Artist
        //    //               });
        //    //           }
        //    //       }

        //    //       // Full Phrase Album Match
        //    //       var fullAlbumMatch = rawMedia.FirstOrDefault(p =>
        //    //           !string.IsNullOrWhiteSpace(p.AlbumName) &&
        //    //           p.AlbumName.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));

        //    //       if (fullAlbumMatch != null)
        //    //       {
        //    //           if (existingEntities.Add((ClassModels.Filters.Album, fullAlbumMatch.AlbumName)))
        //    //           {
        //    //               SearchResultsMain.Add(new MasterSearchModel
        //    //               {
        //    //                   ResultMain = fullAlbumMatch.AlbumName,
        //    //                   SubInformation = "Album",
        //    //                   Album = fullAlbumMatch.AlbumName,
        //    //                   ImageThumbnail = "ms-appx:///Assets/defaultalbum.png",
        //    //                   SearchFilter = ClassModels.Filters.Album
        //    //               });
        //    //           }
        //    //       }
        //    //       var existingFilePaths = new HashSet<string>(
        //    //       SearchResultsMain.Where(p => p.FilePath != null).Select(p => p.FilePath),
        //    //       StringComparer.OrdinalIgnoreCase);

        //    //       Debug.WriteLine("Partial Parsing of title ");
        //    //       // 2. Partial Title Match & Token Parsing
        //    //       Debug.WriteLine("Partial Parsing of title ");
        //    //       var splitwords = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //    //       foreach (var word in splitwords)
        //    //       {
        //    //           Debug.WriteLine("Each word of Query " + word);

        //    //           // -------------------------------------------------------------------
        //    //           // 1. Match Media by Artist FIRST (Case-Insensitive)
        //    //           // -------------------------------------------------------------------
        //    //           var matchedArtistTrack = rawMedia.FirstOrDefault(p =>
        //    //               !string.IsNullOrWhiteSpace(p.Artist) &&
        //    //               p.Artist.Contains(word, StringComparison.OrdinalIgnoreCase));

        //    //           if (matchedArtistTrack != null)
        //    //           {
        //    //               Debug.WriteLine("Artist " + matchedArtistTrack.Artist);

        //    //               if (existingEntities.Add((ClassModels.Filters.Artist, matchedArtistTrack.Artist)))
        //    //               {
        //    //                   SearchResultsMain.Add(new MasterSearchModel
        //    //                   {
        //    //                       ResultMain = matchedArtistTrack.Artist,
        //    //                       SubInformation = "Artist",
        //    //                       Artist = matchedArtistTrack.Artist,
        //    //                       ImageThumbnail = "ms-appx:///Assets/defaultartist.png",
        //    //                       SearchFilter = ClassModels.Filters.Artist
        //    //                   });
        //    //               }

        //    //               if (AllResults == true)
        //    //               {
        //    //                   var artistsongs = rawMedia
        //    //                       .Where(p => string.Equals(p.Artist, matchedArtistTrack.Artist, StringComparison.OrdinalIgnoreCase))
        //    //                       .Take(20);

        //    //                   foreach (var song in artistsongs)
        //    //                   {
        //    //                       if (!string.IsNullOrEmpty(song.FilePath) && existingFilePaths.Add(song.FilePath))
        //    //                       {
        //    //                           if (File.Exists(song.FilePath))
        //    //                           {
        //    //                               bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant());
        //    //                               SearchResultsMain.Add(new MasterSearchModel
        //    //                               {
        //    //                                   FilePath = song.FilePath,
        //    //                                   ResultMain = song.Title,
        //    //                                   SubInformation = isVideo ? "Video" : "Song",
        //    //                                   ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png",
        //    //                                   Artist = song.Artist,
        //    //                                   Album = song.AlbumName
        //    //                               });
        //    //                           }
        //    //                           else
        //    //                           {
        //    //                               existingFilePaths.Remove(song.FilePath);
        //    //                           }
        //    //                       }
        //    //                   }
        //    //               }
        //    //           }

        //    //           // -------------------------------------------------------------------
        //    //           // 2. Match Media by Album (Case-Insensitive)
        //    //           // -------------------------------------------------------------------
        //    //           var matchedAlbumTrack = rawMedia.FirstOrDefault(p =>
        //    //               !string.IsNullOrWhiteSpace(p.AlbumName) &&
        //    //               p.AlbumName.Contains(word, StringComparison.OrdinalIgnoreCase));

        //    //           if (matchedAlbumTrack != null)
        //    //           {
        //    //               if (existingEntities.Add((ClassModels.Filters.Album, matchedAlbumTrack.AlbumName)))
        //    //               {
        //    //                   SearchResultsMain.Add(new MasterSearchModel
        //    //                   {
        //    //                       ResultMain = matchedAlbumTrack.AlbumName,
        //    //                       SubInformation = "Album",
        //    //                       Album = matchedAlbumTrack.AlbumName,
        //    //                       ImageThumbnail = "ms-appx:///Assets/defaultalbum.png",
        //    //                       SearchFilter = ClassModels.Filters.Album
        //    //                   });
        //    //               }
        //    //           }

        //    //           // -------------------------------------------------------------------
        //    //           // 3. Match Playlists, Shows, Folders & Pages
        //    //           // -------------------------------------------------------------------
        //    //           var matchedPlaylist = PlaylistsMaster.FirstOrDefault(p =>
        //    //               !string.IsNullOrWhiteSpace(p.PlaylistName) &&
        //    //               p.PlaylistName.Contains(word, StringComparison.OrdinalIgnoreCase));

        //    //           if (matchedPlaylist != null && existingEntities.Add((ClassModels.Filters.Playlist, matchedPlaylist.PlaylistName)))
        //    //           {
        //    //               SearchResultsMain.Add(new MasterSearchModel
        //    //               {
        //    //                   ResultMain = matchedPlaylist.PlaylistName,
        //    //                   SubInformation = "Playlist",
        //    //                   PlaylistID = matchedPlaylist.PlaylistId,
        //    //                   ImageThumbnail = "ms-appx:///Assets/playlistdefaultdark.png",
        //    //                   SearchFilter = ClassModels.Filters.Playlist
        //    //               });
        //    //           }

        //    //           var matchedShow = ShowsMaster.FirstOrDefault(p =>
        //    //               !string.IsNullOrWhiteSpace(p.Name) &&
        //    //               p.Name.Contains(word, StringComparison.OrdinalIgnoreCase));

        //    //           if (matchedShow != null && existingEntities.Add((ClassModels.Filters.Shows, matchedShow.Name)))
        //    //           {
        //    //               SearchResultsMain.Add(new MasterSearchModel
        //    //               {
        //    //                   ResultMain = matchedShow.Name,
        //    //                   SubInformation = "Show",
        //    //                   ShowID = matchedShow.ShowID,
        //    //                   ImageThumbnail = "ms-appx:///Assets/appicon.png",
        //    //                   SearchFilter = ClassModels.Filters.Shows
        //    //               });
        //    //           }

        //    //           var matchedFolder = FoldersOpenedMaster.FirstOrDefault(p =>
        //    //               !string.IsNullOrWhiteSpace(p.FolderName) &&
        //    //               p.FolderName.Contains(word, StringComparison.OrdinalIgnoreCase));

        //    //           if (matchedFolder != null && existingEntities.Add((ClassModels.Filters.Folders, matchedFolder.FolderName)))
        //    //           {
        //    //               SearchResultsMain.Add(new MasterSearchModel
        //    //               {
        //    //                   ResultMain = matchedFolder.FolderName,
        //    //                   SubInformation = "Folder",
        //    //                   FolderPath = matchedFolder.FolderPath,
        //    //                   ImageThumbnail = "ms-appx:///Assets/foldericon.png",
        //    //                   SearchFilter = ClassModels.Filters.Folders
        //    //               });
        //    //           }

        //    //           var matchedPage = Pages.FirstOrDefault(p =>
        //    //               !string.IsNullOrWhiteSpace(p) &&
        //    //               p.Contains(word, StringComparison.OrdinalIgnoreCase));

        //    //           if (matchedPage != null && existingEntities.Add((ClassModels.Filters.Pages, matchedPage)))
        //    //           {
        //    //               SearchResultsMain.Add(new MasterSearchModel
        //    //               {
        //    //                   ResultMain = matchedPage,
        //    //                   SubInformation = "Page",
        //    //                   ImageThumbnail = "ms-appx:///Assets/appicon.png",
        //    //                   SearchFilter = ClassModels.Filters.Pages
        //    //               });
        //    //           }

        //    //           // -------------------------------------------------------------------
        //    //           // 4. Match Media by Title LAST (Case-Insensitive)
        //    //           // -------------------------------------------------------------------
        //    //           var matchedByWord = rawMedia
        //    //               .Where(p => !string.IsNullOrWhiteSpace(p.Title) &&
        //    //                           p.Title.Length > 2 &&
        //    //                           p.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
        //    //               .OrderBy(p =>
        //    //               {
        //    //                   if (p.Title.Equals(word, StringComparison.OrdinalIgnoreCase)) return 0;
        //    //                   if (p.Title.StartsWith(word, StringComparison.OrdinalIgnoreCase)) return 1;
        //    //                   if (p.Title.Contains($" {word}", StringComparison.OrdinalIgnoreCase)) return 2;
        //    //                   return 3;
        //    //               })
        //    //               .ThenBy(p => p.Title.Length);

        //    //           foreach (var matchedsong in matchedByWord)
        //    //           {
        //    //               if (string.IsNullOrEmpty(matchedsong.FilePath)) continue;

        //    //               if (existingFilePaths.Add(matchedsong.FilePath))
        //    //               {
        //    //                   if (File.Exists(matchedsong.FilePath))
        //    //                   {
        //    //                       bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(matchedsong.FilePath).ToLowerInvariant());
        //    //                       SearchResultsMain.Add(new MasterSearchModel
        //    //                       {
        //    //                           FilePath = matchedsong.FilePath,
        //    //                           ResultMain = matchedsong.Title,
        //    //                           SubInformation = isVideo ? "Video" : "Song",
        //    //                           ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
        //    //                       });
        //    //                   }
        //    //                   else
        //    //                   {
        //    //                       existingFilePaths.Remove(matchedsong.FilePath);
        //    //                   }
        //    //               }
        //    //           }
        //    //       }

        //    //   }
        //    //           return SearchResultsMain;
        //}
    }
}

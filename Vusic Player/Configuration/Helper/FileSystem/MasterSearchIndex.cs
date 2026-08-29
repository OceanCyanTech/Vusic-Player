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

        public class EntityComparer : IEqualityComparer<(ClassModels.Filters Filter, string Name)>
        {
            public bool Equals((ClassModels.Filters Filter, string Name) x, (ClassModels.Filters Filter, string Name) y)
            {
                return x.Filter == y.Filter && string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
            }

            public int GetHashCode((ClassModels.Filters Filter, string Name) obj)
            {
                return HashCode.Combine(obj.Filter, obj.Name?.ToLowerInvariant());
            }
        }
        public static void TestMethod(string query, Filters filters = Filters.All, int ResultCount = 5, bool AllResults = false)
        {
            ObservableCollection<MasterSearchModel> SearchResultsMain = new();
            var rawMedia = FilesInDatabase.rawSongs;
            string cleanQuery = query.Trim();
            if (string.IsNullOrWhiteSpace(cleanQuery)) return;
            Debug.WriteLine("Method Called");
            var splitquery = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i <= splitquery.Length; i++)
            {
                var wordmatchesmedia = rawMedia.Where(p => p.Title.Length > 2 && p.Title.Contains(splitquery[i], StringComparison.OrdinalIgnoreCase) || splitquery[i].Contains(p.Title, StringComparison.OrdinalIgnoreCase));
                foreach (var item in wordmatchesmedia)
                {
                    Debug.WriteLine("Matching media paths: " + item.FilePath);
                }
            }
            //  var exactTitleMatches = rawMedia
            //         .Where(p => string.Equals(cleanQuery, p.Title, StringComparison.OrdinalIgnoreCase))
            //         .ToList();
            //  var partialTitleMatches = rawMedia
            //.Where(p => p.Title.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase) || cleanQuery.Contains(p.Title, StringComparison.OrdinalIgnoreCase) || cleanQuery.StartsWith(p.Title))
            //.ToList();
            //  if (exactTitleMatches.Count > 0)
            //  {
            //      Debug.WriteLine($"Found exact title matches(testm): {cleanQuery}");
            //      var matchesToTake = AllResults ? exactTitleMatches : exactTitleMatches.Take(ResultCount);
            //      var firstitem = exactTitleMatches[0];

            //      foreach (var item in matchesToTake)
            //      {
            //          bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
            //          if (File.Exists(item.FilePath))
            //          {
            //              SearchResultsMain.Add(new MasterSearchModel
            //              {
            //                  FilePath = item.FilePath,
            //                  ResultMain = item.Title,
            //                  SubInformation = isVideo ? "Video" : "Song",
            //                  ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
            //              });
            //          }
            //      }
            //  }
            //  else if (partialTitleMatches.Count > 0)
            //  {
            //      Debug.WriteLine($"Found partial title matches(testm): {cleanQuery}");
            //      var matchesToTake = AllResults ? partialTitleMatches : partialTitleMatches.Take(ResultCount);
            //      var firstitem = partialTitleMatches[0];

            //      foreach (var item in matchesToTake)
            //      {
            //          bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
            //          if (File.Exists(item.FilePath))
            //          {
            //              SearchResultsMain.Add(new MasterSearchModel
            //              {
            //                  FilePath = item.FilePath,
            //                  ResultMain = item.Title,
            //                  SubInformation = isVideo ? "Video" : "Song",
            //                  ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
            //              });
            //          }
            //      }
            //      cleanQuery = cleanQuery.Replace(firstitem.Title, "", StringComparison.OrdinalIgnoreCase);

            //  }
            //  if (!string.IsNullOrWhiteSpace(cleanQuery))
            //  {
            //      var exactArtistMatches = rawMedia
            //       .Where(p => string.Equals(cleanQuery, p.Artist, StringComparison.OrdinalIgnoreCase))
            //       .ToList();
            //      Debug.WriteLine("Clean Query Further Evaluation: " + cleanQuery);
            //  }
            //  //  First check if any media matches Title, Artist, Album, etc...


        }
        private static int CalculateRelevanceScore(AudioTrackLite media, string firstToken, string remainingQuery, string fullQuery)
        {
            bool hasRemaining = !string.IsNullOrEmpty(remainingQuery);
           // Debug.WriteLine(remainingQuery + "  Remaining Query");
            bool firstMatchesTitle = media.Title?.Contains(firstToken, StringComparison.OrdinalIgnoreCase) ?? false;
            bool firstMatchesArtistOrAlbum = (media.Artist?.Contains(firstToken, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                             (media.AlbumName?.Contains(firstToken, StringComparison.OrdinalIgnoreCase) ?? false);

            bool remainingMatchesArtist = hasRemaining && (media.Artist?.Contains(remainingQuery, StringComparison.OrdinalIgnoreCase) ?? false);
            bool remainingMatchesAlbum = hasRemaining && (media.AlbumName?.Contains(remainingQuery, StringComparison.OrdinalIgnoreCase) ?? false);
            bool remainingMatchesTitle = hasRemaining && (media.Title?.Contains(remainingQuery, StringComparison.OrdinalIgnoreCase) ?? false);

            // Rule 1: First word matches Title
            if (firstMatchesTitle)
            {
                if (remainingMatchesArtist) return 6; // Title + Artist (Highest)
                if (remainingMatchesAlbum) return 5; // Title + Album
                return 3;                             // Title Only (Single-word or no artist match)
            }

            // Rule 2: First word matches Artist/Album AND remaining words match Title
            if (firstMatchesArtistOrAlbum && remainingMatchesTitle)
                return 2;

            // Rule 3: Full query fallback match
            if ((media.Title?.Contains(fullQuery, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (media.Artist?.Contains(fullQuery, StringComparison.OrdinalIgnoreCase) ?? false))
                return 1;

            return 0;
        }
        public static async Task<List<MasterSearchModel>>FindMediaAsync(string query)
        {
            string cleanQuery = query.Trim();
            if (string.IsNullOrWhiteSpace(cleanQuery)) return new List<MasterSearchModel>();

            return await Task.Run(() =>
            {
                var rawMedia = FilesInDatabase.rawSongs;

                //First Result is Direct Match/Starts with and preferred with most played
                var FirstResult = rawMedia.FirstOrDefault(p => p.Title.ToLower().Trim() == cleanQuery.ToLower() || cleanQuery.StartsWith(p.Title, StringComparison.OrdinalIgnoreCase));

                var NextTwoTitles = rawMedia.Where(p => p.Title.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase) || cleanQuery.Contains(p.Title, StringComparison.OrdinalIgnoreCase)).Take(2);

                var Artist = rawMedia.Where(p => cleanQuery.Contains(p.Artist, StringComparison.OrdinalIgnoreCase)).Take(1);
                var Album = rawMedia.Where(p => cleanQuery.Contains(p.AlbumName, StringComparison.OrdinalIgnoreCase)).Take(1);

                var rankedResults = new List<MasterSearchModel>();
                if (FirstResult != null)
                {
                    //Adding of first result
                    bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(FirstResult.FilePath).ToLowerInvariant());

                    rankedResults.Add(new MasterSearchModel { ResultMain = FirstResult.Title, FilePath = FirstResult.FilePath, SubInformation = isVideo ? "Video" : "Song",  ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png" });
                }
                return new List<MasterSearchModel>();
            });
            }
        public record SearchResult<T>(T Item, int Score);
        public static async  Task<List<MasterSearchModel>> SearchMediaAsync(string query)
        {
            string cleanQuery = query.Trim();
            if (string.IsNullOrWhiteSpace(cleanQuery)) return new List<MasterSearchModel>();

            return await Task.Run(() =>
            {
                var rawMedia = FilesInDatabase.rawSongs;
                var queryTokens = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                var rankedResults = new List<MasterSearchModel>();

                // -------------------------------------------------------------
                // 1. Identify Artist / Album Matches Across the DB
                // -------------------------------------------------------------
                // Requiring token length > 1 prevents single-letter false matches like "C"
                string matchedArtist = rawMedia
                    .Where(m => !string.IsNullOrWhiteSpace(m.Artist))
                    .FirstOrDefault(m => queryTokens.Any(t => t.Length > 1 && m.Artist.Contains(t, StringComparison.OrdinalIgnoreCase)))?.Artist;

                string matchedAlbum = rawMedia
                    .Where(m => !string.IsNullOrWhiteSpace(m.AlbumName))
                    .FirstOrDefault(m => queryTokens.Any(t => t.Length > 1 && m.AlbumName.Contains(t, StringComparison.OrdinalIgnoreCase)))?.AlbumName;

                // -------------------------------------------------------------
                // 2. Filter & Rank Tracks (Requires ALL tokens to match for 2+ word queries)
                // -------------------------------------------------------------
                foreach (var media in rawMedia)
                {
                    string title = media.Title ?? string.Empty;
                    string artist = !string.IsNullOrWhiteSpace(media.Artist) ? media.Artist : (matchedArtist ?? string.Empty);

                    // If query is multiple tokens, ALL tokens must match across Title/Artist combined
                    bool isMatch = queryTokens.All(t =>
                        title.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                        artist.Contains(t, StringComparison.OrdinalIgnoreCase)
                    );

                    if (isMatch)
                    {
                        bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(media.FilePath).ToLowerInvariant());
                        string mediaType = isVideo ? "Video" : "Song";

                        // Score higher if title matches the full query or first token
                        int score = title.Contains(queryTokens[0], StringComparison.OrdinalIgnoreCase) ? 100 : 50;

                        rankedResults.Add(new MasterSearchModel
                        {
                            ResultMain = media.Title,
                            SubInformation = !string.IsNullOrEmpty(artist) ? $"{mediaType} • {artist}" : mediaType,
                            ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png",
                            Score = score,
                            FilePath = media.FilePath // Use unique key for DistinctBy below
                        });
                    }
                }

                bool titleDetected = rankedResults.Any();

                // -------------------------------------------------------------
                // 3. Add Dedicated Artist/Album Cards based on Priority Rules
                // -------------------------------------------------------------
                if (!string.IsNullOrEmpty(matchedArtist))
                {
                    // If titles were detected, artist goes #2 (Score 80). If no titles, artist goes #1 (Score 150)
                    int artistScore = titleDetected ? 80 : 150;

                    rankedResults.Add(new MasterSearchModel
                    {
                        ResultMain = matchedArtist,
                        SubInformation = "Artist",
                        ImageThumbnail = "ms-appx:///Assets/default.png",
                        Score = artistScore,
                        FilePath = $"ARTIST_{matchedArtist}"
                    });
                }

                if (!string.IsNullOrEmpty(matchedAlbum))
                {
                    int albumScore = titleDetected ? 70 : 140;

                    rankedResults.Add(new MasterSearchModel
                    {
                        ResultMain = matchedAlbum,
                        SubInformation = "Album",
                        ImageThumbnail = "ms-appx:///Assets/default.png",
                        Score = albumScore,
                        FilePath = $"ALBUM_{matchedAlbum}"
                    });
                }

                // -------------------------------------------------------------
                // 4. Return Top 5 Results (Deduplicating by FilePath, NOT Title)
                // -------------------------------------------------------------
                return rankedResults
                    .OrderByDescending(x => x.Score)
                    .DistinctBy(x => x.FilePath) // Allows multiple tracks with the same Title!
                    .Take(5)
                    .ToList();
            });
        }
        public static async Task<List<MasterSearchModel>> GetSearchResults(string query, Filters filters = Filters.All, int ResultCount = 5, bool AllResults = false)
        {
            ObservableCollection<AudioTrackLite> SearchResultsMain = new();

            string cleanQuery = query.Trim();
            if (string.IsNullOrWhiteSpace(cleanQuery)) return new List<MasterSearchModel>();
            return await Task.Run(() =>
            {
                var rawMedia = FilesInDatabase.rawSongs;
                var tokens = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                string firstToken = tokens[0];
                string remainingQuery = string.Join(" ", tokens.Skip(1));

                var rankedResults = new List<MasterSearchModel>();
                foreach (var media in rawMedia)
                {
                    int score = CalculateRelevanceScore(media, firstToken, remainingQuery, cleanQuery);
                    if (score > 0)
                    {
                        bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(media.FilePath).ToLowerInvariant());
                        string mediaType = isVideo ? "Video" : "Song";

                        string resultMain = media.Title;
                        string subInfo = mediaType;
                        string ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png";
                        if (score == 6)
                        {
                            Debug.WriteLine("YES artist: " + media.Artist);
                            // Matched Artist specifically
                            resultMain = media.Artist;
                            subInfo = $"Artist";
                        }
                        else if (score == 5)
                        {
                            Debug.WriteLine("YES album: " + media.AlbumName);

                            // Matched Album specifically
                            resultMain = media.AlbumName;
                            subInfo = $"Album";
                        }

                        rankedResults.Add(new MasterSearchModel
                        {
                            ResultMain = resultMain,
                            SubInformation = subInfo,
                            ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png",
                            Score = score
                        });
                    }
                }

                // Sort by highest score first
                return rankedResults
                    .OrderByDescending(r => r.Score)
                    .Take(5)
                    .ToList();
            });
            //   if (filters == Filters.All)
            //   {
            //       Debug.WriteLine($"Processing Query: {query}");
            //       Debug.WriteLine($"Raw Media Count: {rawMedia.Count}");

            //       string cleanQuery = query.Trim();
            //       if (string.IsNullOrWhiteSpace(cleanQuery)) return SearchResultsMain;

            //       // 1. Exact Title Matches
            //       var exactTitleMatches = rawMedia
            //           .Where(p => string.Equals(cleanQuery, p.Title, StringComparison.OrdinalIgnoreCase))
            //           .ToList();
            //       var existingEntities = new HashSet<(ClassModels.Filters Filter, string Name)>(
            //    SearchResultsMain.Select(p => (p.SearchFilter, p.ResultMain)),
            //    new EntityComparer());

            //       if (exactTitleMatches.Count > 0)
            //       {
            //           Debug.WriteLine($"Found exact title matches: {cleanQuery}");
            //           var matchesToTake = AllResults ? exactTitleMatches : exactTitleMatches.Take(ResultCount);

            //           foreach (var item in matchesToTake)
            //           {
            //               bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
            //               if (File.Exists(item.FilePath))
            //               {
            //                   SearchResultsMain.Add(new MasterSearchModel
            //                   {
            //                       FilePath = item.FilePath,
            //                       ResultMain = item.Title,
            //                       SubInformation = isVideo ? "Video" : "Song",
            //                       ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
            //                   });
            //               }
            //           }
            //       }
            //       var fullArtistMatch = rawMedia.FirstOrDefault(p =>
            //!string.IsNullOrWhiteSpace(p.Artist) &&
            //p.Artist.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));

            //       if (fullArtistMatch != null)
            //       {
            //           if (existingEntities.Add((ClassModels.Filters.Artist, fullArtistMatch.Artist)))
            //           {
            //               SearchResultsMain.Add(new MasterSearchModel
            //               {
            //                   ResultMain = fullArtistMatch.Artist,
            //                   SubInformation = "Artist",
            //                   Artist = fullArtistMatch.Artist,
            //                   ImageThumbnail = "ms-appx:///Assets/defaultartist.png",
            //                   SearchFilter = ClassModels.Filters.Artist
            //               });
            //           }
            //       }

            //       // Full Phrase Album Match
            //       var fullAlbumMatch = rawMedia.FirstOrDefault(p =>
            //           !string.IsNullOrWhiteSpace(p.AlbumName) &&
            //           p.AlbumName.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));

            //       if (fullAlbumMatch != null)
            //       {
            //           if (existingEntities.Add((ClassModels.Filters.Album, fullAlbumMatch.AlbumName)))
            //           {
            //               SearchResultsMain.Add(new MasterSearchModel
            //               {
            //                   ResultMain = fullAlbumMatch.AlbumName,
            //                   SubInformation = "Album",
            //                   Album = fullAlbumMatch.AlbumName,
            //                   ImageThumbnail = "ms-appx:///Assets/defaultalbum.png",
            //                   SearchFilter = ClassModels.Filters.Album
            //               });
            //           }
            //       }
            //       var existingFilePaths = new HashSet<string>(
            //       SearchResultsMain.Where(p => p.FilePath != null).Select(p => p.FilePath),
            //       StringComparer.OrdinalIgnoreCase);

            //       Debug.WriteLine("Partial Parsing of title ");
            //       // 2. Partial Title Match & Token Parsing
            //       Debug.WriteLine("Partial Parsing of title ");
            //       var splitwords = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            //       foreach (var word in splitwords)
            //       {
            //           Debug.WriteLine("Each word of Query " + word);

            //           // -------------------------------------------------------------------
            //           // 1. Match Media by Artist FIRST (Case-Insensitive)
            //           // -------------------------------------------------------------------
            //           var matchedArtistTrack = rawMedia.FirstOrDefault(p =>
            //               !string.IsNullOrWhiteSpace(p.Artist) &&
            //               p.Artist.Contains(word, StringComparison.OrdinalIgnoreCase));

            //           if (matchedArtistTrack != null)
            //           {
            //               Debug.WriteLine("Artist " + matchedArtistTrack.Artist);

            //               if (existingEntities.Add((ClassModels.Filters.Artist, matchedArtistTrack.Artist)))
            //               {
            //                   SearchResultsMain.Add(new MasterSearchModel
            //                   {
            //                       ResultMain = matchedArtistTrack.Artist,
            //                       SubInformation = "Artist",
            //                       Artist = matchedArtistTrack.Artist,
            //                       ImageThumbnail = "ms-appx:///Assets/defaultartist.png",
            //                       SearchFilter = ClassModels.Filters.Artist
            //                   });
            //               }

            //               if (AllResults == true)
            //               {
            //                   var artistsongs = rawMedia
            //                       .Where(p => string.Equals(p.Artist, matchedArtistTrack.Artist, StringComparison.OrdinalIgnoreCase))
            //                       .Take(20);

            //                   foreach (var song in artistsongs)
            //                   {
            //                       if (!string.IsNullOrEmpty(song.FilePath) && existingFilePaths.Add(song.FilePath))
            //                       {
            //                           if (File.Exists(song.FilePath))
            //                           {
            //                               bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant());
            //                               SearchResultsMain.Add(new MasterSearchModel
            //                               {
            //                                   FilePath = song.FilePath,
            //                                   ResultMain = song.Title,
            //                                   SubInformation = isVideo ? "Video" : "Song",
            //                                   ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png",
            //                                   Artist = song.Artist,
            //                                   Album = song.AlbumName
            //                               });
            //                           }
            //                           else
            //                           {
            //                               existingFilePaths.Remove(song.FilePath);
            //                           }
            //                       }
            //                   }
            //               }
            //           }

            //           // -------------------------------------------------------------------
            //           // 2. Match Media by Album (Case-Insensitive)
            //           // -------------------------------------------------------------------
            //           var matchedAlbumTrack = rawMedia.FirstOrDefault(p =>
            //               !string.IsNullOrWhiteSpace(p.AlbumName) &&
            //               p.AlbumName.Contains(word, StringComparison.OrdinalIgnoreCase));

            //           if (matchedAlbumTrack != null)
            //           {
            //               if (existingEntities.Add((ClassModels.Filters.Album, matchedAlbumTrack.AlbumName)))
            //               {
            //                   SearchResultsMain.Add(new MasterSearchModel
            //                   {
            //                       ResultMain = matchedAlbumTrack.AlbumName,
            //                       SubInformation = "Album",
            //                       Album = matchedAlbumTrack.AlbumName,
            //                       ImageThumbnail = "ms-appx:///Assets/defaultalbum.png",
            //                       SearchFilter = ClassModels.Filters.Album
            //                   });
            //               }
            //           }

            //           // -------------------------------------------------------------------
            //           // 3. Match Playlists, Shows, Folders & Pages
            //           // -------------------------------------------------------------------
            //           var matchedPlaylist = PlaylistsMaster.FirstOrDefault(p =>
            //               !string.IsNullOrWhiteSpace(p.PlaylistName) &&
            //               p.PlaylistName.Contains(word, StringComparison.OrdinalIgnoreCase));

            //           if (matchedPlaylist != null && existingEntities.Add((ClassModels.Filters.Playlist, matchedPlaylist.PlaylistName)))
            //           {
            //               SearchResultsMain.Add(new MasterSearchModel
            //               {
            //                   ResultMain = matchedPlaylist.PlaylistName,
            //                   SubInformation = "Playlist",
            //                   PlaylistID = matchedPlaylist.PlaylistId,
            //                   ImageThumbnail = "ms-appx:///Assets/playlistdefaultdark.png",
            //                   SearchFilter = ClassModels.Filters.Playlist
            //               });
            //           }

            //           var matchedShow = ShowsMaster.FirstOrDefault(p =>
            //               !string.IsNullOrWhiteSpace(p.Name) &&
            //               p.Name.Contains(word, StringComparison.OrdinalIgnoreCase));

            //           if (matchedShow != null && existingEntities.Add((ClassModels.Filters.Shows, matchedShow.Name)))
            //           {
            //               SearchResultsMain.Add(new MasterSearchModel
            //               {
            //                   ResultMain = matchedShow.Name,
            //                   SubInformation = "Show",
            //                   ShowID = matchedShow.ShowID,
            //                   ImageThumbnail = "ms-appx:///Assets/appicon.png",
            //                   SearchFilter = ClassModels.Filters.Shows
            //               });
            //           }

            //           var matchedFolder = FoldersOpenedMaster.FirstOrDefault(p =>
            //               !string.IsNullOrWhiteSpace(p.FolderName) &&
            //               p.FolderName.Contains(word, StringComparison.OrdinalIgnoreCase));

            //           if (matchedFolder != null && existingEntities.Add((ClassModels.Filters.Folders, matchedFolder.FolderName)))
            //           {
            //               SearchResultsMain.Add(new MasterSearchModel
            //               {
            //                   ResultMain = matchedFolder.FolderName,
            //                   SubInformation = "Folder",
            //                   FolderPath = matchedFolder.FolderPath,
            //                   ImageThumbnail = "ms-appx:///Assets/foldericon.png",
            //                   SearchFilter = ClassModels.Filters.Folders
            //               });
            //           }

            //           var matchedPage = Pages.FirstOrDefault(p =>
            //               !string.IsNullOrWhiteSpace(p) &&
            //               p.Contains(word, StringComparison.OrdinalIgnoreCase));

            //           if (matchedPage != null && existingEntities.Add((ClassModels.Filters.Pages, matchedPage)))
            //           {
            //               SearchResultsMain.Add(new MasterSearchModel
            //               {
            //                   ResultMain = matchedPage,
            //                   SubInformation = "Page",
            //                   ImageThumbnail = "ms-appx:///Assets/appicon.png",
            //                   SearchFilter = ClassModels.Filters.Pages
            //               });
            //           }

            //           // -------------------------------------------------------------------
            //           // 4. Match Media by Title LAST (Case-Insensitive)
            //           // -------------------------------------------------------------------
            //           var matchedByWord = rawMedia
            //               .Where(p => !string.IsNullOrWhiteSpace(p.Title) &&
            //                           p.Title.Length > 2 &&
            //                           p.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
            //               .OrderBy(p =>
            //               {
            //                   if (p.Title.Equals(word, StringComparison.OrdinalIgnoreCase)) return 0;
            //                   if (p.Title.StartsWith(word, StringComparison.OrdinalIgnoreCase)) return 1;
            //                   if (p.Title.Contains($" {word}", StringComparison.OrdinalIgnoreCase)) return 2;
            //                   return 3;
            //               })
            //               .ThenBy(p => p.Title.Length);

            //           foreach (var matchedsong in matchedByWord)
            //           {
            //               if (string.IsNullOrEmpty(matchedsong.FilePath)) continue;

            //               if (existingFilePaths.Add(matchedsong.FilePath))
            //               {
            //                   if (File.Exists(matchedsong.FilePath))
            //                   {
            //                       bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(matchedsong.FilePath).ToLowerInvariant());
            //                       SearchResultsMain.Add(new MasterSearchModel
            //                       {
            //                           FilePath = matchedsong.FilePath,
            //                           ResultMain = matchedsong.Title,
            //                           SubInformation = isVideo ? "Video" : "Song",
            //                           ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
            //                       });
            //                   }
            //                   else
            //                   {
            //                       existingFilePaths.Remove(matchedsong.FilePath);
            //                   }
            //               }
            //           }
            //       }

            //   }
            //           return SearchResultsMain;
        }
    }
}

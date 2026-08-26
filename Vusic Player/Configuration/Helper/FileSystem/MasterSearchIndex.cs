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
        public static void TestMethod(Filters filters = Filters.All, int ResultCount = 5, bool AllResults = false)
        {
            ObservableCollection<MasterSearchModel> SearchResultsMain = new();
            var rawMedia = FilesInDatabase.rawSongs;
            string query = "";
            string cleanQuery = query.Trim();
            if (string.IsNullOrWhiteSpace(cleanQuery)) return;

            var exactTitleMatches = rawMedia
                   .Where(p => string.Equals(cleanQuery, p.Title, StringComparison.OrdinalIgnoreCase))
                   .ToList();

            if (exactTitleMatches.Count > 0)
            {
                Debug.WriteLine($"Found exact title matches: {cleanQuery}");
                var matchesToTake = AllResults ? exactTitleMatches : exactTitleMatches.Take(ResultCount);
                var firstitem = exactTitleMatches[0];
                
                foreach (var item in matchesToTake)
                {
                    bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
                    if (File.Exists(item.FilePath))
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            FilePath = item.FilePath,
                            ResultMain = item.Title,
                            SubInformation = isVideo ? "Video" : "Song",
                            ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
                        });
                    }
                }
                cleanQuery = cleanQuery.Replace(firstitem.Title, "");
            }
            if (!string.IsNullOrWhiteSpace(cleanQuery))
            {
                var exactArtistMatches = rawMedia
                 .Where(p => string.Equals(cleanQuery, p.Artist, StringComparison.OrdinalIgnoreCase))
                 .ToList();
            }
            //  First check if any media matches Title, Artist, Album, etc...


        }
        public static ObservableCollection<MasterSearchModel> GetSearchResults(string query, Filters filters = Filters.All, int ResultCount = 5, bool AllResults = false)
        {
            ObservableCollection<MasterSearchModel> SearchResultsMain = new();
            var rawMedia = FilesInDatabase.rawSongs;
            if (filters == Filters.All)
            {
                Debug.WriteLine($"Processing Query: {query}");
                Debug.WriteLine($"Raw Media Count: {rawMedia.Count}");

                string cleanQuery = query.Trim();
                if (string.IsNullOrWhiteSpace(cleanQuery)) return SearchResultsMain;

                // 1. Exact Title Matches
                var exactTitleMatches = rawMedia
                    .Where(p => string.Equals(cleanQuery, p.Title, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var existingEntities = new HashSet<(ClassModels.Filters Filter, string Name)>(
             SearchResultsMain.Select(p => (p.SearchFilter, p.ResultMain)),
             new EntityComparer());

                if (exactTitleMatches.Count > 0)
                {
                    Debug.WriteLine($"Found exact title matches: {cleanQuery}");
                    var matchesToTake = AllResults ? exactTitleMatches : exactTitleMatches.Take(ResultCount);

                    foreach (var item in matchesToTake)
                    {
                        bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
                        if (File.Exists(item.FilePath))
                        {
                            SearchResultsMain.Add(new MasterSearchModel
                            {
                                FilePath = item.FilePath,
                                ResultMain = item.Title,
                                SubInformation = isVideo ? "Video" : "Song",
                                ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
                            });
                        }
                    }
                }
                var fullArtistMatch = rawMedia.FirstOrDefault(p =>
         !string.IsNullOrWhiteSpace(p.Artist) &&
         p.Artist.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));

                if (fullArtistMatch != null)
                {
                    if (existingEntities.Add((ClassModels.Filters.Artist, fullArtistMatch.Artist)))
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            ResultMain = fullArtistMatch.Artist,
                            SubInformation = "Artist",
                            Artist = fullArtistMatch.Artist,
                            ImageThumbnail = "ms-appx:///Assets/defaultartist.png",
                            SearchFilter = ClassModels.Filters.Artist
                        });
                    }
                }

                // Full Phrase Album Match
                var fullAlbumMatch = rawMedia.FirstOrDefault(p =>
                    !string.IsNullOrWhiteSpace(p.AlbumName) &&
                    p.AlbumName.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));

                if (fullAlbumMatch != null)
                {
                    if (existingEntities.Add((ClassModels.Filters.Album, fullAlbumMatch.AlbumName)))
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            ResultMain = fullAlbumMatch.AlbumName,
                            SubInformation = "Album",
                            Album = fullAlbumMatch.AlbumName,
                            ImageThumbnail = "ms-appx:///Assets/defaultalbum.png",
                            SearchFilter = ClassModels.Filters.Album
                        });
                    }
                }
                var existingFilePaths = new HashSet<string>(
                SearchResultsMain.Where(p => p.FilePath != null).Select(p => p.FilePath),
                StringComparer.OrdinalIgnoreCase);

                Debug.WriteLine("Partial Parsing of title ");
                // 2. Partial Title Match & Token Parsing
                Debug.WriteLine("Partial Parsing of title ");
                var splitwords = cleanQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var word in splitwords)
                {
                    Debug.WriteLine("Each word of Query " + word);

                    // -------------------------------------------------------------------
                    // 1. Match Media by Artist FIRST (Case-Insensitive)
                    // -------------------------------------------------------------------
                    var matchedArtistTrack = rawMedia.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p.Artist) &&
                        p.Artist.Contains(word, StringComparison.OrdinalIgnoreCase));

                    if (matchedArtistTrack != null)
                    {
                        Debug.WriteLine("Artist " + matchedArtistTrack.Artist);

                        if (existingEntities.Add((ClassModels.Filters.Artist, matchedArtistTrack.Artist)))
                        {
                            SearchResultsMain.Add(new MasterSearchModel
                            {
                                ResultMain = matchedArtistTrack.Artist,
                                SubInformation = "Artist",
                                Artist = matchedArtistTrack.Artist,
                                ImageThumbnail = "ms-appx:///Assets/defaultartist.png",
                                SearchFilter = ClassModels.Filters.Artist
                            });
                        }

                        if (AllResults == true)
                        {
                            var artistsongs = rawMedia
                                .Where(p => string.Equals(p.Artist, matchedArtistTrack.Artist, StringComparison.OrdinalIgnoreCase))
                                .Take(20);

                            foreach (var song in artistsongs)
                            {
                                if (!string.IsNullOrEmpty(song.FilePath) && existingFilePaths.Add(song.FilePath))
                                {
                                    if (File.Exists(song.FilePath))
                                    {
                                        bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant());
                                        SearchResultsMain.Add(new MasterSearchModel
                                        {
                                            FilePath = song.FilePath,
                                            ResultMain = song.Title,
                                            SubInformation = isVideo ? "Video" : "Song",
                                            ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png",
                                            Artist = song.Artist,
                                            Album = song.AlbumName
                                        });
                                    }
                                    else
                                    {
                                        existingFilePaths.Remove(song.FilePath);
                                    }
                                }
                            }
                        }
                    }

                    // -------------------------------------------------------------------
                    // 2. Match Media by Album (Case-Insensitive)
                    // -------------------------------------------------------------------
                    var matchedAlbumTrack = rawMedia.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p.AlbumName) &&
                        p.AlbumName.Contains(word, StringComparison.OrdinalIgnoreCase));

                    if (matchedAlbumTrack != null)
                    {
                        if (existingEntities.Add((ClassModels.Filters.Album, matchedAlbumTrack.AlbumName)))
                        {
                            SearchResultsMain.Add(new MasterSearchModel
                            {
                                ResultMain = matchedAlbumTrack.AlbumName,
                                SubInformation = "Album",
                                Album = matchedAlbumTrack.AlbumName,
                                ImageThumbnail = "ms-appx:///Assets/defaultalbum.png",
                                SearchFilter = ClassModels.Filters.Album
                            });
                        }
                    }

                    // -------------------------------------------------------------------
                    // 3. Match Playlists, Shows, Folders & Pages
                    // -------------------------------------------------------------------
                    var matchedPlaylist = PlaylistsMaster.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p.PlaylistName) &&
                        p.PlaylistName.Contains(word, StringComparison.OrdinalIgnoreCase));

                    if (matchedPlaylist != null && existingEntities.Add((ClassModels.Filters.Playlist, matchedPlaylist.PlaylistName)))
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            ResultMain = matchedPlaylist.PlaylistName,
                            SubInformation = "Playlist",
                            PlaylistID = matchedPlaylist.PlaylistId,
                            ImageThumbnail = "ms-appx:///Assets/playlistdefaultdark.png",
                            SearchFilter = ClassModels.Filters.Playlist
                        });
                    }

                    var matchedShow = ShowsMaster.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p.Name) &&
                        p.Name.Contains(word, StringComparison.OrdinalIgnoreCase));

                    if (matchedShow != null && existingEntities.Add((ClassModels.Filters.Shows, matchedShow.Name)))
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            ResultMain = matchedShow.Name,
                            SubInformation = "Show",
                            ShowID = matchedShow.ShowID,
                            ImageThumbnail = "ms-appx:///Assets/appicon.png",
                            SearchFilter = ClassModels.Filters.Shows
                        });
                    }

                    var matchedFolder = FoldersOpenedMaster.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p.FolderName) &&
                        p.FolderName.Contains(word, StringComparison.OrdinalIgnoreCase));

                    if (matchedFolder != null && existingEntities.Add((ClassModels.Filters.Folders, matchedFolder.FolderName)))
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            ResultMain = matchedFolder.FolderName,
                            SubInformation = "Folder",
                            FolderPath = matchedFolder.FolderPath,
                            ImageThumbnail = "ms-appx:///Assets/foldericon.png",
                            SearchFilter = ClassModels.Filters.Folders
                        });
                    }

                    var matchedPage = Pages.FirstOrDefault(p =>
                        !string.IsNullOrWhiteSpace(p) &&
                        p.Contains(word, StringComparison.OrdinalIgnoreCase));

                    if (matchedPage != null && existingEntities.Add((ClassModels.Filters.Pages, matchedPage)))
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            ResultMain = matchedPage,
                            SubInformation = "Page",
                            ImageThumbnail = "ms-appx:///Assets/appicon.png",
                            SearchFilter = ClassModels.Filters.Pages
                        });
                    }

                    // -------------------------------------------------------------------
                    // 4. Match Media by Title LAST (Case-Insensitive)
                    // -------------------------------------------------------------------
                    var matchedByWord = rawMedia
                        .Where(p => !string.IsNullOrWhiteSpace(p.Title) &&
                                    p.Title.Length > 2 &&
                                    p.Title.Contains(word, StringComparison.OrdinalIgnoreCase))
                        .OrderBy(p =>
                        {
                            if (p.Title.Equals(word, StringComparison.OrdinalIgnoreCase)) return 0;
                            if (p.Title.StartsWith(word, StringComparison.OrdinalIgnoreCase)) return 1;
                            if (p.Title.Contains($" {word}", StringComparison.OrdinalIgnoreCase)) return 2;
                            return 3;
                        })
                        .ThenBy(p => p.Title.Length);

                    foreach (var matchedsong in matchedByWord)
                    {
                        if (string.IsNullOrEmpty(matchedsong.FilePath)) continue;

                        if (existingFilePaths.Add(matchedsong.FilePath))
                        {
                            if (File.Exists(matchedsong.FilePath))
                            {
                                bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(matchedsong.FilePath).ToLowerInvariant());
                                SearchResultsMain.Add(new MasterSearchModel
                                {
                                    FilePath = matchedsong.FilePath,
                                    ResultMain = matchedsong.Title,
                                    SubInformation = isVideo ? "Video" : "Song",
                                    ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
                                });
                            }
                            else
                            {
                                existingFilePaths.Remove(matchedsong.FilePath);
                            }
                        }
                    }
                }

            }
            return SearchResultsMain;
        }
    }
}

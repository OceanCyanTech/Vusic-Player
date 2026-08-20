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
        public static ObservableCollection<MasterSearchModel> GetSearchResults(string query, Filters filters = Filters.All, int ResultCount = 5, bool AllResults = false)
        {
            ObservableCollection<MasterSearchModel> SearchResultsMain = new();
            var rawMedia = FilesInDatabase.rawSongs;
            if (filters == Filters.All)
            {
                Debug.WriteLine("Processing Query: " + query);
                Debug.WriteLine("Raw Media Count: " + rawMedia.Count);
                //First check title:
                bool founditemsExact = rawMedia.Any(p => string.Equals(query, p.Title, StringComparison.OrdinalIgnoreCase));
                bool founditemsPartial = rawMedia.Any(p => query.Contains(p.Title, StringComparison.OrdinalIgnoreCase));
                string cleanQuery = query.Trim();
                bool foundItems = rawMedia.Any(p => p.Title != null && p.Title.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));
                if (founditemsExact)
                {
                    Debug.WriteLine("Found items exact: " + query);

                    var allMatches = rawMedia.Where(p => string.Equals(query, p.Title, StringComparison.OrdinalIgnoreCase)).ToList().Take(ResultCount);
                    foreach (var item in allMatches)
                    {
                        var subinfo = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "Song" : "Video";
                        var imgicon = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "musicnoteicon" : "default";
                        SearchResultsMain.Add(new MasterSearchModel { FilePath = item.FilePath, ResultMain = item.Title, SubInformation = subinfo, ImageThumbnail = $"ms-appx:///Assets/{imgicon}.png"});
                    }

                }
                else
                {
                    Debug.WriteLine("Found items partially: " + query);
                    var matchedSongByTitle = rawMedia
                            .Where(p => !string.IsNullOrWhiteSpace(p.Title) && p.Title.Length > 2)
                            .FirstOrDefault(p => cleanQuery.Contains(p.Title, StringComparison.OrdinalIgnoreCase)
                                              || p.Title.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));

                    if (matchedSongByTitle != null)
                    {
                        // Add Matched Song First
                        bool isAudio = !VideoExtensions.List.Contains(Path.GetExtension(matchedSongByTitle.FilePath).ToLowerInvariant());
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            FilePath = matchedSongByTitle.FilePath,
                            ResultMain = matchedSongByTitle.Title,
                            SubInformation = isAudio ? "Song" : "Video",
                            ImageThumbnail = $"ms-appx:///Assets/{(isAudio ? "musicnoteicon" : "default")}.png"
                        });

                        // Strip matched title out of query
                        string remainingQuery = cleanQuery
                            .Replace(matchedSongByTitle.Title, "", StringComparison.OrdinalIgnoreCase)
                            .Trim();

                        if (!string.IsNullOrWhiteSpace(remainingQuery))
                        {
                            // Split remaining query into individual word tokens: ["conan", "gray"]
                            var tokens = remainingQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                            // Tokenized Matching: Ensure every token exists in p.Artist or p.AlbumName
                            var matchedArtistTrack = rawMedia.FirstOrDefault(p =>
                                !string.IsNullOrWhiteSpace(p.Artist) &&
                                tokens.All(token => p.Artist.Contains(token, StringComparison.OrdinalIgnoreCase)));

                            if (matchedArtistTrack != null)
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

                            var matchedAlbumTrack = rawMedia.FirstOrDefault(p =>
                                !string.IsNullOrWhiteSpace(p.AlbumName) &&
                                tokens.All(token => p.AlbumName.Contains(token, StringComparison.OrdinalIgnoreCase)));

                            if (matchedAlbumTrack != null)
                            {
                                SearchResultsMain.Add(new MasterSearchModel
                                {
                                    ResultMain = matchedAlbumTrack.AlbumName,
                                    SubInformation = "Album",
                                    ImageThumbnail = "ms-appx:///Assets/defaultalbum.png",
                                    Album = matchedAlbumTrack.AlbumName,
                                    SearchFilter = ClassModels.Filters.Album
                                });
                            }
                        }
                    }

                    else if (filters == Filters.Album)
                    {

                    }

                }

                //Second check artist:

            }
            return SearchResultsMain;
        }
    }
}

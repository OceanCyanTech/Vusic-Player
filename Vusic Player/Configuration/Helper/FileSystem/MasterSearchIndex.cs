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
                Debug.WriteLine($"Processing Query: {query}");
                Debug.WriteLine($"Raw Media Count: {rawMedia.Count}");

                string cleanQuery = query.Trim();
                if (string.IsNullOrWhiteSpace(cleanQuery)) return SearchResultsMain;

                // 1. Exact Title Matches
                var exactTitleMatches = rawMedia
                    .Where(p => string.Equals(cleanQuery, p.Title, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (exactTitleMatches.Count > 0)
                {
                    Debug.WriteLine($"Found exact title matches: {cleanQuery}");
                    var matchesToTake = AllResults ? exactTitleMatches : exactTitleMatches.Take(ResultCount);

                    foreach (var item in matchesToTake)
                    {
                        bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant());
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            FilePath = item.FilePath,
                            ResultMain = item.Title,
                            SubInformation = isVideo ? "Video" : "Song",
                            ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
                        });
                    }
                }
                else
                {
                    // 2. Partial Title Match & Token Parsing
                    var matchedSongByTitle = rawMedia
                        .Where(p => !string.IsNullOrWhiteSpace(p.Title) && p.Title.Length > 2)
                        .FirstOrDefault(p => cleanQuery.Contains(p.Title, StringComparison.OrdinalIgnoreCase)
                                          || p.Title.Contains(cleanQuery, StringComparison.OrdinalIgnoreCase));

                    if (matchedSongByTitle != null)
                    {
                        bool isVideo = VideoExtensions.List.Contains(Path.GetExtension(matchedSongByTitle.FilePath).ToLowerInvariant());
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            FilePath = matchedSongByTitle.FilePath,
                            ResultMain = matchedSongByTitle.Title,
                            SubInformation = isVideo ? "Video" : "Song",
                            ImageThumbnail = $"ms-appx:///Assets/{(isVideo ? "default" : "musicnoteicon")}.png"
                        });

                        // Strip matched title out of query for remaining tokens
                        string remainingQuery = cleanQuery
                            .Replace(matchedSongByTitle.Title, "", StringComparison.OrdinalIgnoreCase)
                            .Trim();

                        if (!string.IsNullOrWhiteSpace(remainingQuery))
                        {
                            var tokens = remainingQuery.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

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
                }

                // 3. Exact Artist Matches (Only execute if no exact title was found to prevent duplicate results)
                if (exactTitleMatches.Count == 0)
                {
                    var exactArtistMatches = rawMedia
                        .Where(p => string.Equals(cleanQuery, p.Artist, StringComparison.OrdinalIgnoreCase))
                        .DistinctBy(p => p.Artist) // Avoid adding the same artist multiple times for different tracks
                        .Take(ResultCount);

                    foreach (var item in exactArtistMatches)
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            FilePath = item.FilePath,
                            ResultMain = item.Artist,
                            SearchFilter = ClassModels.Filters.Artist,
                            SubInformation = "Artist",
                            ImageThumbnail = "ms-appx:///Assets/defaultartist.png"
                        });
                    }

                    var exactAlbumMatches = rawMedia
        .Where(p => !string.IsNullOrWhiteSpace(p.AlbumName) &&
                    string.Equals(cleanQuery, p.AlbumName, StringComparison.OrdinalIgnoreCase))
        .DistinctBy(p => p.AlbumName)
        .Take(ResultCount);

                    foreach (var item in exactAlbumMatches)
                    {
                        SearchResultsMain.Add(new MasterSearchModel
                        {
                            FilePath = item.FilePath,
                            ResultMain = item.AlbumName,
                            Album = item.AlbumName,
                            SearchFilter = ClassModels.Filters.Album,
                            SubInformation = "Album",
                            ImageThumbnail = "ms-appx:///Assets/defaultalbum.png"
                        });
                    }
                }
            }
            else if (filters == Filters.Album)
            {

            }
            return SearchResultsMain;
        }
    }
}

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EntireMusicLibrary : Page
    {
        public EntireMusicLibrary()
        {
            InitializeComponent();
            LoadSettings();

            stkLoading.Visibility = Visibility.Visible;
            LoadFolders();
            //        LoadDummy();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string str && str == "EntireHistory")
            {
                ToggleButton[] musicButtons = { btnAllMusic, btnPlaylists, btnArtists, btnAlbums, btnGenres, btnHistory };
                foreach (var btn in musicButtons)
                {
                    btn.IsChecked = false;
                }

                btnHistory.IsChecked = true;
                grdAllMusic.Visibility = Visibility.Collapsed;
                grdGenres.Visibility = Visibility.Collapsed;
                grdPlaylists.Visibility = Visibility.Collapsed;
                grdAlbums.Visibility = Visibility.Collapsed;
                grdArtists.Visibility = Visibility.Collapsed;
                grdAllMusicHistory.Visibility = Visibility.Visible;
                LoadHistory();
            }
            else if (e.Parameter is string str2 && str2 == "Videos")
            {
                txtHistoryHeading.Text = "History of videos you've watched on this app";
                btnAllMusic.Content = "All Videos";
                btnHistory.Content = "Video History";
                txtAllMusicHeader.Text = "All videos across your libraries";
                AllMusicGroupedCollection.SearchPlaceHolderText = "Search for videos...";
                btnAlbums.Visibility = Visibility.Collapsed;
                btnArtists.Visibility = Visibility.Collapsed;
                btnGenres.Visibility = Visibility.Collapsed;
            }
            base.OnNavigatedTo(e);
        }
        private async void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectedBtn = sender as ToggleButton;
            if (selectedBtn == null) return;
            // 1. Define the list of buttons in this group
            ToggleButton[] musicButtons = { btnAllMusic, btnPlaylists, btnArtists, btnAlbums, btnGenres, btnHistory };

            foreach (var btn in musicButtons)
            {
                // 2. Uncheck everything that isn't the button we just clicked
                if (btn != selectedBtn)
                {
                    btn.IsChecked = false;
                }
            }

            selectedBtn.IsChecked = true;
            string category = selectedBtn.Content.ToString()!;
            grdAllMusic.Visibility = Visibility.Collapsed;
            grdGenres.Visibility = Visibility.Collapsed;
            grdPlaylists.Visibility = Visibility.Collapsed;
            grdAlbums.Visibility = Visibility.Collapsed;
            grdArtists.Visibility = Visibility.Collapsed;
            grdAllMusicHistory.Visibility = Visibility.Collapsed;
            if (category == "Music History" || category == "Video History")
            {
                grdAllMusicHistory.Visibility = Visibility.Visible;
                LoadHistory();
            }
            else if (category == "All Music" || category == "All Videos")
            {
                grdAllMusic.Visibility = Visibility.Visible;
            }
            else if (category == "Playlists")
            {
                grdPlaylists.Visibility = Visibility.Visible;
                Debug.WriteLine("Playlst");
                LoadAllPlaylists();
            }
            else if (category == "Artists")
            {
                grdArtists.Visibility = Visibility.Visible;
                Debug.WriteLine("Playlst");
                await LoadArtists();
            }
            else if (category == "Albums")
            {
                grdAlbums.Visibility = Visibility.Visible;
                Debug.WriteLine("Album");
                await LoadAlbums();
            }
            else if (category == "Genres")
            {
                grdGenres.Visibility = Visibility.Visible;
                Debug.WriteLine("Genre");
                await LoadGenres();
            }
        }
        private async void LoadAllPlaylists()
        {
            Debug.WriteLine("Playlst2");

            var currentset = await SettingsLoader.LoadSettingsAsync();
            var playlists = currentset.SavedPlaylists;
            playlistsAll.Clear();

            foreach (var playlist in playlists)
            {
                Debug.WriteLine("PLAYLIST BEING ADDED IS :" + playlist.PlaylistName);
                playlistsAll.Add(new PlaylistItem { PlaylistId = playlist.PlaylistId, PlaylistName = playlist.PlaylistName, PlaylistCount = playlist.PlaylistCount, Thumbnail = playlist.Thumbnail });
            }
            Debug.WriteLine("Playlst3");

        }
        ObservableCollection<RecentMusicModel> recentMusics = new();
        ObservableCollection<PlaylistItem> playlistsAll = new();
        ObservableCollection<ArtistShow> artistsAll = new();
        ObservableCollection<ArtistDiscAlbumModel> albumsAll = new();
        ObservableCollection<GenreModel> genresAll = new();
        private async void LoadHistory()
        {
            recentMusics.CollectionChanged += RecentMusics_CollectionChanged;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recentmusic = currentSettings.RecentMusic;

            foreach (var item in recentmusic)
            {
                recentMusics.Add(new RecentMusicModel
                {
                    FolderName = new DirectoryInfo(
                            Path.GetDirectoryName(item.SongPath) ?? string.Empty
                        ).Name,
                    SongName = item.SongName,
                    SongPath = item.SongPath,
                    PlayCountDisplay = $"{item.PlayCount} {(item.PlayCount == 1 ? "time" : "times")}",
                    LastPlayed = item.LastPlayed
                });

            }
            foreach (var item in recentMusics)
            {
                var task = Task.Run(async () =>
                {
                    var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(item.SongPath);
                    Debug.WriteLine("The thumbnail path is " + thumb);
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        try
                        {
                            var bitmap = new BitmapImage();
                            using (var stream = File.OpenRead(thumb))
                            {
                                await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                            }
                            item.Thumbnail = bitmap;

                            File.Delete(thumb);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("An unexpected error occured: " + ex.Message);
                        }
                    });

                });

            }
            grdViewAllRecentMusic.ItemsSource = recentMusics;
            if (recentMusics.Count == 0)
            {
                grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                stkEmptyHistory.Visibility = Visibility.Visible;
                btnClearHistory.IsEnabled = false;
                btnPlayAll.IsEnabled = false;
                asbRecents.IsEnabled = false;

            }
            else
            {
                grdViewAllRecentMusic.Visibility = Visibility.Visible;
                stkEmptyHistory.Visibility = Visibility.Collapsed;
                btnClearHistory.IsEnabled = true;
                btnPlayAll.IsEnabled = true;
                asbRecents.IsEnabled = true;

            }
        }

        private void RecentMusics_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (recentMusics.Count == 0)
            {
                grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                stkEmptyHistory.Visibility = Visibility.Visible;
                btnClearHistory.IsEnabled = false;
                btnPlayAll.IsEnabled = false;
                asbRecents.IsEnabled = false;

            }
            else
            {
                btnClearHistory.IsEnabled = true;
                btnPlayAll.IsEnabled = true;
                asbRecents.IsEnabled = true;

                grdViewAllRecentMusic.Visibility = Visibility.Visible;
                stkEmptyHistory.Visibility = Visibility.Collapsed;
            }
        }

        private void btnAllMusic_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGlyph_Click(object sender, RoutedEventArgs e)
        {

        }

        private void hypTitle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void hypArtist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void hypAlbum_Click(object sender, RoutedEventArgs e)
        {

        }
        public async Task<List<StorageFile>> GetAllFilesRecursivelyAsync(StorageFolder folder)
        {
            var fileList = new List<StorageFile>();

            // 1. Get surface files
            var surfaceFiles = await folder.GetFilesAsync();
            fileList.AddRange(surfaceFiles);

            // 2. Loop through subfolders and recurse
            var subfolders = await folder.GetFoldersAsync();
            foreach (var subfolder in subfolders)
            {
                var subfolderFiles = await GetAllFilesRecursivelyAsync(subfolder);
                fileList.AddRange(subfolderFiles);
            }

            return fileList;
        }
        private async Task LoadArtists()
        {
            if (AllAvailableSongs.Count > 0)
            {
                artistsAll.Clear();
                var artistSongCounts = AllAvailableSongs
        .Where(song => song.Artist != null && !string.IsNullOrEmpty(song.Artist)) // Ensure artist isn't null/empty
        .GroupBy(song => song.Artist) // Group songs together by the artist's name
        .Select(group => new
        {
            ArtistName = group.Key,     // The name we grouped by
            SongCount = group.Count(),
            Songs = group.ToList()// Total number of items in this group
        })
        .OrderBy(result => result.ArtistName) // Optional: alphabetize by artist name
        .ToList();
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var artists = currentSettings.ArtistsList;
                foreach (var artist in artistSongCounts)
                {
                    string fallbackUri = "ms-appx:///Assets/defaultartist.png";

                    var existartist = artists.FirstOrDefault(p => p.Name == artist.ArtistName);
                    if (existartist != null)
                    {
                        fallbackUri = existartist.Thumbnail;
                    }
                    var artistsongcount = $"• {artist.SongCount} {(artist.SongCount == 1 ? "item" : "items")}";
                    artistsAll.Add(new ArtistShow { ArtistName = artist.ArtistName, ArtistSongCount = artistsongcount, ArtistThumbnail = fallbackUri, Songs = artist.Songs });
                }
            }
        }
        private async Task LoadAlbums()
        {
            if (AllAvailableSongs.Count > 0)
            {
                albumsAll.Clear();
                var albumsSongCounts = AllAvailableSongs
    .Where(song => song.AlbumName != null && !string.IsNullOrEmpty(song.AlbumName))
    .GroupBy(song => song.AlbumName)
    .Select(group =>
    {

        // 1. Extract years directly from the file tags
        var years = group
            .Select(song =>
            {
                try
                {
                    // Assuming your song model has a Path or FilePath property
                    using (var file = TagLib.File.Create(song.FilePath))
                    {
                        return (int)file.Tag.Year; // Returns 0 if not set
                    }
                }
                catch
                {
                    return 0; // Fallback for unreadable files
                }
            })
            .Where(year => year > 0)
            .ToList();

        int finalYear = 0;

        if (years.Any())
        {
            var yearGroups = years.GroupBy(y => y).OrderByDescending(g => g.Count()).ToList();
            bool isAllVaried = yearGroups.All(g => g.Count() == yearGroups.First().Count());
            finalYear = isAllVaried ? years.First() : yearGroups.First().Key;
        }

        return new
        {
            AlbumName = group.Key,
            SongCount = group.Count(),
            CalculatedYear = finalYear,
            Artists = string.Join(", ", group
                .Select(song => song.Artist)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .OrderBy(name => name)),
            Songs = group.ToList()
        };
    })
    .OrderBy(result => result.AlbumName)
    .ToList();
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var albums = currentSettings.AlbumsList;
                foreach (var artist in albumsSongCounts)
                {
                    string fallbackUri = "ms-appx:///Assets/defaultalbum.png";

                    var existalbum = albums.FirstOrDefault(p => p.Name == artist.AlbumName);
                    if (existalbum != null)
                    {
                        fallbackUri = existalbum.Thumbnail;
                    }

                    var artistsongcount = $"• {artist.SongCount} {(artist.SongCount == 1 ? "item" : "items")}";
                    albumsAll.Add(new ArtistDiscAlbumModel { AlbumName = artist.AlbumName, AlbumCount = artistsongcount, Thumbnail = fallbackUri, AlbumYear = $"• {artist.CalculatedYear.ToString()}", AlbumArtists = artist.Artists, Songs = artist.Songs });
                }
            }
        }

        private async Task LoadGenres()
        {
            if (AllAvailableSongs.Count > 0)
            {

                genresAll.Clear();

                grdViewGenres.ItemsSource = genresAll;
                // 1. Define your presets and add them to your collection first
                var genrePresets = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
{
    { "Rock", "ms-appx:///Assets/Genres/appicon.png" },
    { "Pop", "ms-appx:///Assets/Genres/appicon.png" },
    { "Jazz", "ms-appx:///Assets/Genres/appicon.png" },
    { "Classical", "ms-appx:///Assets/Genres/appicon.png" },
    { "Hip-Hop", "ms-appx:///Assets/Genres/appicon.png" }
};


                // 2. Add presets to your collection first
                foreach (var preset in genrePresets)
                {
                    if (!genresAll.Any(g => g.GenreName.Equals(preset.Key, StringComparison.OrdinalIgnoreCase)))
                    {
                        genresAll.Add(new GenreModel
                        {
                            GenreName = preset.Key,
                            GenreCount = "• 0 items",
                            GenreCover = preset.Value // Uses the specific preset cover
                        });
                    }
                }

                // 2. Extract and group genres from your available songs
                var genresSongCounts = AllAvailableSongs
                    .Select(song =>
                    {
                        string songGenre = "Unknown Genre";
                        try
                        {
                            if (!string.IsNullOrEmpty(song.FilePath))
                            {
                                using (var file = TagLib.File.Create(song.FilePath))
                                {
                                    var primaryGenre = file.Tag.FirstGenre;
                                    if (!string.IsNullOrEmpty(primaryGenre))
                                    {
                                        songGenre = primaryGenre.Trim();
                                    }
                                }
                            }
                        }
                        catch
                        {
                            songGenre = "Unknown Genre";
                        }
                        return songGenre;
                    })
                    .GroupBy(genre => genre)
                    .Select(group => new
                    {
                        GenreName = group.Key,
                        SongCount = group.Count()
                    })
                    .ToList();

                // 3. Merge grouped data into genresAll without causing duplicates
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var genresListFromSettings = currentSettings.GenresList;

                foreach (var genreData in genresSongCounts)
                {
                    var itemStringCount = $"• {genreData.SongCount} {(genreData.SongCount == 1 ? "item" : "items")}";

                    // Check if this genre was already added via presets
                    var existingGenre = genresAll.FirstOrDefault(g => g.GenreName.Equals(genreData.GenreName, StringComparison.OrdinalIgnoreCase));

                    if (existingGenre != null)
                    {
                        // Duplicate prevented! Just update the item count for the existing preset
                        existingGenre.GenreCount = itemStringCount;

                        // Optional: Update thumbnail from settings if available
                        var existGenreSettings = genresListFromSettings?.FirstOrDefault(p => p.GenreName == existingGenre.GenreName);
                        if (existGenreSettings != null)
                        {
                            existingGenre.GenreCover = existGenreSettings.GenreCover;
                        }
                    }
                    else
                    {
                        // Completely new genre found in files, resolve thumbnail and add it
                        string fallbackUri = "ms-appx:///Assets/defaultgenre.png";
                        var existGenreSettings = genresListFromSettings?.FirstOrDefault(p => p.GenreName == genreData.GenreName);
                        if (existGenreSettings != null)
                        {
                            fallbackUri = existGenreSettings.GenreCover;
                        }

                        genresAll.Add(new GenreModel
                        {
                            GenreName = genreData.GenreName,
                            GenreCount = itemStringCount,
                            GenreCover = fallbackUri
                        });
                    }
                }
            }
        }


        private async void LoadSettings()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            chkIncludeSubDirectories.IsChecked = currentSettings.IncludeSubDirMusLib;
        }
        public ObservableCollection<SongModel> AllAvailableSongs = new ObservableCollection<SongModel>();
        private async Task LoadAllFiles(List<string> searchPaths)
        {
            //List<StorageFile> allFoundFiles = new List<StorageFile>();
            bool includeSubDirs = chkIncludeSubDirectories.IsChecked == true;

            // 1. Gather all files
            //foreach (var path in searchPaths)
            //{
            //    try
            //    {
            //        StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            //        var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, AudioExtensions.List)
            //        {
            //            FolderDepth = includeSubDirs ? FolderDepth.Deep : FolderDepth.Shallow
            //        };

            //        queryOptions.SetPropertyPrefetch(PropertyPrefetchOptions.MusicProperties, null);

            //        var queryResult = folder.CreateFileQueryWithOptions(queryOptions);
            //        IReadOnlyList<StorageFile> files = await queryResult.GetFilesAsync();
            //        allFoundFiles.AddRange(files);
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine($"Error scanning path {path}: {ex.Message}");
            //    }
            //}

            //if (allFoundFiles.Count == 0) return;
            Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            // 1. Keep the UI-bound settings/existing paths check on the UI thread
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites?.ToList() ?? new List<FavouriteItems>();

            // 1. Snapshot currently loaded paths
            var existingPaths = AllAvailableSongs
                .Select(s => string.IsNullOrWhiteSpace(s.FilePath) ? "" : Path.GetFullPath(s.FilePath))
                .Where(p => !string.IsNullOrEmpty(p))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // 2. Scan and parse completely using the Lite Model
            List<AudioTrackLite> discoveredTracks = await Task.Run(() =>
            {
                var uniqueFilesToParse = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var path in searchPaths)
                {
                    try
                    {
                        if (!Directory.Exists(path)) continue;

                        var searchOption = includeSubDirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
                        var directoryInfo = new DirectoryInfo(path);
                        var files = directoryInfo.EnumerateFiles("*.*", searchOption)
                            .Where(f => AudioExtensions.List.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

                        foreach (var file in files)
                        {
                            string normalizedPath = Path.GetFullPath(file.FullName);
                            if (!existingPaths.Contains(normalizedPath))
                            {
                                uniqueFilesToParse.Add(normalizedPath);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error scanning path {path}: {ex.Message}");
                    }
                }

                var liteList = new List<AudioTrackLite>();

                foreach (var filePath in uniqueFilesToParse)
                {
                    try
                    {
                        using (var tagFile = TagLib.File.Create(filePath))
                        {
                            bool isFav = favourites.Any(p => string.Equals(Path.GetFullPath(p.FilePath), filePath, StringComparison.OrdinalIgnoreCase));

                            string title = string.IsNullOrWhiteSpace(tagFile.Tag.Title)
                                ? Path.GetFileNameWithoutExtension(filePath)
                                : tagFile.Tag.Title;

                            string album = string.IsNullOrWhiteSpace(tagFile.Tag.Album)
                                ? "Unknown Album"
                                : tagFile.Tag.Album;

                            string artist = string.IsNullOrWhiteSpace(string.Join(',', tagFile.Tag.AlbumArtists))
                                ? "Unknown Artist"
                                : string.Join(',', tagFile.Tag.AlbumArtists);

                            liteList.Add(new AudioTrackLite
                            {
                                Title = title,
                                AlbumName = album,
                                Artist = artist,
                                FilePath = filePath,
                                SongDuration = tagFile.Properties.Duration,
                                IsFavourite = isFav
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"TagLib parsing error for {filePath}: {ex.Message}");
                    }
                }

                return liteList;
            });

            // 3. Batch conversion to your original SongModel on the UI thread
            int batchSize = 40;
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

            for (int i = 0; i < discoveredTracks.Count; i += batchSize)
            {
                var batch = discoveredTracks.Skip(i).Take(batchSize).ToList();

                dispatcher.TryEnqueue(() =>
                {
                    foreach (var liteTrack in batch)
                    {
                        // Direct duplicate guard-check against the UI collection
                        if (AllAvailableSongs.Any(s => string.Equals(Path.GetFullPath(s.FilePath), liteTrack.FilePath, StringComparison.OrdinalIgnoreCase)))
                            continue;

                        // Instantiate your original SongModel safely inside the UI Thread Context
                        AllAvailableSongs.Add(new SongModel
                        {
                            Title = liteTrack.Title,
                            AlbumName = liteTrack.AlbumName,
                            Artist = liteTrack.Artist,
                            FilePath = liteTrack.FilePath,
                            SongDuration = liteTrack.SongDuration,
                            IsFavourite = liteTrack.IsFavourite,
                            Glyph = "\uEC4F"
                        });
                    }
                });

                // Breath frame for the UI engine
                await Task.Delay(16);
            }            // This allows Windows to read all file metadata concurrently in the background safely
            //var parsingTasks = allFoundFiles.Select(async file =>
            //{
            //    if (existingPaths.Contains(file.Path)) return null;

            //    try
            //    {
            //        // Safely called on the UI thread context, but executes asynchronously
            //        var musicProperties = await file.Properties.GetMusicPropertiesAsync();
            //        bool isFav = favourites.Any(p => p.FilePath == file.Path);

            //        string title = string.IsNullOrWhiteSpace(musicProperties.Title)
            //            ? file.DisplayName
            //            : musicProperties.Title;

            //        string album = string.IsNullOrWhiteSpace(musicProperties.Album)
            //            ? "Unknown Album"
            //            : musicProperties.Album;

            //        string artist = string.IsNullOrWhiteSpace(musicProperties.Artist)
            //            ? "Unknown Artist"
            //            : musicProperties.Artist;

            //        return new SongModel
            //        {
            //            Title = title,
            //            AlbumName = album,
            //            Artist = artist,
            //            FilePath = file.Path,
            //            SongDuration = musicProperties.Duration,
            //            IsFavourite = isFav,
            //            Glyph = "\uEC4F"
            //        };
            //    }
            //    catch (Exception ex)
            //    {
            //        Debug.WriteLine($"Native parsing error for {file.Name}: {ex.Message}");
            //        return null;
            //    }
            //});

            //// 3. Await all metadata lookups in parallel. The UI stays 100% fluid here.
            //var results = await Task.WhenAll(parsingTasks);

            //// 4. Filter out nulls (skipped/failed files) and batch add to your UI collection
            //foreach (var song in results)
            //{
            //    if (song != null)
            //    {
            //        AllAvailableSongs.Add(song);
            //    }
            //}
        }
        private async Task ScanAllFoldersAsync(List<string> searchPaths)
        {
            var extensions = AudioExtensions.List;

            await Task.Run(() =>
            {
                var discoveredSongs = new List<SongModel>();
                int batchSize = 50;

                foreach (var path in searchPaths)
                {
                    if (!Directory.Exists(path)) continue;

                    IEnumerable<string> files;
                    try
                    {
                        files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
                    }
                    catch { continue; }

                    foreach (var file in files)
                    {
                        string ext = Path.GetExtension(file).ToLower();
                        if (!extensions.Contains(ext)) continue;

                        try
                        {
                            // Use TagLib to read metadata completely offline on the background thread
                            using (var taggedFile = TagLib.File.Create(file))
                            {
                                var props = taggedFile.Tag;

                                var song = new SongModel
                                {
                                    Title = string.IsNullOrEmpty(props.Title)
                                        ? Path.GetFileNameWithoutExtension(file)
                                        : props.Title,
                                    Artist = string.IsNullOrEmpty(props.FirstPerformer)
                                        ? "Unknown"
                                        : props.FirstPerformer,
                                    AlbumName = props.Album ?? "",
                                    SongDuration = taggedFile.Properties.Duration,
                                    FilePath = file
                                };

                                discoveredSongs.Add(song);
                            }

                            if (discoveredSongs.Count >= batchSize)
                            {
                                var batchToPush = discoveredSongs.ToList();
                                discoveredSongs.Clear();

                                _ = DispatcherQueue.EnqueueAsync(() =>
                                {
                                    foreach (var item in batchToPush)
                                    {
                                        AllAvailableSongs.Add(item);
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex.Message, "EntireMusicLibrarySearchFiles", Logger.LogLevelType.Error);
                        }
                    }
                }

                if (discoveredSongs.Count > 0)
                {
                    _ = DispatcherQueue.EnqueueAsync(() =>
                    {
                        foreach (var item in discoveredSongs)
                        {
                            AllAvailableSongs.Add(item);
                        }
                    });
                }
            });
        }
        private void btnFavourite_Click(object sender, RoutedEventArgs e)
        {

        }
        private async void LoadItemsFromFolder(string path)
        {

        }
        private async void LoadFolders()
        {
            UserDataPaths paths = UserDataPaths.GetDefault();

            // Assigning the system paths to the Tag property
            btnMusic.Tag = paths.Music;
            btnPictures.Tag = paths.Pictures;
            btnVideos.Tag = paths.Videos;
            btnDocuments.Tag = paths.Documents;
            btnDownloads.Tag = paths.Downloads;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            foldersListOpened.Clear();
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Music, FolderName = "Music Folder", isChecked = true });
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Pictures, FolderName = "Pictures Folder", isChecked = true });
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Videos, FolderName = "Videos Folder", isChecked = true });
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Documents, FolderName = "Documents Folder", isChecked = true });
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Downloads, FolderName = "Downloads Folder", isChecked = true });

            foreach (var item in currentSettings.SavedFoldersOpened)
            {
                item.FolderName = Path.GetFileName(item.FolderPath);
                foldersListOpened.Add(item);
                //    ToggleButton toggleButton = new();
                //    if (item.isChecked)
                //    {
                //        toggleButton.IsChecked = true;
                //    }
                //    toggleButton.ContextFlyout = (MenuFlyout)this.Resources["FolderContextMenu"];
                //    var flyout = toggleButton.ContextFlyout as MenuFlyout;
                //    var openItem = flyout.Items[0] as MenuFlyoutItem; // "Open Location"
                //    var removeItem = flyout.Items[2] as MenuFlyoutItem; // "Remove" (skip separator)

                //    // Detach old handlers if any, and attach a specific one for THIS button
                //    openItem.Click += (s, e) =>
                //    {
                //        string path = item.FolderPath;
                //        if (System.IO.Directory.Exists(path))
                //        {
                //            System.Diagnostics.
                //            ("explorer.exe", $"/select,\"{path}\"");
                //        }
                //    };
                //    toggleButton.Tag = item.FolderPath;
                //    toggleButton.CornerRadius = new CornerRadius(16);
                //    toggleButton.FontSize = 16;
                //    ToolTipService.SetToolTip(toggleButton, item.FolderPath);

                //    toggleButton.Padding = new Thickness(10);
                //    toggleButton.Content = Path.GetFileName(item.FolderPath);
                //    if (App.Current.Resources.TryGetValue("SurfaceStrokeColorDefaultBrush", out object resource))
                //    {
                //        toggleButton.BorderBrush = (Brush)resource;
                //    }
                //    toggleButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                //    stkAddedFolders.Children.Add(toggleButton);
                //}
            }
            grdViewFolders.ItemsSource = foldersListOpened;
            List<string> fpaths = new();
            foreach (var item in foldersListOpened)
            {
                fpaths.Add(item.FolderPath);
            }
            AllAvailableSongs.Clear();
            await LoadAllFiles(fpaths);
            stkLoading.Visibility = Visibility.Collapsed;
        }
        ObservableCollection<FoldersListOpened> foldersListOpened = new();
        private void LoadDummy()
        {
            var list = new ObservableCollection<SongModel>();
            list.Add(new SongModel { Title = "The Best", Artist = "Conan Gray", AlbumName = "Wishbone Deluxe" });
            list.Add(new SongModel { Title = "Vodka Cranberry", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "This Song", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Nauseous", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Actor", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Care", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Conell", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Class Clown", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "The Exit", Artist = "Conan Gray", AlbumName = "Superache" });
            list.Add(new SongModel { Title = "Memories", Artist = "Conan Gray", AlbumName = "Superache" });
            list.Add(new SongModel { Title = "Footnote", Artist = "Conan Gray", AlbumName = "Superache" });
            list.Add(new SongModel { Title = "Astroonmy", Artist = "Conan Gray", AlbumName = "Superache" });
            list.Add(new SongModel { Title = "Movies", Artist = "Conan Gray", AlbumName = "Superache" });
            AllMusicGroupedCollection.ItemsSource = list;
        }
        private async void LoadItems(List<string> paths)
        {
            var userPaths = UserDataPaths.GetDefault();
            var ObservableCollectioln = AllAvailableSongs;

            var extensions = AudioExtensions.List;

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                    continue;

                IEnumerable<string> files;

                try
                {
                    files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
                    Debug.WriteLine("check1");
                }
                catch
                {
                    continue;
                }

                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();

                    if (!extensions.Contains(ext))
                        continue;

                    try
                    {
                        StorageFile storageFile =
                            await StorageFile.GetFileFromPathAsync(file);

                        MusicProperties props =
                            await storageFile.Properties.GetMusicPropertiesAsync();
                        Debug.WriteLine("check2");

                        var song = new SongModel
                        {
                            Title = string.IsNullOrEmpty(props.Title)
                                ? Path.GetFileNameWithoutExtension(file)
                                : props.Title,

                            Artist = string.IsNullOrEmpty(props.Artist)
                                ? "Unknown"
                                : props.Artist,

                            AlbumName = props.Album ?? "",

                        };

                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            Debug.WriteLine("check3");

                            ObservableCollectioln.Add(song);

                        });

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, "EntireMusicLibrarySearchFiles", Logger.LogLevelType.Error);
                    }
                }

            }
            AllMusicGroupedCollection.ItemsSource = ObservableCollectioln;
        }
        private async void btnAddFolders_Click(object sender, RoutedEventArgs e)
        {
            var obser = new ObservableCollection<SongModel>();

            if (App.MainWindowInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Choose Folder", Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
            if (folder != null)
            {
                var alreadyexist = foldersListOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                if (alreadyexist != null) return;

                foldersListOpened.Add(new FoldersListOpened { FolderPath = folder.Path, FolderName = Path.GetFileName(folder.Path), isChecked = true });
                // Detach old handlers if any, and attach a specific one for THIS button

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var check = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                if (check == null)
                {
                    currentSettings.SavedFoldersOpened.Add(new FoldersListOpened { FolderPath = folder.Path, isChecked = true });
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void ToggleButton_Checked1(object sender, RoutedEventArgs e)
        {
        }

        private void mnftOpenFolder_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {

                if (tgl.FolderPath != null)
                {

                    if (Directory.Exists(tgl.FolderPath))
                    {

                        Process.Start("explorer.exe", $"\"{tgl.FolderPath}\"");
                    }
                }
            }
        }

        private void mnftCopyPathFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {
                if (tgl.FolderPath is string str)
                {
                    CopyToClipboard.CopyStringToClipboard(str);
                }
            }
        }

        private async void mnftRemoveFolder_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)

            {
                foldersListOpened.Remove(tgl);
                var currentse = await SettingsLoader.LoadSettingsAsync();
                if (tgl.FolderPath is string str)
                {
                    var folder = currentse.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == str);
                    if (folder != null)
                    {
                        currentse.SavedFoldersOpened.Remove(folder);
                    }
                    await SettingsLoader.SaveSettingsAsync(currentse);
                }
            }
        }

        private async void btnGenericFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tgl)
            {
                if (tgl.IsChecked == true)
                {
                    if (tgl.DataContext is FoldersListOpened folder)
                    {
                        var folderpath = folder.FolderPath;
                        var list = new List<string>();
                        list.Add(folderpath);
                        await LoadAllFiles(list);
                    }
                }
            }
            //if (sender is ToggleButton tgl)
            //{

            //    Debug.WriteLine("add");
            //    if (tgl.DataContext is FoldersListOpened fol)
            //    {
            //        Debug.WriteLine("add2");

            //        var path = fol.FolderPath;
            //        Debug.WriteLine(path);

            //               AllAvailableSongs.Clear();
            //        List<string> paths = new();
            //        paths.Add(path);
            //        LoadItems(paths);
            //    }
            //    //        await ScanAllFoldersAsync(paths);
            //    //        AllMusicGroupedCollection.ItemsSource = AllAvailableSongs;

            //    //    }
            //    //}

        }

        private void btnGenericFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn && btn.DataContext is FoldersListOpened folder)
            {
                if (btn.IsChecked == false)
                {
                    var folderpath = folder.FolderPath;
                    string folderWithBackslash = folderpath.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? folderpath
                : folderpath + Path.DirectorySeparatorChar;

                    // 2. Use .Any() to see if AT LEAST ONE song is in this folder
                    var songsInThisFolder = AllAvailableSongs.Where(p =>
            p.FilePath.StartsWith(folderWithBackslash, StringComparison.OrdinalIgnoreCase))
            .ToList();

                    // Now you have the songs! For example, if you want to remove them:
                    foreach (var song in songsInThisFolder)
                    {
                        AllAvailableSongs.Remove(song);
                    }
                }
            }
        }

        private async void btnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recentmusic = currentSettings.RecentMusic;
            recentMusics.Clear();
            recentmusic.Clear();
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private void btnDisableHistory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftRemoveFromRecentMusic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftGoToFileLocation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftPlayRecents_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftAddtoQueueRecentMusic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftViewFileInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recentmusic = currentSettings.RecentMusic;
            if (recentmusic.Count == 0) return;
            ObservableCollection<SongModel> tempTransfer = new();
            foreach (var item in recentmusic)
            {
                var file = await StorageFile.GetFileFromPathAsync(item.SongPath);
                var props = await file.Properties.GetMusicPropertiesAsync();
                string Title = props.Title;
                if (Title == "")
                {
                    Title = Path.GetFileNameWithoutExtension(file.Path);
                }
                string AlbumName = props.Album;
                string Artist = props.Artist;
                tempTransfer.Add(new SongModel { Title = Title, AlbumName = AlbumName, Artist = Artist, SongDuration = props.Duration, FilePath = file.Path });
            }
            QueueService.PlayMedia(tempTransfer, false, false);

        }
        ObservableCollection<RecentMusicModel> searchresults = new();

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender is AutoSuggestBox asb)
            {
                if (asb.Text == "")
                {
                    searchresults.Clear();
                    asb.ItemsSource = null;
                    grdNoSearchResults.Visibility = Visibility.Collapsed;

                    grdViewAllRecentMusic.ItemsSource = recentMusics;
                    asb.ItemsSource = null;
                    grdViewAllRecentMusic.Visibility = Visibility.Visible;
                }
                if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {

                    searchresults.Clear();
                    var rawQuery = asb.Text.Trim();
                    // 1. Extract time components
                    var minMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:min|m)", RegexOptions.IgnoreCase);
                    var secMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:sec|s)", RegexOptions.IgnoreCase);

                    int searchSeconds = 0;
                    if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
                    if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);


                    var textQuery = rawQuery;
                    if (minMatch.Success) textQuery = textQuery.Replace(minMatch.Value, "");
                    if (secMatch.Success) textQuery = textQuery.Replace(secMatch.Value, "");
                    textQuery = textQuery.Trim().ToLower();

                    // 3. Filter the list
                    var results = recentMusics.Where(s =>
                    {
                        // Check if any text matches (only if textQuery isn't empty)
                        bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                            (s.SongName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                            (s.FolderName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)
                        );

                        return textMatch;
                    })
                    .OrderByDescending(s => s.SongName?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
                    .ThenBy(s => s.SongName)
                    .ToList();

                    if (results.Any())
                    {
                        asb.ItemsSource = null;
                        foreach (var item in results)
                        {
                            searchresults.Add(item);
                        }
                        grdViewAllRecentMusic.ItemsSource = searchresults;
                    }
                    else
                    {
                        var noresult = new List<string>();
                        noresult.Add("No matches found!");
                        asb.ItemsSource = null;
                        asb.ItemsSource = noresult;
                    }
                }
            }
        }
        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            asbRecents.Text = "";
            grdViewAllRecentMusic.Focus(FocusState.Programmatic);
            asbRecents.ItemsSource = null;
        }
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender is AutoSuggestBox asb)
            {
                searchresults.Clear();
                var query = asb.Text.ToLower().Trim();
                var minMatch = Regex.Match(query, @"(\d+)\s*(?:min|m)");
                var secMatch = Regex.Match(query, @"(\d+)\s*(?:sec|s)");
                int searchSeconds = 0;
                if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
                if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);
                var results = recentMusics.Where(s =>
                (s.SongName != null && s.SongName.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (s.FolderName != null && s.FolderName.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase))
             ).OrderByDescending(s =>
        s.SongName?.StartsWith(query, StringComparison.OrdinalIgnoreCase) == true)
                        .ThenBy(s => s.SongName)
                        .ToList();

                if (results.Any())
                {
                    foreach (var item in results)
                    {
                        searchresults.Add(item);
                    }
                    grdViewAllRecentMusic.ItemsSource = searchresults;
                    //       lstViewQueue.LoadMedia(searchresults, Frame);
                }
                else
                {
                    grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                    if (recentMusics.Count != 0)
                    {
                        grdNoSearchResults.Visibility = Visibility.Visible;
                        await Task.Delay(200);
                        frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
                    }

                }
            }
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = chkSelect.IsChecked ?? false;

            grdViewAllRecentMusic.GridSelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked ?? false)
                grdViewAllRecentMusic.SelectAll();
            else
                grdViewAllRecentMusic.ClearSelection();
        }

        private void btnRemoveFromHistory_Click(object sender, RoutedEventArgs e)
        {
            selectMoreOptions.Visibility = Visibility.Collapsed;

            grdViewAllRecentMusic.RemoveSelection();
        }

        private async void chkIncludeSubDirectories_Checked(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            stkLoading.Visibility = Visibility.Visible;

            currentSettings.IncludeSubDirMusLib = chkIncludeSubDirectories.IsChecked ?? true;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            List<string> fpaths = new();
            foreach (var item in foldersListOpened)
            {
                fpaths.Add(item.FolderPath);
            }
            AllAvailableSongs.Clear();
            await LoadAllFiles(fpaths);
            stkLoading.Visibility = Visibility.Collapsed;

        }
        private IEnumerable<GenreModel> GetFilteredResultsGenre(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<GenreModel>();

            var rawQuery = query.Trim();


            var textQuery = rawQuery;

            textQuery = textQuery.Trim();

            return genresAll.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.GenreName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.GenreCount?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.GenreTag?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)

                );


                return textMatch;
            })
            .OrderByDescending(s => s.GenreName?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.GenreName);
        }
        ObservableCollection<GenreModel> searchresultsgenre = new();

        private void asbSearchGenres_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
            {
                searchresultsgenre.Clear();
                grdNoSearchResultsGenre.Visibility = Visibility.Collapsed;

                grdViewGenres.ItemsSource = genresAll;
                grdViewGenres.Visibility = Visibility.Visible;


                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = GetFilteredResultsGenre(sender.Text);

                searchresultsgenre.Clear();
                foreach (var item in results) searchresultsgenre.Add(item);

                sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };

                Debug.WriteLine("searching in gridview");
                grdViewGenres.ItemsSource = searchresultsgenre;


            }
        }

        private void asbSearchGenres_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var results = GetFilteredResultsGenre(sender.Text);

            if (results.Any())
            {
                grdNoSearchResultsGenre.Visibility = Visibility.Collapsed;

                grdViewGenres.Visibility = Visibility.Visible;


                searchresultsgenre.Clear();
                foreach (var item in results) searchresultsgenre.Add(item);
            }
            else if (genresAll.Count > 0)
            {

                grdViewGenres.Visibility = Visibility.Collapsed;

                grdNoSearchResultsGenre.Visibility = Visibility.Visible;
                frmSearchResultsNOMATCHGenre.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
            }
        }

        private void btnCloseSearchGenre_Click(object sender, RoutedEventArgs e)
        {
            asbSearchGenres.Text = "";
            grdViewGenres.Focus(FocusState.Programmatic);
            asbSearchGenres.ItemsSource = null;
        }
    }
}

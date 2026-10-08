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
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
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
          //  FirstTimeLaunch();
            AllAvailableSongs.CollectionChanged -= AllAvailableSongs_CollectionChanged;
            AllAvailableSongs.CollectionChanged += AllAvailableSongs_CollectionChanged;
            foldersListOpened.CollectionChanged -= FoldersListOpened_CollectionChanged;
            foldersListOpened.CollectionChanged += FoldersListOpened_CollectionChanged;
            PlaylistCreation.CreationCallAdd -= PlaylistCreation_CreationCallAdd;
            PlaylistCreation.CreationCallAdd += PlaylistCreation_CreationCallAdd;

            LoadSettings();

            stkLoading.Visibility = Visibility.Visible;
            LoadFolders();

           
        }
        private async void FirstTimeLaunch()
        {

            grdViewFolders.ItemsSource = foldersListOpened;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var folders = currentSettings.SavedFoldersOpened;
            bool isfirsttimelaunch = currentSettings.IsFirstTimeLaunchMusicLib;
            Debug.WriteLine("DUFUF " + isfirsttimelaunch);
            if (isfirsttimelaunch == true)
            {
                currentSettings.IsFirstTimeLaunchMusicLib = false;
                var userPaths = UserDataPaths.GetDefault();
                foldersListOpened.Add(new FoldersListOpened { FolderName = "Music Folder", FolderPath = userPaths.Music, isChecked = true, Show = true });
                folders.Add(new FoldersListOpened { FolderName = "Music Folder", FolderPath = userPaths.Music, isChecked = true, Show = true });
                foldersListOpened.Add(new FoldersListOpened { FolderName = "Pictures Folder", FolderPath = userPaths.Pictures, isChecked = true, Show = true });
                folders.Add(new FoldersListOpened { FolderName = "Pictures Folder", FolderPath = userPaths.Pictures, isChecked = true, Show = true });
                foldersListOpened.Add(new FoldersListOpened { FolderName = "Videos Folder", FolderPath = userPaths.Videos, isChecked = true, Show = true });
                folders.Add(new FoldersListOpened { FolderName = "Videos Folder", FolderPath = userPaths.Videos, isChecked = true, Show = true });
                foldersListOpened.Add(new FoldersListOpened { FolderName = "Downloads Folder", FolderPath = userPaths.Downloads, isChecked = true, Show = true });
                folders.Add(new FoldersListOpened { FolderName = "Downloads Folder", FolderPath = userPaths.Downloads, isChecked = true, Show = true });
                foldersListOpened.Add(new FoldersListOpened { FolderName = "Documents Folder", FolderPath = userPaths.Documents, isChecked = true, Show = true });
                folders.Add(new FoldersListOpened { FolderName = "Documents Folder", FolderPath = userPaths.Documents, isChecked = true, Show = true });
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }
        private void FoldersListOpened_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (foldersListOpened.Count == 0)
            {
                txtNofoldersadded.Visibility = Visibility.Visible;
            }
            else
            {
                txtNofoldersadded.Visibility = Visibility.Collapsed;

            }
        }

        private void AllAvailableSongs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AllAvailableSongs.Count == 0)
            {
                grdEmptyLibrary.Visibility = Visibility.Visible;
            }
            else
            {
                grdEmptyLibrary.Visibility = Visibility.Collapsed;
            }
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
                playlistsAll.Add(new PlaylistItem { PlaylistId = playlist.PlaylistId, PlaylistName = playlist.PlaylistName, Thumbnail = playlist.Thumbnail, SongsPaths = playlist.SongsPaths });
            }
            Debug.WriteLine("Playlst3");
            stkNoPlaylists.Visibility = Visibility.Collapsed;
            AllPlaylistGroupedCollection.Visibility = Visibility.Visible;


        }
        ObservableCollection<RecentMusicModel> recentMusics = new();
        ObservableCollection<PlaylistItem> playlistsAll = new();
        ObservableCollection<ArtistShow> artistsAll = new();
        ObservableCollection<ArtistDiscAlbumModel> albumsAll = new();
        ObservableCollection<GenreModel> genresAll = new();
        private async void LoadHistory()
        {
            stkLoadingAll.Visibility = Visibility.Visible;

            recentMusics.CollectionChanged += RecentMusics_CollectionChanged;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if(currentSettings.IsMusicHistoryDisabled == true)
            {
                btnDisableHistory.Visibility = Visibility.Collapsed;
                grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                asbRecents.Visibility = Visibility.Collapsed;
                chkSelect.Visibility = Visibility.Collapsed;
                stkDisabledHistory.Visibility = Visibility.Visible;
                btnPlayAll.Visibility = Visibility.Collapsed;
                btnClearHistory.Visibility = Visibility.Collapsed;
                stkEmptyHistory.Visibility = Visibility.Collapsed;
                return;
            }
            var recentmusic = currentSettings.RecentMusic;

            foreach (var item in recentmusic)
            {
                Debug.WriteLine("LOADING HISTORY " + item.SongPath);
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
                item.Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(item.SongPath);


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
            stkLoadingAll.Visibility = Visibility.Collapsed;

        }

        private void RecentMusics_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (recentMusics.Count == 0)
            {
                grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                if (stkDisabledHistory.Visibility == Visibility.Collapsed)
                {
                    stkEmptyHistory.Visibility = Visibility.Visible;
                }
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

        private async void btnAll_Click(object sender, RoutedEventArgs e)
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
                stkNoArtists.Visibility = Visibility.Collapsed;
                ArtistsGroupedCollection.Visibility = Visibility.Visible;
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
                stkNoAlbums.Visibility = Visibility.Collapsed;
                AlbumsGroupedCollection.Visibility = Visibility.Visible;
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

                var genresSongCounts = AllAvailableSongs
                    .GroupBy(song =>
                    {
                        // 1. Determine the genre for each song while keeping the song object intact
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
                            // Fallback if TagLib fails to read the file
                            songGenre = "Unknown Genre";
                        }
                        return songGenre;
                    })
                    .Select(group => new
                    {
                        GenreName = group.Key,         // The string returned by the GroupBy lambda above
                        SongCount = group.Count(),     // Total number of songs in this group
                        SongList = group.ToList()      // This will now correctly be a List of your original song objects
                    })
                    .ToList();                // 3. Merge grouped data into genresAll without causing duplicates
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var genresListFromSettings = currentSettings.GenresList;

                foreach (var genreData in genresSongCounts)
                {
                    foreach(var genree in genreData.SongList)
                    {
                        Debug.WriteLine(genree.FilePath + "  isshdhd shfgendre to "+ genreData.GenreName);
                    }
                    var itemStringCount = $"• {genreData.SongCount} {(genreData.SongCount == 1 ? "item" : "items")}";

                    // Check if this genre was already added via presets
                    var existingGenre = genresAll.FirstOrDefault(g => g.GenreName.Equals(genreData.GenreName, StringComparison.OrdinalIgnoreCase));

                    if (existingGenre != null)
                    {
                        // Duplicate prevented! Just update the item count for the existing preset
                        existingGenre.GenreCount = itemStringCount;
                        Debug.WriteLine("existing  genree ");
                        // Optional: Update thumbnail from settings if available
                        var existGenreSettings = genresListFromSettings?.FirstOrDefault(p => p.GenreName == existingGenre.GenreName);
                        if (existGenreSettings != null)
                        {
                            existingGenre.GenreCover = existGenreSettings.GenreCover;
                        }
                        existingGenre.Songs = genreData.SongList;
                    }
                    else
                    {
                        Debug.WriteLine("no existing genree ");

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
                            GenreCover = fallbackUri,
                            Songs = genreData.SongList
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
        private async Task LoadAllFilesAsync()
        {
            stkLoading.Visibility = Visibility.Visible;
            await Task.Yield(); // Allow spinner to draw

            bool includeSubDirs = chkIncludeSubDirectories.IsChecked == true;

            var excludedPaths = foldersListOpened
                .Where(f => !f.isChecked && !string.IsNullOrWhiteSpace(f.FolderPath))
                .Select(f => f.FolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar)
                .ToList();

            var activeFolders = foldersListOpened
                .Where(f => f.isChecked && !string.IsNullOrWhiteSpace(f.FolderPath))
                .Select(f => f.FolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar)
                .ToList();

            var rawFiles = FilesInDatabase.rawSongs;

            // 1. Background filtering
            var matchingVideos = await Task.Run(() =>
            {
                var resultList = new List<SongModel>();
                var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var validExtensions = new HashSet<string>(AudioExtensions.List, StringComparer.OrdinalIgnoreCase);

                foreach (var raw in rawFiles)
                {
                    if (string.IsNullOrEmpty(raw.FilePath)) continue;
                    if (!validExtensions.Contains(Path.GetExtension(raw.FilePath))) continue;
                    if (excludedPaths.Any(ex => raw.FilePath.StartsWith(ex, StringComparison.OrdinalIgnoreCase))) continue;

                    bool isMatch = includeSubDirs
                        ? activeFolders.Any(af => raw.FilePath.StartsWith(af, StringComparison.OrdinalIgnoreCase))
                        : activeFolders.Any(af => string.Equals(Path.GetDirectoryName(raw.FilePath), af.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));

                    if (isMatch && seenPaths.Add(raw.FilePath))
                    {
                        resultList.Add(new SongModel
                        {
                            Title = raw.Title,
                            FilePath = raw.FilePath,
                            SongDuration = raw.SongDuration,
                            Genre = raw.Genre,
                            Artist = raw.Artist,
                            AlbumName = raw.AlbumName
                        });
                    }
                }
                return resultList;
            });

            // 2. DETACH the control to prevent UI thrashing
            AllMusicGroupedCollection.ItemsSource = null!;

            // 3. Clear and fill the collection instantly
            AllAvailableSongs.Clear();
            foreach (var item in matchingVideos)
            {
                AllAvailableSongs.Add(item);
            }

            // 4. RE-ATTACH: GroupedCollectionView processes the entire list in ONE single pass
            AllMusicGroupedCollection.ItemsSource = AllAvailableSongs;

            txtMusicCount.Text = $"• {AllAvailableSongs.Count} {(AllAvailableSongs.Count == 1 ? "item" : "items")}";
            grdEmptyLibrary.Visibility = (AllAvailableSongs.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            AllMusicGroupedCollection.Visibility = (AllAvailableSongs.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
            stkLoading.Visibility = Visibility.Collapsed;
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
            grdViewFolders.ItemsSource = foldersListOpened;
            // Assigning the system paths to the Tag property

            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            var folders = currentSettings.SavedFoldersOpened;
            var folderMappings = new (bool IsEnabled, string Path)[]
    {
    (currentSettings.IsDocumentEnabled, paths.Documents),
    (currentSettings.IsDownloadsEnabled, paths.Downloads),
    (currentSettings.IsPicturesEnabled, paths.Pictures),
    (currentSettings.IsVideosEnabled, paths.Videos),
    (currentSettings.IsMusicEnabled, paths.Music),
    };

            foreach (var (isEnabled, path) in folderMappings)
            {
                if (isEnabled)
                {
                    foldersListOpened.Add(new FoldersListOpened { FolderPath = path, IsHighLevelFolder = true, FolderName = Path.GetFileName(path), isChecked = true });
                }
            }
            foreach (var item in currentSettings.SavedFoldersOpened)
            {
                if (item.Show == true)
                {
                    var exist = foldersListOpened.FirstOrDefault(p => p.FolderPath == item.FolderPath);
                    if (exist == null)
                    {
                        item.FolderName = Path.GetFileName(item.FolderPath);
                        foldersListOpened.Add(item);
                    }

                }
                else
                {
                    Debug.WriteLine("item show = false " + item.FolderPath);
                    var exist = foldersListOpened.FirstOrDefault(p => p.FolderPath == item.FolderPath);
                    if (exist != null)
                    {
                        if (exist.Show == false)
                        {

                            foldersListOpened.Remove(item);
                        }
                    }
                }
            }

            AllAvailableSongs.Clear();
            await LoadAllFilesAsync();
            stkLoading.Visibility = Visibility.Collapsed;
        }
        //private async void LoadAllFiles()
        //{
        //    try
        //    {
        //        bool includeSubDirs = chkIncludeSubDirectories.IsChecked == true;

        //        AllAvailableSongs.Clear();
        //        var rawfiles = FilesInDatabase.rawSongs;
        //        foreach (var folder in foldersListOpened)
        //        {
        //            var audiofiles = rawfiles.Where(p => !string.IsNullOrEmpty(p.FilePath) && AudioExtensions.List.Contains(Path.GetExtension(p.FilePath).ToLowerInvariant()) && Path.GetDirectoryName(p.FilePath) == folder.FolderPath);

        //            foreach (var videof in audiofiles)
        //            {
        //                if (AllAvailableSongs.Any(v => string.Equals(v.FilePath, videof.FilePath, StringComparison.OrdinalIgnoreCase)))
        //                {
        //                    continue;
        //                }
        //                Debug.WriteLine("ADDING FILE: " + videof.FilePath);
        //                AllAvailableSongs.Add(new SongModel { Title = videof.Title, FilePath = videof.FilePath, SongDuration = videof.SongDuration, Artist = videof.Artist, AlbumName = videof.AlbumName, Genre = videof.Genre });
        //            }
        //        }
        //        txtMusicCount.Text = $"• {AllAvailableSongs.Count} {(AllAvailableSongs.Count == 1 ? "item" : "items")}";
        //        grdEmptyLibrary.Visibility = (AllAvailableSongs.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
        //        AllMusicGroupedCollection.Visibility = (AllAvailableSongs.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
        //    }
        //    finally
        //    {
        //        stkLoading.Visibility = Visibility.Collapsed;
        //    }
        //}

        ObservableCollection<FoldersListOpened> foldersListOpened = new();
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
            //Add folders to list
            var obser = new ObservableCollection<SongModel>();

            if (App.MainWindowInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Choose Folder", Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
            if (folder != null)
            {
                // Detach old handlers if any, and attach a specific one for THIS button

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                Debug.WriteLine("folder add");
                var check = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                var savedfolders = currentSettings.SavedFoldersOpened;
                if (check == null)
                {
                    Debug.WriteLine("folder add NULL CHECK");

                    savedfolders.Add(new FoldersListOpened { FolderPath = folder.Path, isChecked = true, FolderName = Path.GetFileName(folder.Path) });
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    Debug.WriteLine("folder add NULL CHECK test");
                    var alreadyexist = foldersListOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                    if (alreadyexist != null) return;

                    foldersListOpened.Add(new FoldersListOpened { FolderPath = folder.Path, FolderName = Path.GetFileName(folder.Path), isChecked = true });

                }
            }
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
            //Remove Folder from the List
            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {
                foldersListOpened.Remove(tgl);
                UserDataPaths paths = UserDataPaths.GetDefault();

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                if (tgl.IsHighLevelFolder)
                {
                    bool isChecked = false;

                    switch (tgl.FolderPath)
                    {
                        case var p when p == paths.Documents:
                            currentSettings.IsDocumentEnabled = isChecked;
                            break;
                        case var p when p == paths.Downloads:
                            currentSettings.IsDownloadsEnabled = isChecked;
                            break;
                        case var p when p == paths.Pictures:
                            currentSettings.IsPicturesEnabled = isChecked;
                            break;
                        case var p when p == paths.Videos:
                            currentSettings.IsVideosEnabled = isChecked;
                            break;
                        case var p when p == paths.Music:
                            currentSettings.IsMusicEnabled = isChecked;
                            break;
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);

                }
                else
                {
                    if (tgl.FolderPath is string str)
                    {
                        var folder = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == str);
                        if (folder != null)
                        {
                            Debug.WriteLine(str);
                            currentSettings.SavedFoldersOpened.Remove(folder);
                        }
                        await SettingsLoader.SaveSettingsAsync(currentSettings);
                    }
                }

                AllAvailableSongs.Clear();
                await LoadAllFilesAsync();
            }
        }

        private async void btnGenericFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tgl && tgl.DataContext is FoldersListOpened folder)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                UserDataPaths paths = UserDataPaths.GetDefault();
                bool isChecked = tgl.IsChecked ?? false;

                // 1. Update in-memory state
                folder.isChecked = isChecked;

                // 2. Persist to settings
                if (folder.IsHighLevelFolder)
                {
                    switch (folder.FolderPath)
                    {
                        case var p when p == paths.Documents: currentSettings.IsDocumentEnabled = isChecked; break;
                        case var p when p == paths.Downloads: currentSettings.IsDownloadsEnabled = isChecked; break;
                        case var p when p == paths.Pictures: currentSettings.IsPicturesEnabled = isChecked; break;
                        case var p when p == paths.Videos: currentSettings.IsVideosEnabled = isChecked; break;
                        case var p when p == paths.Music: currentSettings.IsMusicEnabled = isChecked; break;
                    }
                }
                else
                {
                    var saved = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.FolderPath);
                    if (saved != null)
                    {
                        saved.isChecked = isChecked;
                    }
                }

                await SettingsLoader.SaveSettingsAsync(currentSettings);

                // 3. Re-run scan to honor exclusions and checked states
                await LoadAllFilesAsync();
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

        private async void btnDisableHistory_Click(object sender, RoutedEventArgs e)
        {

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsMusicHistoryDisabled = true;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            btnDisableHistory.Visibility = Visibility.Collapsed;
            asbRecents.Visibility = Visibility.Collapsed;
            chkSelect.Visibility = Visibility.Collapsed;
            grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
            stkDisabledHistory.Visibility = Visibility.Visible;
            btnPlayAll.Visibility = Visibility.Collapsed;
            btnClearHistory.Visibility = Visibility.Collapsed;
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

        
        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("PALSY");
            var recentmusic = recentMusics;
            if (recentmusic.Count == 0) return;
            ObservableCollection<SongModel> tempTransfer = new();
            foreach (var item in recentmusic)
            {
                Debug.WriteLine("ITEM FILE APTHS " + item.SongPath);
                if (File.Exists(item.SongPath))
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
            //Check box for including sub-directories
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            stkLoading.Visibility = Visibility.Visible;
            if (chkIncludeSubDirectories.IsChecked == true)
            {
                currentSettings.IncludeSubDirMusLib = true;
            }
            else
            {
                currentSettings.IncludeSubDirMusLib = false;
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);

            LoadFolders();
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

        private void btnPlayAllMusic_Click(object sender, RoutedEventArgs e)
        {
            QueueService.PlayMedia(AllAvailableSongs, false, false);
        }

        private void mnftPlayShuffled_Click(object sender, RoutedEventArgs e)
        {
            QueueService.PlayMedia(AllAvailableSongs, true, false);

        }

        private void mnftPlayOnLoop_Click(object sender, RoutedEventArgs e)
        {
            QueueService.PlayMedia(AllAvailableSongs, false, true);

        }

        private void mnftPlayOnLoopShuffled_Click(object sender, RoutedEventArgs e)
        {
            QueueService.PlayMedia(AllAvailableSongs, true, true);
        }

        private async void mnftPlayGenre_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                Debug.WriteLine(genre.Songs.Count + " is hthe cout");
                foreach (var path in genre.Songs)
                {
                    Debug.WriteLine("SHD " + path);
                    var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, false, false);
            }

        }

        private async void mnftPlayGenreShuffled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in genre.Songs)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, true, false);
            }
        }

        private async void mnftPlayGenreLoop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in genre.Songs)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, false, true);
            }

        }

        private async void mnftPlayGenreLoopShuffle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in genre.Songs)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, true, true);
            }
        }

        private async void mnftRemoveGenre_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                Debug.WriteLine("REMOVE GENRE " + genre.GenreName);
                var songlist = new List<string>();

                // .ToList() creates a copy, so we aren't mutating the collection we are looping through
                foreach (var song in genre.Songs.ToList())
                {
                    Debug.WriteLine("REMOVE GENRE for " + song.FilePath);

                    songlist.Add(song.FilePath);
                    genre.Songs.Remove(song); // Safe because genre.Songs isn't the loop target anymore
                }
                var artistsongcount = $"• {genre.Songs.Count} {(genre.Songs.Count == 1 ? "item" : "items")}";

                genre.GenreCount = artistsongcount;
                Debug.WriteLine("REMOV EGENRE CONTINUE");

                AudioMetadata.ChangeGenre(songlist, "Unknown Genre");
              await  LoadGenres();
            }
        }

        private void grdViewGenres_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is GenreModel genre)
            {
                if (App.NavigationFrame != null)
                {
               
                    App.NavigationFrame.Navigate(typeof(GenreView), genre);
                }
            }
        }

        private void mnftOpenGenre_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GenreModel genre)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(GenreView), genre);
                }
            }
        }

        private void MenuFlyout_Opened(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            if (flyout == null) return;




            var mnftPlay = flyout?.Items
        .OfType<MenuFlyoutItem>()
        .FirstOrDefault(x => x.Text == "Play");

            var mnftPlayShuffled = flyout?.Items
     .OfType<MenuFlyoutItem>()
     .FirstOrDefault(x => x.Text == "Play Shuffled");

            if (mnftPlayShuffled == null)
                return;

            var mnftPlayonLoop = flyout?.Items
  .OfType<MenuFlyoutItem>()
  .FirstOrDefault(x => x.Text == "Play on Loop");

            if (mnftPlayonLoop == null)
                return;


            var mnftPlayonShuffleLoop = flyout?.Items
  .OfType<MenuFlyoutItem>()
  .FirstOrDefault(x => x.Text == "Play Shuffled and on Loop");

            if (mnftPlayonShuffleLoop == null)
                return;
            var selectedgenre = mnftPlay?.DataContext as GenreModel;
            if (selectedgenre == null) return;
            if(selectedgenre.Songs.Count == 0)
            {
                if (mnftPlay == null)
                    return;

                mnftPlay.IsEnabled = false;
                mnftPlayShuffled.IsEnabled = false;
                mnftPlayonLoop.IsEnabled = false;
                mnftPlayonShuffleLoop.IsEnabled = false;
            }
            else
            {
                if (mnftPlay == null)
                    return;

                mnftPlay.IsEnabled = true;
                mnftPlayShuffled.IsEnabled = true;
                mnftPlayonLoop.IsEnabled = true;
                mnftPlayonShuffleLoop.IsEnabled = true;
            }
        }

        private async void btnEnableHistory_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsMusicHistoryDisabled = false;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            btnDisableHistory.Visibility = Visibility.Visible;
            asbRecents.Visibility = Visibility.Visible;
            chkSelect.Visibility = Visibility.Visible;
            grdViewAllRecentMusic.Visibility = Visibility.Visible;
            stkDisabledHistory.Visibility = Visibility.Collapsed;
            btnPlayAll.Visibility = Visibility.Visible;
            btnClearHistory.Visibility = Visibility.Visible;
            LoadHistory();
        }

        private void AllPlaylistGroupedCollection_playlistcollectionchanged()
        {
            stkNoPlaylists.Visibility = Visibility.Visible;
            AllPlaylistGroupedCollection.Visibility = Visibility.Collapsed;
        }
        PlaylistCreationValues Instance => PlaylistCreationValues.Instance;

        bool iscreating = false;
        private  void btnNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Playlist", "Create", "", "Cancel", OceanDialogWindow.ContentType.PlaylistCreation, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "Playlist", "", "", "", "", new PlaylistItem(), false, false);
            Debug.WriteLine("BTNNEWPLAYLIST");
            OceanContentDialog.PrimaryRequested += (async () =>
            {
                if (iscreating) return;
                try
                {
                    iscreating = true;
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    string baseName = Instance.PlaylistName.Trim();

                    if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                    string finalName = baseName;
                    int counter = 1;
                    while (currentSettings.SavedPlaylists.Any(p =>
                        string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalName = $"{baseName} ({counter++})";
                    }
                    var newplaylist = new PlaylistItem { PlaylistName = finalName, PlaylistGenre = Instance.Genre, plthumb = Instance.PlCover, PlaylistId = Instance.PlaylistID, DateCreation = Instance.CreationDate, SongsPaths = new HashSet<string>(Instance.MediaPaths), Thumbnail = Instance.Thumbnail };
                    Debug.WriteLine(newplaylist.PlaylistName);
                    Instance.PlaylistsMaster.Add(newplaylist);

                    currentSettings.SavedPlaylists.Add(newplaylist);
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                }
                catch (Exception ex)
                {
                    Logger.Log("An unexpected error occured: " + ex.Message, "PlaylistCreate.MusicLibPage", Logger.LogLevelType.Error);
                }
                finally
                {
                    iscreating = false;
                }
            });

        }

        private void OceanContentDialog_PrimaryRequested()
        {
            PlaylistCreation.CallPlaylistCreation();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();

        }
        private bool _isSavingPlaylist = false;

        private async void PlaylistCreation_CreationCallAdd()
        {
            if (_isSavingPlaylist) return;
            _isSavingPlaylist = true;
            try
            {
                if (PlaylistCreation.playlistItem != null)
                {
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    if (PlaylistCreation.playlistItem.PlaylistName is string name)
                    {
                        string baseName = name.Trim();

                        if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                        string finalName = baseName;
                        int counter = 1;
                        while (currentSettings.SavedPlaylists.Any(p =>
                            string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                        {
                            finalName = $"{baseName} ({counter++})";
                        }
                        PlaylistCreation.playlistItem.PlaylistName = finalName;
                    }

                    MasterSearchIndex.PlaylistsMaster.Add(PlaylistCreation.playlistItem);

                    currentSettings.SavedPlaylists.Add(PlaylistCreation.playlistItem);
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    LoadAllPlaylists();
                }
            }
            finally
            {
                _isSavingPlaylist = false;
            }
        }

        private void ArtistsGroupedCollection_artistcollectionchanged()
        {
            stkNoArtists.Visibility = Visibility.Visible;
            ArtistsGroupedCollection.Visibility = Visibility.Collapsed;
        }

        private void AlbumsGroupedCollection_albumcollectionchanged()
        {
            stkNoAlbums.Visibility = Visibility.Visible;
            AlbumsGroupedCollection.Visibility = Visibility.Collapsed;
        }
    }
}


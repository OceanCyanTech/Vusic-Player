using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Vusic_Player.Configuration.ClassModels;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Storage;
using Vusic_Player.Extensions;
using Windows.Storage.Search;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Configuration.AppConfig;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Diagnostics;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Playback;
using CommunityToolkit.WinUI;
using Windows.Storage.FileProperties;


namespace Vusic_Player.Pages.Views;

public sealed partial class ArtistView : Page
{
    public ArtistView()
    {
        InitializeComponent();
    }

    private async void FoundSongs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (FoundSongs.Count == 0)
        {
            txtEmptySongs.Visibility = Visibility.Visible;
            lstViewAllSongs.Visibility = Visibility.Collapsed;
            btnPlayAll.IsEnabled = false;
            btnShuffle.IsEnabled = false;
            btnRenameArtist.IsEnabled = false;

        }
        else
        {
            txtEmptySongs.Visibility = Visibility.Collapsed;
            lstViewAllSongs.Visibility = Visibility.Visible;
            btnPlayAll.IsEnabled = true;
            btnShuffle.IsEnabled = true;
            btnRenameArtist.IsEnabled = true;
        }
        ts = TimeSpan.Zero;
        foreach (var item in FoundSongs.ToList())
        {
            var Storagefile = await StorageFile.GetFileFromPathAsync(item.FilePath);
            var props = await Storagefile.Properties.GetMusicPropertiesAsync();
            ts += props.Duration;
        }
        string formatted = ts.TotalHours >= 1
    ? ts.ToString(@"h\:mm\:ss")
    : ts.ToString(@"m\:ss");
        txtTotalDuration.Text = "• " + formatted;
        txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "item" : "items")}";
    }
    TimeSpan ts;
    public ObservableCollection<SongModel> FoundSongs { get; set; } = new ObservableCollection<SongModel>();
    ObservableCollection<ArtistDiscAlbumModel> albumCollection = new ObservableCollection<ArtistDiscAlbumModel>();
    public ObservableCollection<SongModel> Singles { get; set; } = new ObservableCollection<SongModel>();
    HashSet<string> uniqueArtists = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    ObservableCollection<string> AlbumsList { get; set; } = new();
    private bool _isRenaming = false;
    private async Task SearchFiles()
    {
        string targetArtist = txtArtistName.Text;

        FoundSongs.Clear();
        uniqueArtists.Clear();
        albumCollection.Clear();
        Singles.Clear();
        AlbumsList.Clear();
        ts = TimeSpan.Zero;

        // 🔹 Progress UI start
        ttProgress.IsOpen = true;
        prgProgress.IsIndeterminate = true;
        prgProgress.Value = 0;

        string[] searchPaths =
        {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos
    };

        List<StorageFile> allFoundFiles = new List<StorageFile>();

        // 🔹 STEP 1: Collect files
        foreach (var path in searchPaths)
        {
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);

                var queryOptions = new QueryOptions(
                    CommonFileQuery.OrderByMusicProperties,
                   AudioExtensions.List);


                var query = folder.CreateFileQueryWithOptions(queryOptions);
                var files = await query.GetFilesAsync();

                allFoundFiles.AddRange(files);
            }
            catch
            {
                // Ignore access denied
            }
        }

        // 🔹 STEP 2: Process files
        if (allFoundFiles.Count > 0)
        {
            prgProgress.IsIndeterminate = false;
            prgProgress.Maximum = allFoundFiles.Count;

            int processedCount = 0;

            foreach (var file in allFoundFiles)
            {
                try
                {

                    var storagefile = await StorageFile.GetFileFromPathAsync(file.Path);
                    var musicprops = await storagefile.Properties.GetMusicPropertiesAsync();
                    var tagFile = TagLib.File.Create(file.Path);
                    var tag = tagFile.Tag;
                    bool isMatch = (tag.AlbumArtists != null && tag.AlbumArtists.Contains(targetArtist, StringComparer.OrdinalIgnoreCase)) ||
                   (tag.Performers != null && tag.Performers.Contains(targetArtist, StringComparer.OrdinalIgnoreCase) || musicprops.Artist.Contains(targetArtist, StringComparison.OrdinalIgnoreCase));

                    if (!isMatch)
                    {
                        tagFile.Dispose(); // Clean up if we skip
                        continue;
                    }
                    string artistName = !string.IsNullOrWhiteSpace(tag.FirstAlbumArtist)
                        ? tag.FirstAlbumArtist
                        : tag.FirstPerformer;
                    if (!string.IsNullOrEmpty(artistName))
                        uniqueArtists.Add(artistName);

                    ts += musicprops.Duration;

                    // 🔥 Extract year ONCE (IMPORTANT)
                    int year = (int)tag.Year;
                    if (year == 0)
                        year = File.GetCreationTime(file.Path).Year;

                    string albumName = string.IsNullOrWhiteSpace(tag.Album)
                        ? "Unknown Album"
                        : tag.Album;
                    string artists = "Unknown Artist";

                    if (tag.AlbumArtists != null && tag.AlbumArtists.Length > 0)
                    {
                        artists = string.Join(", ", tag.AlbumArtists);
                    }
                    else if (tag.Performers != null && tag.Performers.Length > 0)
                    {
                        // This maps to "Contributing Artist" in Windows Properties
                        artists = string.Join(", ", tag.Performers);
                    }
                    AlbumsList.Add(albumName);
                    var colorbrush = new SolidColorBrush(Microsoft.UI.Colors.White);
                    var glyph = "\uEC4F";
                    if (PlayerService.CurrentPlayingPath == file.Path)
                    {
                        colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
                        if (PlayerService.Masterplayer!.IsPlaying)
                            glyph = "\uE769";
                        else
                        {
                            glyph = "\uE768";
                        }
                    }
                    var settings = await SettingsLoader.LoadSettingsAsync();
                    var favourites = settings.Favourites;
                    var favSet = new HashSet<FavouriteItems>(favourites);
                    bool isfav = favSet.Any(f => f.FilePath == file.Path);
                    double opac = isfav ? 1.0 : 0.0;
                    string text = isfav ? "Remove from Favourites" : "Add to Favourites";
                    string filenamepath = Path.GetFileNameWithoutExtension(file.Path);
                    var song = new SongModel
                    {
                        Title = string.IsNullOrEmpty(musicprops.Title) ? filenamepath : musicprops.Title,
                        Artist = artists,
                        AlbumName = albumName,
                        SongDuration = musicprops.Duration,
                        FilePath = file.Path,
                        Year = year,
                        Remove = "Remove from artist",
                        MediaType = "ArtistAll",
                        FavOpacity = opac,
                        FavString = text,
                        Glyph = glyph,
                        IsFavourite = favSet.Any(f => f.FilePath == file.Path),
                        TitleColor = colorbrush,
                    };

                    FoundSongs.Add(song);

                    // ✅ Add to Singles if no album
                    if (albumName == "Unknown Album")
                    {
                        Singles.Add(song);
                    }
                    processedCount++;
                    prgProgress.Value = processedCount;
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, "ArtistPage.Load", Logger.LogLevelType.Error);
                }
            }
        }
        lstViewSingles.ItemsSource = Singles;
        // 🔹 Load songs into UI ONCE
        lstViewAllSongs.ItemsSource = FoundSongs;

        // 🔹 STEP 3: Group albums
        var groupedAlbums = FoundSongs
            .GroupBy(s => s.AlbumName)
            .ToList();
        var knownAlbums = groupedAlbums
.Where(g => g.Key != "Unknown Album")
.ToList();

        var unknownSongs = groupedAlbums
            .Where(g => g.Key == "Unknown Album")
            .SelectMany(g => g)
            .ToList();


        foreach (var album in groupedAlbums)
        {
            var songs = album.ToList();

            int countsongs = songs.Count;

            int mostCommonYear = songs
                .Select(s => s.Year)
                .Where(y => y > 0)
                .GroupBy(y => y)
                .OrderByDescending(g => g.Count())
                .ThenBy(g => g.Key)
                .Select(g => g.Key)
                .FirstOrDefault();

            string countsong =
                $"{countsongs} {(countsongs == 1 ? "item" : "items")}";

            string yearstring =
                mostCommonYear > 0 ? mostCommonYear.ToString() : "";

            BitmapImage img = await LoadExistingThumbnailAsync(album.Key ?? "Unknown Album");

            albumCollection.Add(new ArtistDiscAlbumModel
            {
                AlbumName = album.Key ?? "Unknown Album",
                AlbumCount = countsong,
                AlbumCoverThumbnail = img,
                AlbumYear = yearstring
            });
        }
        grdViewAlbums.ItemsSource = albumCollection;

        // 🔹 Total duration
        string formatted = ts.TotalHours >= 1
            ? ts.ToString(@"h\:mm\:ss")
            : ts.ToString(@"m\:ss");

        txtTotalDuration.Text = formatted;

        // 🔹 Song count
        int count = FoundSongs.Count;
        txtSongCount.Text =
            $"• {count} {(count == 1 ? "item" : "items")}";
        int count2 = albumCollection.Count;
        txtAlbumCount.Text =
         $"• {count2} {(count2 == 1 ? "Album" : "Albums")}";
        var
            edArtists = uniqueArtists.OrderBy(a => a);
        if (Singles.Count == 0)
        {
            txtEmptySingles.Visibility = Visibility.Visible;
            lstViewSingles.Visibility = Visibility.Collapsed;
        }
        else
        {
            txtEmptySingles.Visibility = Visibility.Collapsed;
            lstViewSingles.Visibility = Visibility.Visible;
        }
        if (FoundSongs.Count == 0)
        {
            txtEmptySongs.Visibility = Visibility.Visible;
        }
        else
        {
            txtEmptySongs.Visibility = Visibility.Collapsed;

        }
        if (albumCollection.Count == 0)
        {
            txtEmptyAlbums.Visibility = Visibility.Visible;
        }
        else
        {
            txtEmptyAlbums.Visibility = Visibility.Collapsed;

        }
        LoadMostPlayedSongs();

        await Task.Delay(500);
        FoundSongs.CollectionChanged -= FoundSongs_CollectionChanged;
        FoundSongs.CollectionChanged += FoundSongs_CollectionChanged;

        ttProgress.IsOpen = false;
    }
    ObservableCollection<SongModel> mostplayedsongs = new();

    private async void LoadMostPlayedSongs()
    {

        mostplayedsongs.Clear();
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var mostPlayed = currentSettings.RecentMusic;

        if (mostPlayed != null)
        {
            // Sort by PlayCount in descending order (highest first)
            // Then use .ToList() or simply iterate over the sorted collection
            var sortedSongs = mostPlayed.OrderByDescending(x => x.PlayCount).Take(5);
            foreach (var item in sortedSongs)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(item.SongPath);
                MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();
                string title = !string.IsNullOrWhiteSpace(properties.Title) ? properties.Title : Path.GetFileNameWithoutExtension(file.Path);
                string album = !string.IsNullOrWhiteSpace(properties.Album) ? properties.Album : "Unknown Album";
                string artist = !string.IsNullOrWhiteSpace(properties.Artist) ? properties.Artist : "Unknown Artist";
                var filetag = TagLib.File.Create(item.SongPath);
                var Artists = filetag.Tag.AlbumArtists;

                var favourites = currentSettings.Favourites;
                var favSet = new HashSet<FavouriteItems>(favourites);
                bool isfav = favSet.Any(f => f.FilePath == file.Path);
                double opac = isfav ? 1.0 : 0.0;
                var colorbrush = new SolidColorBrush(Microsoft.UI.Colors.White);
                var glyph = "\uEC4F";
                if (PlayerService.CurrentPlayingPath == item.SongPath)
                {
                    colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
                    if (PlayerService.Masterplayer!.IsPlaying)
                        glyph = "\uE769";
                    else
                    {
                        glyph = "\uE768";
                    }
                }
                string text = isfav ? "Remove from Favourites" : "Add to Favourites";
                if (Artists.Contains(txtArtistName.Text, StringComparer.OrdinalIgnoreCase))
                {
                    var SongModelt = new SongModel
                    {
                        Title = title,
                        AlbumName = album,
                        Artist = artist,
                        FilePath = item.SongPath,
                        FavOpacity = opac,
                        FavString = text,
                        SongDuration = properties.Duration,
                        IsFavourite = favSet.Any(f => f.FilePath == file.Path),
                        Glyph = glyph,
                        TitleColor = colorbrush,
                        Remove = "Remove from History",
                        MediaType = "ArtistMP",
                        IsMovableItem = Visibility.Collapsed,
                    };
                    mostplayedsongs.Add(SongModelt);
                }
            }

            lstViewMasterMostPlayed.ItemsSource = mostplayedsongs;

        }
        if (mostplayedsongs.Count == 0)
        {
            lstViewMasterMostPlayed.Visibility = Visibility.Collapsed;
            txtEmptyMostPlayed.Visibility = Visibility.Visible;
        }
        else
        {
            lstViewMasterMostPlayed.Visibility = Visibility.Visible;
            txtEmptyMostPlayed.Visibility = Visibility.Collapsed;
        }
    }

    private async Task<BitmapImage> LoadExistingThumbnailAsync(string albumname)
    {
        Uri fallbackUri = new Uri("ms-appx:///Assets/defaultalbum.png");

        var currentSettings = await SettingsLoader.LoadSettingsAsync();

        var existingAlbum = currentSettings.AlbumsList?
            .FirstOrDefault(a => a.Name == albumname);

        if (existingAlbum != null && !string.IsNullOrEmpty(existingAlbum.Thumbnail))
        {
            try
            {
                return new BitmapImage(new Uri(existingAlbum.Thumbnail));
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to load thumbnail, reverting to default: {ex.Message}",
                    "AlbumPage", Logger.LogLevelType.Error);

                return new BitmapImage(fallbackUri);
            }
        }

        return new BitmapImage(fallbackUri);
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string selectedSongArtist)
        {
            txtArtistName.Text = selectedSongArtist;
            imgArtist.DisplayName = txtArtistName.Text;
            await SearchFiles();
            Uri fallbackUri = new Uri("ms-appx:///Assets/defaultartist.png");
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var existingArtist = currentSettings.ArtistsList?
                .FirstOrDefault(a => a.Name == txtArtistName.Text);
            if (existingArtist != null && !string.IsNullOrEmpty(existingArtist.Thumbnail))
            {
                try
                {
                    imgArtist.ProfilePicture = new BitmapImage(new Uri(existingArtist.Thumbnail));
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to load thumbnail, reverting to default: {ex.Message}", "ArtistPage", Logger.LogLevelType.Error);
                    imgArtist.ProfilePicture = new BitmapImage(fallbackUri);
                }
            }
            else
            {

                imgArtist.ProfilePicture = new BitmapImage(fallbackUri);
            }
        }
        base.OnNavigatedTo(e);
    }
    private void imgArtist_Tapped(object sender, TappedRoutedEventArgs e)
    {
        Debug.WriteLine("Artist image tapped.");
    }

    private async void btnSetArtistProfilePicture_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance == null) return;
        var file = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.MainWindowInstance, "Choose Profile Picture for Artist");

        if (file != null)
        {
            imgArtist.ProfilePicture = new BitmapImage(new Uri(file.Path));
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var artists = currentSettings.ArtistsList;
            var existingArtist = artists.FirstOrDefault(a => a.Name == txtArtistName.Text);

            if (existingArtist != null)
            {
                existingArtist.Thumbnail = file.Path;
            }
            else
            {
                var newArtist = new ArtistModel
                {
                    Name = txtArtistName.Text,
                    Thumbnail = file.Path
                };
                artists.Add(newArtist);
            }
            mnftRemoveImage.Visibility = Visibility.Visible;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
    }

    private async void mnftRemoveImage_Click(object sender, RoutedEventArgs e)
    {
        imgArtist.ProfilePicture = new BitmapImage(new Uri("ms-appx:///Assets/defaultartist.png"));
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var artists = currentSettings.ArtistsList;
        var existingArtist = artists.FirstOrDefault(a => a.Name == txtArtistName.Text);

        if (existingArtist != null)
        {
            // 1. Update the existing entry
            existingArtist.Thumbnail = "";
        }
        else
        {
            // 2. Create a new entry if it doesn't exist
            var newArtist = new ArtistModel
            {
                Name = txtArtistName.Text,
                Thumbnail = ""
                // Add other default properties here
            };
            artists.Add(newArtist);
        }

        // Don't forget to save the changes back to storage!
        await SettingsLoader.SaveSettingsAsync(currentSettings);
    }

    private void btnFindArtistProfileOnline_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance == null) return;
        CheckInternet.SetImage -= CheckInternet_SetImage;
        CheckInternet.SetImage += CheckInternet_SetImage;
        OceanContentDialog.Show("Find Artist Picture Online", "Set", "", "Cancel", OceanDialogWindow.ContentType.OnlineArtistPicture, OceanContentDialogDefault.Primary, XamlRoot, 800, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "", "", txtArtistName.Text);

        OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
        OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
    }

    private void CheckInternet_SetImage()
    {
        LoadImage(CheckInternet.UrlToDownload);
    }


    public async void LoadImage(string filePath)
    {
        try
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
            using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);
                imgArtist.ProfilePicture = bitmap;
            }
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var artists = currentSettings.ArtistsList;
            var existingArtist = artists.FirstOrDefault(a => a.Name == txtArtistName.Text);

            if (existingArtist != null)
            {
                existingArtist.Thumbnail = file.Path;
            }
            else
            {
                var newArtist = new ArtistModel
                {
                    Name = txtArtistName.Text,
                    Thumbnail = file.Path
                    // Add other default properties here
                };
                artists.Add(newArtist);
            }

            // Don't forget to save the changes back to storage!
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message, "ArtistImageLoad", Logger.LogLevelType.Error);
        }
    }


    // --- Cropping / ContentDialog Events ---
    private void cropCircle_PointerPressed(object sender, PointerRoutedEventArgs e) { }
    private void cropCircle_PointerMoved(object sender, PointerRoutedEventArgs e) { }
    private void cropCircle_PointerReleased(object sender, PointerRoutedEventArgs e) { }

    // --- Artist Action Buttons (Rename, Play, Shuffle) ---
    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        txtRename.Text = txtArtistName.Text;
    }

    private void txtRename_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb) tb.SelectAll();
    }

    private async void btnRenameArtist_Click(object sender, RoutedEventArgs e)
    {
        if (_isRenaming) return; // Prevent double clicks
        _isRenaming = true;

        // Disable the button or show a loading state if needed
        btnRenameArtist.IsEnabled = false;

        try
        {
            string newArtistName = txtRename.Text;
            var songsToProcess = FoundSongs.ToList();
            foreach (SongModel item in FoundSongs.ToList())
            {
                try
                {
                    if (item.FilePath != null)
                    {
                        var filelocked = GetLockingProcess.GetLockingProcesses(item.FilePath);
                        if (filelocked.Count == 0)
                        {
                            var file = TagLib.File.Create(item.FilePath);
                            file.Tag.AlbumArtists = new[] { newArtistName };
                            file.Save();
                            file.Dispose(); // Important: Release TagLib handle
                        }
                        else
                        {
                            bool onlyVusicPlayer = filelocked.All(p => p.ProcessName == "Vusic Player");

                            if (onlyVusicPlayer)
                            {
                                if (PlayerService.Masterplayer != null)
                                {
                                    var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                                    PlayerService.curtime = curTime;
                                    PlayerService.curtimetemp = PlayerService.Masterplayer.CurTime;

                                    if (PlayerService.Masterplayer.Status == FlyleafLib.MediaPlayer.Status.Playing)
                                    {
                                        Debug.WriteLine("TRUEEE");
                                        isPaused2 = false;
                                    }
                                    else
                                    {
                                        isPaused2 = true;
                                    }
                                    PlayerService.filestreamcurrent?.Dispose();
                                    PlayerService.JustDisposed = true;
                                    var filelocked2 = GetLockingProcess.GetLockingProcesses(item.FilePath);
                                    if (filelocked2.Count == 0)
                                    {
                                        try
                                        {
                                            var file = TagLib.File.Create(item.FilePath);
                                            file.Tag.AlbumArtists = new[] { newArtistName };
                                            file.Save();
                                            file.Dispose();

                                            if (isPaused2 == false)
                                            {
                                                Debug.WriteLine("IsPuae");
                                                PlayerService.Play();
                                            }


                                        }
                                        catch (Exception ex)
                                        {
                                            Logger.Log(ex.Message, "ArtistPage.Remove", Logger.LogLevelType.Error);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var processNames = string.Join(", ", filelocked.Select(p => p.ProcessName));
                                if (App.MainWindowInstance == null) return;
                                OceanContentDialog.Show("Error", "Skip", "", "", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 550, 300, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"Unable to remove file because it is in use by {processNames}", "error");
                                OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
                                OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, "ArtistPage.RenameArtist", Logger.LogLevelType.Error);
                }
            }

            // Trigger ONE search/refresh after everything is done
            await SearchFiles();
        }
        finally
        {
            _isRenaming = false;
            btnRenameArtist.IsEnabled = true;
            txtArtistName.Text = txtRename.Text;

            flyoutRename.Hide();
        }

    }
    bool isPaused2 = false;
    private void OceanContentDialog_PrimaryRequested()
    {
        OceanContentDialog.HideDlg();
        MainWindow.ShowWindow();
    }

    private void btnPlayAll_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in FoundSongs)
        {
            item.IsCompleted = false;
        }
        QueueService.PlayMedia(FoundSongs, btnShuffle.IsChecked ?? false, false);

    }

    private void btnShuffle_Checked(object sender, RoutedEventArgs e)
    {
        if (btnShuffle.IsChecked == true)
        {
            QueueService.IsShuffleTrue = true;

            QueueService.ShuffleNext();


        }
        else
        {
            QueueService.IsShuffleTrue = false;

            QueueService.RestoreNext();


        }
    }

    private void btnShuffle_Unchecked(object sender, RoutedEventArgs e)
    {
        if (btnShuffle.IsChecked == true)
        {
            QueueService.IsShuffleTrue = true;

            QueueService.ShuffleNext();


        }
        else
        {
            QueueService.IsShuffleTrue = false;

            QueueService.RestoreNext();


        }
    }

    private async void btnAddSongs_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance == null) return;
        var files = await FilePickers.MediaPicker.PickMultipleAudioFilesAsync(App.MainWindowInstance, "Choose files");

        if (files == null) return;

        ObservableCollection<string> existingPaths = new();
        foreach (var file in files)
        {
            var alreadyexist = FoundSongs.FirstOrDefault(s => s.FilePath == file.Path);
            if (alreadyexist == null)
            {
                var tagFile = TagLib.File.Create(file.Path);
                string[] albumartists = tagFile.Tag.AlbumArtists;
                List<string> artistss = albumartists.ToList();
                var newArtist = txtArtistName.Text;

                if (!string.IsNullOrEmpty(newArtist) &&
                    !artistss.Any(a => a.Equals(newArtist, StringComparison.OrdinalIgnoreCase)))
                {
                    artistss.Add(newArtist);
                    tagFile.Tag.AlbumArtists = artistss.ToArray();
                    tagFile.Save();
                }
            }
        }
        await Task.Delay(1500);
        await SearchFiles();
    }

    // --- Discography & Album Events ---
    private void ifbError_CloseButtonClick(InfoBar sender, object args)
    {
        sender.Visibility = Visibility.Collapsed;
    }

    private async void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        await SearchFiles();
    }

    private void chckSelectAlbums_Checked(object sender, RoutedEventArgs e)
    {
        if (chckSelectAlbums.IsChecked == true)
        {
            stkMultiOptionsAlbums.Visibility = Visibility.Visible;
            grdViewAlbums.SelectionMode = ListViewSelectionMode.Multiple;

        }
        else
        {
            stkMultiOptionsAlbums.Visibility = Visibility.Collapsed;
            grdViewAlbums.SelectionMode = ListViewSelectionMode.None;
        }
    }



    private void btnSelectAll_Click(object sender, RoutedEventArgs e)
    {
        grdViewAlbums.SelectAll();
    }

    private void btnClearSelection_Click(object sender, RoutedEventArgs e)
    {
        grdViewAlbums.DeselectAll();
    }

    // --- GridView (Albums) Interaction ---
    private void grdViewAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        Debug.WriteLine("Album selection changed.");
    }

    private void grdViewAlbums_ItemClick(object sender, ItemClickEventArgs e)
    {
        if (chckSelectAlbums.IsChecked == false)
        {
            if (e.ClickedItem is ArtistDiscAlbumModel album && album.AlbumName is string str)
            {
                if (App.NavigationFrame == null) return;
                var myApp = (App)Application.Current;
                if (myApp.SelectedAlbum == null)
                {
                    myApp.SelectedAlbum = new AlbumContext { Name = str };
                }
                else
                {
                    myApp.SelectedAlbum.Name = str;
                }
                App.NavigationFrame.Navigate(typeof(AlbumView), myApp.SelectedAlbum);

            }
        }
    }

    // --- Album Context Menu (MenuFlyout) ---
    private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel sng && sng.AlbumName is string str)
        {
            if (str != "")
            {
                if (App.NavigationFrame == null) return;
                var myApp = (App)Application.Current;
                if (myApp.SelectedAlbum == null)
                {
                    myApp.SelectedAlbum = new AlbumContext { Name = str };
                }
                else
                {
                    myApp.SelectedAlbum.Name = str;
                }
                App.NavigationFrame.Navigate(typeof(AlbumView), myApp.SelectedAlbum);

            }
        }
    }

    private void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel albumModel && albumModel.AlbumName is string name)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Album", "Close", "", "", OceanDialogWindow.ContentType.AlbumDetails, OceanContentDialogDefault.Primary, XamlRoot, 800, 980, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "", "", "", name);
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested1;
        }
    }

    private async void OceanContentDialog_PrimaryRequested1()
    {

        OceanContentDialog.HideDlg();
        MainWindow.ShowWindow();
        await SearchFiles();


    }

    private void MenuFlyoutItem_Click_3(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("View Album Info clicked.");
    }

    private void mnftChangeAlbumCover_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Change Album Cover clicked.");
    }

    private void mnftFindAlbumCoverOnline_Click(object sender, RoutedEventArgs e)
    {
    }

    private void mnftRemoveAlbum_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Remove Album clicked (from list).");
    }

    private void mnftDeleteAlbum_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine("Delete Album from disk clicked.");
    }


    // --- Missing Multi-Select Action ---
    private async void btnRemoveSelections_Click(object sender, RoutedEventArgs e)
    {
        var selectedItems = grdViewAlbums.SelectedItems.Cast<ArtistDiscAlbumModel>().ToList();

        foreach (var item in selectedItems)
        {
            if (item.AlbumName != null)
            {
                Debug.WriteLine(item.AlbumName);
                await SearchAlbumFiles(item.AlbumName);
            }
        }


        foreach (var item in selectedItems)
        {
            albumCollection.Remove(item);
        }
    }
    private async Task FinishAlbumRemoval()
    {
        foreach (var item in AlbumSongs)
        {
            if (item.FilePath != null)
            {
                var filelocked = GetLockingProcess.GetLockingProcesses(item.FilePath);
                if (filelocked.Count == 0)
                {
                    var file = TagLib.File.Create(item.FilePath);
                    file.Tag.Album = "";
                    file.Save();
                    file.Dispose();

                }
            }
        }
    }
    ObservableCollection<SongModel> AlbumSongs = new();
    private async Task SearchAlbumFiles(string albumname)
    {

        string targetAlbum = albumname;
        AlbumSongs.Clear();


        string[] searchPaths = {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Pictures,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos
    };

        List<StorageFile> allFoundFiles = new List<StorageFile>();

        foreach (var path in searchPaths)
        {
            Debug.WriteLine("JSJS2");

            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                var queryOptions = new QueryOptions(CommonFileQuery.OrderByMusicProperties, AudioExtensions.List);
                queryOptions.ApplicationSearchFilter = $"System.Music.AlbumTitle:=\"{targetAlbum}\"";

                var query = folder.CreateFileQueryWithOptions(queryOptions);
                var files = await query.GetFilesAsync();
                allFoundFiles.AddRange(files);
            }
            catch { /* Handle access denied */ }
        }

        if (allFoundFiles.Count > 0)
        {
            Debug.WriteLine("JSJS");
            foreach (var file in allFoundFiles)
            {
                var tagFile = TagLib.File.Create(file.Path);
                tagFile.Tag.Album = "";
                tagFile.Save();

            }
        }
    }

    // --- Image Large Viewer ---
    // If you plan to handle the ContentDialog showing the large image:
    private async void ShowLargeImage()
    {
        // Note: Your ContentDialog in XAML needs an x:Name="cdLargeImage" 
        // to be called from code-behind
        // await cdLargeImage.ShowAsync();
    }
}

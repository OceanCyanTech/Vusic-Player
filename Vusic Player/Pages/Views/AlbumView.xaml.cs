using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.AppConfig;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Vusic_Player.Configuration;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Navigation;
using System.Diagnostics;
using Windows.Storage;
using Windows.Storage.Search;
using Vusic_Player.Extensions;
using System.Collections.ObjectModel;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.Internet;

namespace Vusic_Player.Pages.Views;

public sealed partial class AlbumView : Page
{
    public ObservableCollection<SongModel> FoundSongs { get; set; } = new ObservableCollection<SongModel>();
    HashSet<string> uniqueArtists = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    TimeSpan ts;
    private bool _isRenaming = false;
    bool isPaused2 = false;

    private ObservableCollection<ArtistShow> ArtistShows { get; set; } = new ObservableCollection<ArtistShow>();
    public AlbumView()
    {
        InitializeComponent();
    }

    private async void FoundSongs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (FoundSongs.Count == 0)
        {
            txtNoSongs.Visibility = Visibility.Visible;
            lstViewMain.Visibility = Visibility.Collapsed;
            btnPlayAll.IsEnabled = false;
            btnShuffle.IsEnabled = false;
            btnRename.IsEnabled = false;
            txtArtistsInvolved.Text = "• Empty Album";
            ArtistShows.Clear();
            txtNoArtists.Visibility = Visibility.Visible;
            grdViewArtists.Visibility = Visibility.Collapsed;
        }
        else
        {
            txtNoSongs.Visibility = Visibility.Collapsed;
            lstViewMain.Visibility = Visibility.Visible;
            btnPlayAll.IsEnabled = true;
            btnShuffle.IsEnabled = true;
            btnRename.IsEnabled = true;
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

    private async Task SearchFiles()
    {

        string targetAlbum = txtAlbumName.Text;
        FoundSongs.Clear();
        uniqueArtists.Clear();

        ttProgress.IsOpen = true;
        prgProgress.IsIndeterminate = true; // Slide back and forth while we gather files
        prgProgress.Value = 0;

        string[] searchPaths = {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos
    };

        List<StorageFile> allFoundFiles = new List<StorageFile>();

        foreach (var path in searchPaths)
        {
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
            prgProgress.IsIndeterminate = false;
            prgProgress.Maximum = allFoundFiles.Count;

            int processedCount = 0;

            foreach (var file in allFoundFiles)
            {
                var tagFile = TagLib.File.Create(file.Path);
                var tag = tagFile.Tag;

                var props = await file.Properties.GetMusicPropertiesAsync();
                //     ts += props.Duration;
                string artistName = !string.IsNullOrWhiteSpace(props.AlbumArtist)
                    ? props.AlbumArtist : props.Artist;
                string title = !string.IsNullOrWhiteSpace(props.Title) ? props.Title : Path.GetFileNameWithoutExtension(file.Path);

                if (!string.IsNullOrEmpty(artistName))
                {
                    // Split by comma
                    var parts = artistName.Split(',');

                    foreach (var part in parts)
                    {
                        var cleanedArtist = part.Trim();
                        if (!string.IsNullOrWhiteSpace(cleanedArtist))
                        {
                            uniqueArtists.Add(cleanedArtist);
                        }
                    }
                }
                var artistna = props.Artist;

                if (artistna == "")
                {
                    artistna = "Unknown Artist";
                }
                FoundSongs.Add(new SongModel
                {
                    Title = title,
                    Artist = artistna,
                    AlbumName = props.Album,
                    SongDuration = props.Duration,
                    FilePath = file.Path
                });
                string albumName = string.IsNullOrWhiteSpace(tag.Album)
                     ? "Unknown Album"
                     : tag.Album;
                string artists = (tag.AlbumArtists != null && tag.AlbumArtists.Length > 0)
? string.Join(", ", tag.AlbumArtists)
: "Unknown Artist";

                processedCount++;
                prgProgress.Value = processedCount;
            }
        }
        lstViewMain.ItemsSource = FoundSongs;

        var sortedArtists = uniqueArtists.OrderBy(a => a);
        txtArtistsInvolved.Text = "• " + string.Join(", ", sortedArtists);
        if (txtArtistsInvolved.Text == "• ")
        {
            txtArtistsInvolved.Text = "• Empty Album";
        }
        ArtistShows.Clear();
        foreach (var artist in uniqueArtists)
        {
            Debug.WriteLine(artist + " arti");

            Uri fallbackUri = new Uri("ms-appx:///Assets/defaultartist.png");

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var existingAlbum = currentSettings.ArtistsList?
                .FirstOrDefault(a => a.Name == artist);
            string thumbnail = "ms-appx:///Assets/defaultartist.png";

            if (existingAlbum != null && existingAlbum.Thumbnail is string str)
            {
                thumbnail = str;
            }


            var ArtistSe = new ArtistShow { ArtistName = artist, ArtistThumbnailImage = new BitmapImage(new Uri(thumbnail)), ArtistThumbnail = thumbnail };
            ArtistShows.Add(ArtistSe);
        }


        grdViewArtists.ItemsSource = ArtistShows;
        if (ArtistShows.Count == 0)
        {
            grdViewArtists.Visibility = Visibility.Collapsed;
            txtNoArtists.Visibility = Visibility.Visible;
            txtArtistsHeader.Visibility = Visibility.Collapsed;
        }
        else
        {
            txtNoArtists.Visibility = Visibility.Collapsed;
            grdViewArtists.Visibility = Visibility.Visible;
            txtArtistsHeader.Visibility = Visibility.Visible;

        }
        await Task.Delay(500);
        prgActiveProgress.Visibility = Visibility.Collapsed;
        FoundSongs.CollectionChanged -= FoundSongs_CollectionChanged;
        FoundSongs.CollectionChanged += FoundSongs_CollectionChanged;

        ttProgress.IsOpen = false;
    }
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        base.OnNavigatedFrom(e);
    }
    private AlbumContext? _activeAlbum;
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var myApp = (App)Application.Current;
        if (myApp.SelectedAlbum != null)
        {
            _activeAlbum = myApp.SelectedAlbum;
        }
        if (e.Parameter is string AlbumName)
        {

            if (myApp.SelectedAlbum == null)
            {
                myApp.SelectedAlbum = new AlbumContext { Name = AlbumName };
            }
            else
            {
                myApp.SelectedAlbum.Name = AlbumName;
            }
        }
        if (_activeAlbum == null) return;
        LoadAlbumCoverSaved();
        txtAlbumName.Text = _activeAlbum.Name ?? string.Empty;
        LoadFiles();
        base.OnNavigatedTo(e);
    }
    private async void LoadFiles()
    {
        FoundSongs.Clear();
        try
        {
            string target = txtAlbumName.Text;
            var rawSongs = FilesInDatabase.rawSongs;
            var songModels = rawSongs.Where(s => s.AlbumName.Contains(target)).Select(s => new SongModel
            {
                Title = s.Title,

                Artist = s.Artist,

                AlbumName = s.AlbumName,

                FilePath = s.FilePath,

                SongDuration = s.SongDuration

            }).ToList();
            FoundSongs = new ObservableCollection<SongModel>(songModels);
            lstViewMain.ItemsSource = FoundSongs;

            txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";

            TotalDuration();
            LoadArtists();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("ERROR: " + ex.Message);
        }
    }
    private void TotalDuration()
    {
        TimeSpan total = TimeSpan.FromTicks(
            FoundSongs.Sum(s => s.SongDuration?.Ticks ?? 0)
        );

        txtTotalDuration.Text = total.TotalHours >= 1
            ? $"{(int)total.TotalHours}:{total.Minutes:D2}:{total.Seconds:D2}"
            : total.ToString(@"m\:ss");
    }
    private async void LoadArtists()
    {
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        if (FoundSongs.Count != 0)
        {
            grdViewArtists.ItemsSource = ArtistShows;
            var groupedArtists = FoundSongs.Where(song => song.Artist != null && !string.IsNullOrEmpty(song.Artist)).GroupBy(song => song.Artist).ToList();
            foreach (var group in groupedArtists)
            {
                var existingArtist = currentSettings.ArtistsList?
                    .FirstOrDefault(a => a.Name == group.Key);
                string thumbnail = "ms-appx:///Assets/defaultartist.png";

                if (existingArtist != null && existingArtist.Thumbnail is string str)
                {
                    thumbnail = str;
                }
                var artistShow = new ArtistShow { ArtistName = group.Key, Songs = group.ToList(), ArtistThumbnail = thumbnail, ArtistThumbnailImage = new BitmapImage(new Uri(thumbnail)) };
                ArtistShows.Add(artistShow);
            }
           
        }
    }
    private async void LoadAlbumCoverSaved()
    {
        if (_activeAlbum != null)
        {
            Uri fallbackUri = new Uri("ms-appx:///Assets/defaultalbum.png");
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var existing = currentSettings.AlbumsList?
                .FirstOrDefault(a => a.Name == _activeAlbum.Name);
            if (existing != null && !string.IsNullOrEmpty(existing.Thumbnail))
            {
                try
                {
                    imgAlbumCover.Source = new BitmapImage(new Uri(existing.Thumbnail));
                }
                catch (Exception ex)
                {
                    Logger.Log($"Failed to load thumbnail, reverting to default: {ex.Message}", "AlbumPage", Logger.LogLevelType.Error);
                    imgAlbumCover.Source = new BitmapImage(fallbackUri);
                }
            }
            else
            {

                imgAlbumCover.Source = new BitmapImage(fallbackUri);
            }
        }
    }
    private void imgAlbumCover_Tapped(object sender, TappedRoutedEventArgs e)
    {

    }

    private void txtRename_GotFocus(object sender, RoutedEventArgs e)
    {
        txtRename.SelectAll();
    }

    private void btnRename_Click(object sender, RoutedEventArgs e)
    {
        txtRename.Text = txtAlbumName.Text;
    }

    private async void btnRenameAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (_isRenaming) return; // Prevent double clicks
        _isRenaming = true;

        // Disable the button or show a loading state if needed
        btnRename.IsEnabled = false;

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
                            file.Tag.Album = newArtistName;
                            file.Save();
                            file.Dispose();
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
                                            file.Tag.Album = newArtistName;
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
                                            Logger.Log(ex.Message, "AlbumPage.Rename", Logger.LogLevelType.Error);
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
                    Logger.Log(ex.Message, "AlbumPage.RenameAlbum", Logger.LogLevelType.Error);
                }
            }


            // Trigger ONE search/refresh after everything is done
        }
        finally
        {
            _isRenaming = false;
            btnRename.IsEnabled = true;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var albumslist = currentSettings.AlbumsList;
            var exist = albumslist.FirstOrDefault(p => p.Name == txtAlbumName.Text);

            _activeAlbum!.Name = txtAlbumName.Text;

            // 2. Update the "Parking Spot" in App.xaml.cs just to be safe


            if (exist != null)
            {
                exist.Name = txtRename.Text;
            }

            else
            {
                albumslist.Add(new AlbumModel { Name = txtRename.Text });
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            await Task.Delay(1000);
            txtAlbumName.Text = txtRename.Text;
            ((App)Application.Current).SelectedAlbum!.Name = txtAlbumName.Text;
            await SearchFiles();

            flyoutRename.Hide();
        }
    }
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

    private async void btnAddSongs_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance == null) return;
        var files = await FilePickers.MediaPicker.PickMultipleAudioFilesAsync(App.MainWindowInstance, "Choose files");

        if (files == null) return;
        prgActiveProgress.Visibility = Visibility.Visible;

        ObservableCollection<string> existingPaths = new();
        foreach (var file in files)
        {
            var alreadyexist = FoundSongs.FirstOrDefault(s => s.FilePath == file.Path);
            if (alreadyexist == null)
            {
                var tagFile = TagLib.File.Create(file.Path);
                tagFile.Tag.Album = txtAlbumName.Text;
                tagFile.Save();
            }
        }
        await Task.Delay(1500);
        await SearchFiles();
    }

    private async void btnSetAlbumCover_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance == null) return;
        var file = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.MainWindowInstance, "Choose Profile Picture for Artist");

        if (file != null)
        {
            imgAlbumCover.Source = new BitmapImage(new Uri(file.Path));
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var albums = currentSettings.AlbumsList;
            var existingAlbum = albums.FirstOrDefault(a => a.Name == txtAlbumName.Text);

            if (existingAlbum != null)
            {
                existingAlbum.Thumbnail = file.Path;
            }
            else
            {
                var newAlbum = new AlbumModel
                {
                    Name = txtAlbumName.Text,
                    Thumbnail = file.Path
                };
                albums.Add(newAlbum);
            }
            mnftRemoveImage.Visibility = Visibility.Visible;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
    }

    private async void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        await SearchFiles();
    }

    private void btnFindAlbumCoverOnline_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance == null) return;
        CheckInternet.SetImage -= CheckInternet_SetImage;
        CheckInternet.SetImage += CheckInternet_SetImage;
        OceanContentDialog.Show("Find Album Cover Online", "Set", "", "Cancel", OceanDialogWindow.ContentType.OnlineArtistPicture, OceanContentDialogDefault.Primary, XamlRoot, 800, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "", "", txtAlbumName.Text);

        OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
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
                imgAlbumCover.Source = bitmap;
            }
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var albums = currentSettings.AlbumsList;
            var existingAlbum = albums.FirstOrDefault(a => a.Name == txtAlbumName.Text);

            if (existingAlbum != null)
            {
                existingAlbum.Thumbnail = file.Path;
            }
            else
            {
                var newAlbum = new AlbumModel
                {
                    Name = txtAlbumName.Text,
                    Thumbnail = file.Path
                    // Add other default properties here
                };
                albums.Add(newAlbum);
            }
            // Don't forget to save the changes back to storage!
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message, "ArtistImageLoad", Logger.LogLevelType.Error);
        }
    }

    private void ppArtist_Tapped(object sender, TappedRoutedEventArgs e)
    {

    }

    private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is HyperlinkButton hyp && hyp.DataContext is ArtistShow artist && artist.ArtistName is string artistname)
        {
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(ArtistView), artistname);
            }
        }
    }

    private async void mnftRemoveImage_Click(object sender, RoutedEventArgs e)
    {
        imgAlbumCover.Source = new BitmapImage(new Uri("ms-appx:///Assets/defaultalbum.png"));
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var list = currentSettings.AlbumsList;
        var existing = list.FirstOrDefault(a => a.Name == txtAlbumName.Text);

        if (existing != null)
        {
            // 1. Update the existing entry
            existing.Thumbnail = "";
        }
        else
        {
            // 2. Create a new entry if it doesn't exist
            var newAlbum = new AlbumModel
            {
                Name = txtAlbumName.Text,
                Thumbnail = ""
                // Add other default properties here
            };
            list.Add(newAlbum);
        }

        // Don't forget to save the changes back to storage!
        await SettingsLoader.SaveSettingsAsync(currentSettings);
    }
}

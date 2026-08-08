using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;

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

        FileSystemWatch.FileModified -= FileSystemWatch_FileModified;
        FileSystemWatch.FileModified += FileSystemWatch_FileModified;
    }
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _debouncers = new();

    private void FileSystemWatch_FileModified(string filePath, string arg2, string arg3, string arg4, TimeSpan duration, string genre)
    {
        // If an update is already pending for this file, cancel it and restart the timer
        if (_debouncers.TryRemove(filePath, out var existingCts))
        {
            existingCts.Cancel();
            existingCts.Dispose();
        }

        var cts = new CancellationTokenSource();
        _debouncers[filePath] = cts;

        Task.Run(async () =>
        {
            try
            {
                // Wait 300ms for Windows Explorer to finish writing and close the file
                await Task.Delay(300, cts.Token);

                if (cts.Token.IsCancellationRequested) return;

                // Retry reading tags in case File Explorer still holds a brief lock
                for (int attempt = 0; attempt < 5; attempt++)
                {
                    try
                    {
                        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        var abstraction = new SimpleStreamAbstraction(filePath, stream);
                        using var tagFile = TagLib.File.Create(abstraction);

                        string title = string.IsNullOrWhiteSpace(tagFile.Tag.Title)
                            ? Path.GetFileNameWithoutExtension(filePath)
                            : tagFile.Tag.Title;
                        var tag = tagFile.Tag;
                        string artist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
                              ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
                              : string.Join(", ", tag.AlbumArtists);

                        string album = string.IsNullOrWhiteSpace(tagFile.Tag.Album)
                            ? "Unknown Album"
                            : tagFile.Tag.Album;

                        TimeSpan duration = tagFile.Properties.Duration;

                        // Update SQLite Database
                        await DatabaseService.UpdateSongMetadataAsync(filePath, title, artist, album);

                        // Safely dispatch to UI Thread once
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            UpdateSongInCollections(filePath, album, artist, title, duration);
                        });

                        break; // Success! Exit retry loop
                    }
                    catch (Exception ex) when (ex is IOException || ex is COMException)
                    {
                        // File is still locked by Explorer; wait 200ms and try again
                        await Task.Delay(200, cts.Token);
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // Normal - superseded by a newer event for the same file
            }
            finally
            {
                _debouncers.TryRemove(filePath, out _);
                cts.Dispose();
            }
        });
    }
    private async void UpdateSongInCollections(string filePath, string newAlbum, string newArtist, string newTitle, TimeSpan duration)
    {
        string currentFilter = txtAlbumName?.Text?.Trim() ?? "";
        var collections = new ObservableCollection<SongModel>[] { FoundSongs };
        bool foundInAny = false;

        bool matchesAlbum = string.IsNullOrEmpty(currentFilter) ||
                             newAlbum.Contains(currentFilter, StringComparison.OrdinalIgnoreCase);

        foreach (var collection in collections)
        {
            if (collection == null) continue;

            var song = collection.FirstOrDefault(s => s.FilePath == filePath);
            if (song != null)
            {
                foundInAny = true;
                if (!matchesAlbum)
                {
                    collection.Remove(song);
                    Debug.WriteLine($"[Watcher] Removed (Album changed/unmatched): {filePath}");
                }
                else
                {
                    song.AlbumName = newAlbum;
                    song.Artist = newArtist;
                    song.Title = newTitle;
                    song.SongDuration = duration;
                    Debug.WriteLine($"[Watcher] Updated: {filePath}");
                }
            }
        }

        if (!foundInAny && matchesAlbum)
        {
            FoundSongs?.Add(new SongModel
            {
                FilePath = filePath,
                AlbumName = newAlbum,
                Artist = newArtist,
                Title = newTitle,
                SongDuration = duration
            });
            Debug.WriteLine($"[Watcher] Added new track: {filePath}");
        }
        if (FoundSongs != null)
        {
            txtSongCount.Text = $"• {FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";
            TotalDuration();
        }
        // DEBOUNCE LoadArtists: Wait until ALL batch file watcher events finish before rebuilding artists view
        _artistReloadCts?.Cancel();
        _artistReloadCts = new CancellationTokenSource();
        var token = _artistReloadCts.Token;

        try
        {
            await Task.Delay(500, token); // Wait 500ms after last watcher event
            if (!token.IsCancellationRequested)
            {
                LoadArtists();
            }
        }
        catch (TaskCanceledException)
        {
            // Normal: a new file watcher event reset the timer
        }
    }

    private CancellationTokenSource? _artistReloadCts;

    private async void FoundSongs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //    if (FoundSongs.Count == 0)
        //    {
        //        txtNoSongs.Visibility = Visibility.Visible;
        //        lstViewMain.Visibility = Visibility.Collapsed;
        //        btnPlayAll.IsEnabled = false;
        //        btnShuffle.IsEnabled = false;
        //        btnRename.IsEnabled = false;
        //        txtArtistsInvolved.Text = "• Empty Album";
        //        ArtistShows.Clear();
        //        txtNoArtists.Visibility = Visibility.Visible;
        //        grdViewArtists.Visibility = Visibility.Collapsed;
        //    }
        //    else
        //    {
        //        txtNoSongs.Visibility = Visibility.Collapsed;
        //        lstViewMain.Visibility = Visibility.Visible;
        //        btnPlayAll.IsEnabled = true;
        //        btnShuffle.IsEnabled = true;
        //        btnRename.IsEnabled = true;
        //    }
        //    ts = TimeSpan.Zero;
        //    foreach (var item in FoundSongs.ToList())
        //    {
        //        var Storagefile = await StorageFile.GetFileFromPathAsync(item.FilePath);
        //        var props = await Storagefile.Properties.GetMusicPropertiesAsync();
        //        ts += props.Duration;
        //    }
        //    string formatted = ts.TotalHours >= 1
        //? ts.ToString(@"h\:mm\:ss")
        //: ts.ToString(@"m\:ss");
        //    txtTotalDuration.Text = "• " + formatted;
        //    txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "item" : "items")}";

        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
          e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move
          )
        {

            txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";
            TotalDuration();
        }
        txtNoSongs.Visibility = FoundSongs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        lstViewMain.Visibility = FoundSongs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
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
        ArtistShows.Clear();
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
        if (_isRenaming) return; // Prevent concurrent re-entry
        _isRenaming = true;
        btnRenameAlbum.IsEnabled = false;
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var albums = currentSettings.AlbumsList;
        var existalbum = albums.FirstOrDefault(p => p.Name == txtAlbumName.Text);
        if (existalbum == null) return;
        string newAlbumName = txtRename.Text?.Trim() ?? "";

        if (string.IsNullOrEmpty(newAlbumName))
        {
            _isRenaming = false;
            btnRenameAlbum.IsEnabled = true;
            return;
        }

        var filePathsToProcess = FoundSongs
        .Where(s => !string.IsNullOrEmpty(s.FilePath))
        .Select(s => s.FilePath)
        .ToList();
        try
        {
            List<string> filepathstemp = new List<string>();
            await Task.Run(() =>
            {
                foreach (var filePath in filePathsToProcess)
                {
                    // This triggers FileSystemWatcher automatically when saved!
                    AudioMetadata.ChangeAlbumName(filePath, newAlbumName);
                }
            });

            if (existalbum != null)
            {
                existalbum.Name = newAlbumName;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }



            txtAlbumName.Text = newAlbumName;
            flyoutRename.Hide();
        }
        catch (Exception ex)
        {
            Logger.Log(ex.Message, "ArtistPage.RenameArtist", Logger.LogLevelType.Error);
        }
        finally
        {
            // 7. Always restore state and resume watchers
            _isRenaming = false;
            btnRename.IsEnabled = true;
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
        //if (App.MainWindowInstance == null) return;
        //var files = await FilePickers.MediaPicker.PickMultipleAudioFilesAsync(App.MainWindowInstance, "Choose files");

        //if (files == null) return;
        //prgActiveProgress.Visibility = Visibility.Visible;

        //ObservableCollection<string> existingPaths = new();
        //foreach (var file in files)
        //{
        //    var alreadyexist = FoundSongs.FirstOrDefault(s => s.FilePath == file.Path);
        //    if (alreadyexist == null)
        //    {
        //        var tagFile = TagLib.File.Create(file.Path);
        //        tagFile.Tag.Album = txtAlbumName.Text;
        //        tagFile.Save();
        //    }
        //}
        //await Task.Delay(1500);
        //await SearchFiles();

        if (App.MainWindowInstance == null) return;

        var files = await FilePickers.MediaPicker.PickMultipleAudioFilesAsync(App.MainWindowInstance, "Choose files");
        if (files == null || !files.Any()) return;

        string targetAlbum = txtAlbumName?.Text?.Trim() ?? "";

        foreach (var song in files)
        {
            var filepath = song.Path;
            var exist = FoundSongs.FirstOrDefault(p => p.FilePath == filepath);
            if (exist == null)
            {
                if (AudioMetadata.ChangeAlbumName(song.Path, targetAlbum) == false)
                {
                    Debug.WriteLine($"ERROR OCCURED IN ADDING {filepath} TO ALBUM: " + targetAlbum);
                }
            }

        }
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
        LoadFiles();
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

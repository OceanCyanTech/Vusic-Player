using CommunityToolkit.WinUI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.UI.UserViews.Grids;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;


namespace Vusic_Player.Pages.Views;

public sealed partial class ArtistView : Page
{
    public ArtistView()
    {
        InitializeComponent();

        FileSystemWatch.FileModified -= FileSystemWatch_FileModified;
        FileSystemWatch.FileModified += FileSystemWatch_FileModified;
        albumCollection.CollectionChanged -= AlbumCollection_CollectionChanged;
        albumCollection.CollectionChanged += AlbumCollection_CollectionChanged;
    }

    private void AlbumCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        txtEmptyAlbums.Visibility = albumCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        grdViewAlbums.Visibility = albumCollection.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        txtAlbumCount.Text = "• " + $"{albumCollection.Count} {(albumCollection.Count == 1 ? "Album" : "Albums")}";

    }

    private readonly ConcurrentDictionary<string, CancellationTokenSource> _debouncers = new();

    private void FileSystemWatch_FileModified(string filePath, string arg2, string arg3, string arg4, TimeSpan duration)
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
    private CancellationTokenSource? _albumReloadCts;

    private async void UpdateSongInCollections(string filePath, string newAlbum, string newArtist, string newTitle, TimeSpan duration)
    {
        string currentFilter = txtArtistName?.Text?.Trim() ?? "";
        var collections = new ObservableCollection<SongModel>[] { FoundSongs, mostplayedsongs, Singles };
        bool foundInAny = false;

        bool matchesArtist = string.IsNullOrEmpty(currentFilter) ||
                             newArtist.Contains(currentFilter, StringComparison.OrdinalIgnoreCase);

        foreach (var collection in collections)
        {
            if (collection == null) continue;

            var song = collection.FirstOrDefault(s => s.FilePath == filePath);
            if (song != null)
            {
                foundInAny = true;
                if (!matchesArtist)
                {
                    collection.Remove(song);
                    Debug.WriteLine($"[Watcher] Removed (Artist changed/unmatched): {filePath}");
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

        if (!foundInAny && matchesArtist && selBarMain?.SelectedItem == selBarItemAllSongs)
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
        // DEBOUNCE LoadAlbums: Wait until ALL batch file watcher events finish before rebuilding albums view
        _albumReloadCts?.Cancel();
        _albumReloadCts = new CancellationTokenSource();
        var token = _albumReloadCts.Token;

        try
        {
            await Task.Delay(500, token); // Wait 500ms after last watcher event
            if (!token.IsCancellationRequested)
            {
                LoadAlbums();
            }
        }
        catch (TaskCanceledException)
        {
            // Normal: a new file watcher event reset the timer
        }
    }
    private async void FoundSongs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
            e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
            e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Move
            )
        {

            txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";
            TotalDuration();
        }
        txtEmptySongs.Visibility = FoundSongs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        lstViewAllSongs.Visibility = FoundSongs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }
    TimeSpan ts;
    public ObservableCollection<SongModel> FoundSongs { get; set; } = new ObservableCollection<SongModel>();
    ObservableCollection<ArtistDiscAlbumModel> albumCollection = new ObservableCollection<ArtistDiscAlbumModel>();
    public ObservableCollection<SongModel> Singles { get; set; } = new ObservableCollection<SongModel>();
    HashSet<string> uniqueArtists = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    ObservableCollection<string> AlbumsList { get; set; } = new();
    private void LoadSingles()
    {
        Singles.Clear();
        var singlesongs = FoundSongs.Where(p => p.AlbumName == "Unknown Album");
        Singles = new ObservableCollection<SongModel>(singlesongs);

        txtEmptySingles.Visibility = Singles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        lstViewSingles.Visibility = Singles.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        lstViewSingles.ItemsSource = Singles;
        txtSinglesCount.Text = $"• {Singles.Count} {(Singles.Count == 1 ? "item" : "items")}";
        TimeSpan? timeSpan = TimeSpan.Zero;
        foreach (var item in Singles.ToList())
        {

            timeSpan += item.SongDuration;
        }
        string formatted = timeSpan is TimeSpan ts
          ? (ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss"))
          : "0:00";
        txtSinglesDuration.Text = "• " + formatted;
    }

    private bool _isRenaming = false;

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
    private async void LoadAlbums()
    {
        AlbumsList.Clear();
        albumCollection.Clear();
        var groupedAlbums = FoundSongs
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
        foreach (var albumGroup in groupedAlbums)
        {
            var songs = albumGroup.Songs;
            int countsongs = songs.Count;
            string countsong = $"{countsongs} {(countsongs == 1 ? "item" : "items")}";

            BitmapImage img = await LoadExistingThumbnailAsync(albumGroup.AlbumName ?? "Unknown Album");

            albumCollection.Add(new ArtistDiscAlbumModel
            {
                AlbumName = albumGroup.AlbumName ?? "Unknown Album",
                AlbumCount = countsong,
                AlbumCoverThumbnail = img,
                AlbumYear = albumGroup.CalculatedYear.ToString(),
                Songs = songs
            });
        }
        grdViewAlbums.ItemsSource = albumCollection;

        txtEmptyAlbums.Visibility = albumCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        grdViewAlbums.Visibility = albumCollection.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        txtAlbumCount.Text = "• " + $"{albumCollection.Count} {(albumCollection.Count == 1 ? "Album" : "Albums")}";


    }
    private async void LoadSelectorBarSelectionSettings()
    {
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var selectorBarindex = currentSettings.ArtistView_selectorbarindex;
        if (selectorBarindex == 0)
        {
            selBarMain.SelectedItem = selBarItemMostPlayed;
            LoadMostPlayed();
        }
        else if (selectorBarindex == 1)
        {
            selBarMain.SelectedItem = selBarItemAlbum;
            LoadAlbums();
        }
        else if (selectorBarindex == 2)
        {
            selBarMain.SelectedItem = selBarItemSingles;
            LoadSingles();
        }
        else if (selectorBarindex == 3)
        {
            selBarMain.SelectedItem = selBarItemAllSongs;
        }
    }
    private async void SaveSelectorBarSelection()
    {
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        int selectbarindex = 0;
        if (selBarMain.SelectedItem == selBarItemMostPlayed)
        {
            selectbarindex = 0;
        }
        else if (selBarMain.SelectedItem == selBarItemAlbum)
        {
            selectbarindex = 1;
        }
        else if (selBarMain.SelectedItem == selBarItemSingles)
        {
            selectbarindex = 2;
        }
        else if (selBarMain.SelectedItem == selBarItemAllSongs)
        {
            selectbarindex = 3;
        }
        currentSettings.ArtistView_selectorbarindex = selectbarindex;
        await SettingsLoader.SaveSettingsAsync(currentSettings);

    }
    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string selectedSongArtist)
        {
            txtArtistName.Text = selectedSongArtist;

            imgArtist.DisplayName = txtArtistName.Text;

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
            FoundSongs.Clear();
            //lstViewAllSongs.ItemsSource = FoundSongs;

            try

            {
                string targetArtist = txtArtistName.Text;
                var rawSongs = FilesInDatabase.rawSongs;
                var songModels = rawSongs.Where(s => s.Artist.Contains(targetArtist)).Select(s => new SongModel

                {

                    Title = s.Title,

                    Artist = s.Artist,

                    AlbumName = s.AlbumName,

                    FilePath = s.FilePath,

                    SongDuration = s.SongDuration

                }).ToList();

                // 3. Assign directly to UI

                FoundSongs = new ObservableCollection<SongModel>(songModels);

                lstViewAllSongs.ItemsSource = FoundSongs;



                txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";

                TotalDuration();
                LoadSelectorBarSelectionSettings();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load songs: {ex.Message}");
            }
        }
        FilesInDatabase.SongsDiscovered -= FilesInDatabase_SongsDiscovered;
        FilesInDatabase.SongsDiscovered += FilesInDatabase_SongsDiscovered;
        LoadAlbums();
        FoundSongs.CollectionChanged -= FoundSongs_CollectionChanged;
        FoundSongs.CollectionChanged += FoundSongs_CollectionChanged;
        base.OnNavigatedTo(e);
    }

    private void FilesInDatabase_SongsDiscovered(IEnumerable<SongModel> obj)
    {
        foreach (var song in obj.ToList())
        {
            FoundSongs.Add(song);
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



    private async void LoadMostPlayed()
    {
        mostplayedsongs.Clear();
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var sortedSongs = currentSettings.RecentMusic.OrderByDescending(x => x.PlayCount).Take(5).ToList();
        foreach (var song in sortedSongs)
        {
            var existingsong = FoundSongs.FirstOrDefault(p => p.FilePath == song.SongPath);
            if (existingsong != null)
            {
                mostplayedsongs.Add(existingsong);
            }
        }
        lstViewMasterMostPlayed.ItemsSource = mostplayedsongs;
        lstViewMasterMostPlayed.Visibility = mostplayedsongs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        txtEmptyMostPlayed.Visibility = mostplayedsongs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        txtMostPlayedCount.Text = $"• {mostplayedsongs.Count} {(mostplayedsongs.Count == 1 ? "item" : "items")}";
        TimeSpan? timeSpan = TimeSpan.Zero;
        foreach (var item in mostplayedsongs.ToList())
        {

            timeSpan += item.SongDuration;
        }
        string formatted = +timeSpan is TimeSpan ts
          ? (ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss"))
          : "0:00";
        txtMostPlayedDuration.Text = "• " + formatted;
    }
    ObservableCollection<SongModel> mostplayedsongs = new();
    private async Task LoadMostPlayedSongsBackground(SettingsValues currentSettings, string targetArtist, HashSet<string> favSet, string currentPlayingPath, bool isPlaying)
    {
        if (currentSettings?.RecentMusic == null) return;
        mostplayedsongs.Clear();

        var sortedSongs = currentSettings.RecentMusic.OrderByDescending(x => x.PlayCount).Take(5).ToList();

        // Parse TagLib attributes for recent play matches purely on background worker tasks
        var rawMatches = await Task.Run(() =>
        {
            var list = new List<AudioTrackLite>();
            foreach (var item in sortedSongs)
            {
                try
                {
                    if (!System.IO.File.Exists(item.SongPath)) continue;

                    using var filetag = TagLib.File.Create(item.SongPath);
                    var artists = filetag.Tag.AlbumArtists;

                    if (artists != null && artists.Any(a => a.Contains(targetArtist, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(new AudioTrackLite
                        {
                            Title = !string.IsNullOrWhiteSpace(filetag.Tag.Title) ? filetag.Tag.Title : Path.GetFileNameWithoutExtension(item.SongPath),
                            AlbumName = !string.IsNullOrWhiteSpace(filetag.Tag.Album) ? filetag.Tag.Album : "Unknown Album",
                            Artist = string.Join(", ", artists),
                            FilePath = item.SongPath,
                            SongDuration = filetag.Properties.Duration,
                            IsFavourite = favSet.Contains(Path.GetFullPath(item.SongPath))
                        });
                    }
                }
                catch { /* Gracefully digest single metadata reading errors */ }
            }
            return list;
        });

        // Populate elements natively inside the primary application frame thread context
        foreach (var item in rawMatches)
        {
            var colorbrush = new SolidColorBrush(Microsoft.UI.Colors.White);
            var glyph = "\uEC4F";

            if (currentPlayingPath == item.FilePath)
            {
                colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
                glyph = isPlaying ? "\uE769" : "\uE768";
            }

            mostplayedsongs.Add(new SongModel
            {
                Title = item.Title,
                AlbumName = item.AlbumName,
                Artist = item.Artist,
                FilePath = item.FilePath,
                FavOpacity = item.IsFavourite ? 1.0 : 0.0,
                FavString = item.IsFavourite ? "Remove from Favourites" : "Add to Favourites",
                SongDuration = item.SongDuration,
                IsFavourite = item.IsFavourite,
                Glyph = glyph,
                TitleColor = colorbrush,
                Remove = "Remove from History",
                MediaType = "ArtistMP",
                IsMovableItem = Visibility.Collapsed
            });
        }

        lstViewMasterMostPlayed.ItemsSource = mostplayedsongs;
        lstViewMasterMostPlayed.Visibility = mostplayedsongs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        txtEmptyMostPlayed.Visibility = mostplayedsongs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
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
        if (_isRenaming) return; // Prevent concurrent re-entry
        _isRenaming = true;
        btnRenameArtist.IsEnabled = false;
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var artistss = currentSettings.ArtistsList;
        var existartist = artistss.FirstOrDefault(p => p.Name == txtArtistName.Text);
        string newArtistName = txtRename.Text?.Trim() ?? "";
        var existartistinlist = artistss.FirstOrDefault(p => p.Name == newArtistName);
        if (existartistinlist != null)
        {

        }
        if (string.IsNullOrEmpty(newArtistName))
        {
            _isRenaming = false;
            btnRenameArtist.IsEnabled = true;
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
                    AudioMetadata.ChangeArtistName(filePath, newArtistName);
                }
            });

            if (existartist != null)
            {
                existartist.Name = newArtistName;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }



            txtArtistName.Text = newArtistName;
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
            btnRenameArtist.IsEnabled = true;
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
        if (files == null || !files.Any()) return;

        string targetArtist = txtArtistName?.Text?.Trim() ?? "";

        // 1. Pause watchers during batch file & DB operations
        FileSystemWatch.Pause();

        try
        {
            await Task.Run(async () =>
            {
                foreach (var file in files)
                {
                    string path = file.Path;

                    // Check if already in current memory collection
                    bool existsInMemory = FoundSongs.Any(s => s.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase));

                    // 2. Modify metadata on disk if an artist name is set
                    if (!string.IsNullOrEmpty(targetArtist))
                    {
                        AudioMetadata.ChangeArtistName(path, targetArtist);
                    }

                    // Read updated tags using TagLib for accurate database entry
                    using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    var abstraction = new SimpleStreamAbstraction(path, stream);
                    using var tagFile = TagLib.File.Create(abstraction);

                    string title = string.IsNullOrWhiteSpace(tagFile.Tag.Title)
                        ? Path.GetFileNameWithoutExtension(path)
                        : tagFile.Tag.Title;

                    string artist = string.IsNullOrWhiteSpace(tagFile.Tag.FirstPerformer)
                        ? (string.IsNullOrEmpty(targetArtist) ? "Unknown Artist" : targetArtist)
                        : tagFile.Tag.FirstPerformer;

                    string album = string.IsNullOrWhiteSpace(tagFile.Tag.Album)
                        ? "Unknown Album"
                        : tagFile.Tag.Album;

                    TimeSpan duration = tagFile.Properties.Duration;

                    // 3. Save / Update in SQLite Database
                    await DatabaseService.UpdateSongMetadataAsync(path, title, artist, album);

                    // 4. Update UI collection safely on UI thread
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        var existingSong = FoundSongs.FirstOrDefault(s => s.FilePath.Equals(path, StringComparison.OrdinalIgnoreCase));

                        if (existingSong != null)
                        {
                            // Update existing entry
                            existingSong.Title = title;
                            existingSong.Artist = artist;
                            existingSong.AlbumName = album;
                            existingSong.SongDuration = duration;
                        }
                        else
                        {
                            // Add new entry
                            FoundSongs.Add(new SongModel
                            {
                                FilePath = path,
                                Title = title,
                                Artist = artist,
                                AlbumName = album,
                                SongDuration = duration
                            });
                        }
                    });

                    // Small breather to let Windows Explorer release handles
                    await Task.Delay(30);
                }
            });

            // 5. Recalculate total duration once all files are processed
            TotalDuration();
        }
        finally
        {
            // 6. Always resume watchers
            FileSystemWatch.Resume();
        }
    }
    // --- Discography & Album Events ---
    private void ifbError_CloseButtonClick(InfoBar sender, object args)
    {
        sender.Visibility = Visibility.Collapsed;
    }

    private void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        Refresh();
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
    private async void OceanContentDialog_PrimaryRequested1()
    {
        OceanContentDialog.HideDlg();
        MainWindow.ShowWindow();
        LoadAlbums();
    }



    private async void mnftChangeAlbumCover_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel album)
        {
            if (App.MainWindowInstance != null)
            {
                var image = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.MainWindowInstance, "Change Cover of Album");
                if (image != null)
                {
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var albums = currentSettings.AlbumsList;
                    var existingalbum = albums.FirstOrDefault(p => p.Name == album.AlbumName);
                    if (existingalbum != null)
                    {

                        existingalbum.Thumbnail = image.Path;
                    }
                    else
                    {
                        albums.Add(new AlbumModel { Name = album.AlbumName, Thumbnail = image.Path });
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    album.AlbumCoverThumbnail = new BitmapImage(new Uri(image.Path));
                }
            }
        }
    }

    private async void mnftFindAlbumCoverOnline_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel album)
        {
            if (App.MainWindowInstance == null) return;
            CheckInternet.SetImage -= CheckInternet_SetImage;
            OceanContentDialog.Show("Find Album Cover Online", "Set", "", "Cancel", OceanDialogWindow.ContentType.OnlineArtistPicture, OceanContentDialogDefault.Primary, XamlRoot, 800, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "", "", album.AlbumName);
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;

            CheckInternet.SetImage += async () =>
            {
                var file = await StorageFile.GetFileFromPathAsync(CheckInternet.UrlToDownload);
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    album.Thumbnail = file.Path;
                    album.AlbumCoverThumbnail = bitmap;
                }
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var artists = currentSettings.AlbumsList;
                var existingArtist = artists.FirstOrDefault(a => a.Name == album.AlbumName);

                if (existingArtist != null)
                {
                    existingArtist.Thumbnail = file.Path;
                }
                else
                {
                    var newArtist = new AlbumModel
                    {
                        Name = album.AlbumName,
                        Thumbnail = file.Path
                        // Add other default properties here
                    };
                    artists.Add(newArtist);
                }

                await SettingsLoader.SaveSettingsAsync(currentSettings);
            };


        }
    }

    private async void mnftRemoveAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel albumModel && albumModel.AlbumName is string name)
        {
            FileSystemWatch.Pause();
            try
            {
                var songs = albumModel.Songs;
                foreach (var song in songs)
                {
                    Debug.WriteLine($"RENAME ALBUM: {song.FilePath}");

                    // Write to file metadata asynchronously if possible, or keep synchronous if required
                    if (AudioMetadata.ChangeAlbumName(song.FilePath, ""))
                    {
                        Debug.WriteLine("TRUE, FILE NAME ALBUM CHANGED");

                        await FileSystemWatch.ProcessFileMetadataChangeAsync(song.FilePath);

                    }
                    else
                    {
                        Debug.WriteLine("ERROR, FILE NAME ALBUM NOT CHANGED");

                    }
                }
            }
            finally
            {
                FileSystemWatch.Resume();
                //     LoadAlbums();
                albumCollection.Remove(albumModel);
                var albumunknown = albumCollection.FirstOrDefault(p => p.AlbumName == "Unknown Album");
                if (albumunknown != null)
                {
                    var count = albumunknown.Songs.Count;
                    count += albumModel.Songs.Count;
                    albumunknown.AlbumCount = $"{count} {(count == 1 ? "item" : "items")}";

                }
            }
        }
    }
    private void UpdateUIView()
    {
        if (selBarMain.SelectedItem == selBarItemMostPlayed)
        {
            lstViewMasterMostPlayed.Visibility = mostplayedsongs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            txtEmptyMostPlayed.Visibility = mostplayedsongs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            txtMostPlayedCount.Text = $"• {mostplayedsongs.Count} {(mostplayedsongs.Count == 1 ? "item" : "items")}";
            TimeSpan? timeSpan = TimeSpan.Zero;
            foreach (var item in mostplayedsongs.ToList())
            {

                timeSpan += item.SongDuration;
            }
            string formatted = +timeSpan is TimeSpan ts
              ? (ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss"))
              : "0:00";
            txtMostPlayedDuration.Text = "• " + formatted;
        }

        else if (selBarMain.SelectedItem == selBarItemSingles)
        {
            txtEmptySingles.Visibility = Singles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

            lstViewSingles.Visibility = Singles.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            lstViewSingles.ItemsSource = Singles;
            txtSinglesCount.Text = $"• {Singles.Count} {(Singles.Count == 1 ? "item" : "items")}";
            TimeSpan? timeSpan = TimeSpan.Zero;
            foreach (var item in Singles.ToList())
            {

                timeSpan += item.SongDuration;
            }
            string formatted = timeSpan is TimeSpan ts
              ? (ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss"))
              : "0:00";
            txtSinglesDuration.Text = "• " + formatted;
        }

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

    private void mnftOpenAlbum_Click(object sender, RoutedEventArgs e)
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

    private void mnftRenameAlbum_Click(object sender, RoutedEventArgs e)
    {

        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel albumModel && albumModel.AlbumName is string name)
        {
            txtRenameAlbum.Text = name;
            ttRenameAlbum.IsOpen = true;

            // 1. Safe Event Handling: Remove any previous handler before adding a new one
            // to prevent multiple executions and memory leaks.
            RoutedEventHandler? clickHandler = null;
            clickHandler = (object btnSender, RoutedEventArgs e) =>
            {
                btnRenameAlbum.Click -= clickHandler; // Unsubscribe immediately
                ttRenameAlbum.IsOpen = false;

                var newName = txtRenameAlbum.Text.Trim();
                if (string.IsNullOrEmpty(newName) || newName == name)
                {
                    return;
                }

                // 2. Update the main model
                albumModel.AlbumName = newName;
                var songs = albumModel.Songs;

                foreach (var song in songs)
                {
                    Debug.WriteLine($"RENAME ALBUM: {song.FilePath}");

                    // Write to file metadata asynchronously if possible, or keep synchronous if required
                    AudioMetadata.ChangeAlbumName(song.FilePath, newName);

                    // 3. Optimized Lookup: Target specific lists directly if you know where the song lives,
                    // or use a unified update approach.
                    UpdateSongCollection(FoundSongs, song.FilePath, newName);
                    UpdateSongCollection(Singles, song.FilePath, newName);
                    UpdateSongCollection(mostplayedsongs, song.FilePath, newName);
                }

                // 4. Update the item count string efficiently
                var count = FoundSongs.Count(p => p.AlbumName == newName);
                albumModel.AlbumCount = $"{count} {(count == 1 ? "item" : "items")}";
            };

            // Clear old handlers just in case, then bind the new one
            btnRenameAlbum.Click += clickHandler;
        }
    }

    // Helper method to keep your code DRY (Don't Repeat Yourself)
    private void UpdateSongCollection(IEnumerable<SongModel> collection, string filePath, string newName)
    {
        var song = collection?.FirstOrDefault(p => p.FilePath == filePath);
        if (song != null)
        {
            song.AlbumName = newName;
        }
    }

    private async void mnftAlbumInfo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel albumModel && albumModel.AlbumName is string name)
        {
            ttAlbumInfo.IsOpen = true;
            ttAlbumInfo.Title = "Album Info - " + name;
            txtAlbumName.Text = name;
            ToolTipService.SetToolTip(txtAlbumName, name);
            txtCount.Text = "• " + $"{albumModel.Songs.Count} {(albumModel.Songs.Count == 1 ? "item" : "items")}";
            txtArtistsInvolved.Text = albumModel.AlbumArtists;
            ToolTipService.SetToolTip(txtArtistsInvolved, txtArtistsInvolved.Text);
            imgAlbumCover.Source = albumModel.AlbumCoverThumbnail;
            TimeSpan total = TimeSpan.FromTicks(albumModel.Songs.Sum(s => s.SongDuration?.Ticks ?? 0));

            txtDuration.Text = total.TotalHours >= 1
                ? $"{(int)total.TotalHours}:{total.Minutes:D2}:{total.Seconds:D2}"
                : "• " + total.ToString(@"m\:ss");
            // txtDuration.Text = "• " + txtDuration.Text;
            var uniqueArtists = new HashSet<string>();
            var observablesongs = new ObservableCollection<SongModel>();

            foreach (var song in albumModel.Songs)
            {
                observablesongs.Add(new SongModel { Artist = song.Artist, Glyph = song.Glyph, Title = song.Title, FilePath = song.FilePath });
                if (!string.IsNullOrEmpty(song.Artist))
                    uniqueArtists.Add(song.Artist);
            }
            lstViewAlbumSongs.ItemsSource = observablesongs;
            var sortedArtists = uniqueArtists.OrderBy(a => a);
            txtArtistsInvolved.Text = "• " + string.Join(", ", sortedArtists);
            txtEmptySongsAlbum.Visibility = observablesongs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            lstViewAlbumSongs.Visibility = observablesongs.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void mnftViewImage_Click(object sender, RoutedEventArgs e)
    {
        //PENDING: ENLARGED IMAGE VIEWER
    }


    private void selBarMain_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        grdMostPlayed.Visibility = Visibility.Collapsed;
        grdArtistSingles.Visibility = Visibility.Collapsed;
        grdAlbums.Visibility = Visibility.Collapsed;
        grdAllSongs.Visibility = Visibility.Collapsed;
        if (selBarMain.SelectedItem == selBarItemMostPlayed)
        {
            grdMostPlayed.Visibility = Visibility.Visible;
            LoadMostPlayed();
        }
        else if (selBarMain.SelectedItem == selBarItemAlbum)
        {
            grdAlbums.Visibility = Visibility.Visible;
            LoadAlbums();
        }
        else if (selBarMain.SelectedItem == selBarItemSingles)
        {
            grdArtistSingles.Visibility = Visibility.Visible;
            LoadSingles();
        }
        else if (selBarMain.SelectedItem == selBarItemAllSongs)
        {
            grdAllSongs.Visibility = Visibility.Visible;
        }
        SaveSelectorBarSelection();
    }
    private async void Refresh()
    {
        FoundSongs.Clear();

        try

        {
            string targetArtist = txtArtistName.Text;
            var rawSongs = FilesInDatabase.rawSongs;
            var songModels = rawSongs.Where(s => s.Artist.Contains(targetArtist)).Select(s => new SongModel

            {

                Title = s.Title,

                Artist = s.Artist,

                AlbumName = s.AlbumName,

                FilePath = s.FilePath,

                SongDuration = s.SongDuration

            }).ToList();

            // 3. Assign directly to UI

            FoundSongs = new ObservableCollection<SongModel>(songModels);

            lstViewAllSongs.ItemsSource = FoundSongs;



            txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";

            TotalDuration();
            LoadSelectorBarSelectionSettings();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load songs: {ex.Message}");
        }
        if (selBarMain.SelectedItem == selBarItemMostPlayed)
        {
            LoadMostPlayed();
        }
        else if (selBarMain.SelectedItem == selBarItemAlbum)
        {
            LoadAlbums();
        }
        else if (selBarMain.SelectedItem == selBarItemSingles)
        {
            LoadSingles();
        }

    }
    private void btnPlayAllMostPlayed_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in mostplayedsongs)
        {
            item.IsCompleted = false;
        }
        QueueService.PlayMedia(mostplayedsongs, btnShuffle.IsChecked ?? false, false);
    }

    private void btnPlayAllSingles_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in Singles)
        {
            item.IsCompleted = false;
        }
        QueueService.PlayMedia(Singles, btnShuffle.IsChecked ?? false, false);
    }

    private void HoverOverlay_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimateOpacity(HoverOverlay, 0.7);
        AnimateOpacity(btnRemoveImage, 1);
    }

    private void HoverOverlay_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimateOpacity(HoverOverlay, 0);
        AnimateOpacity(btnRemoveImage, 0);

    }
    private async void btnChangeAlbumCover_Click(object sender, RoutedEventArgs e)
    {
        if (App.OceanDialogInstance == null) return;
        var file = await MediaPicker.PickSingleImageFileAsync(App.OceanDialogInstance, "Choose Album Cover");
        if (file != null)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var artists = currentSettings.AlbumsList;
            var existingArtist = artists.FirstOrDefault(a => a.Name == txtAlbumName.Text);

            if (existingArtist != null)
            {
                existingArtist.Thumbnail = file.Path;
            }
            else
            {
                var newArtist = new AlbumModel
                {
                    Name = txtAlbumName.Text,
                    Thumbnail = file.Path
                };
                artists.Add(newArtist);
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            imgAlbumCover.Source = new BitmapImage(new Uri(file.Path));
        }

    }
    private void btnFindOnline_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance == null) return;
        CheckInternet.SetImage -= CheckInternet_SetImage;
        OceanContentDialog.Show("Find Album Cover Online", "Set", "", "Cancel", OceanDialogWindow.ContentType.OnlineArtistPicture, OceanContentDialogDefault.Primary, XamlRoot, 800, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "", "", txtAlbumName.Text);
        OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
        OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
        var existingalbum = albumCollection.FirstOrDefault(p => p.AlbumName == txtAlbumName.Text);
        CheckInternet.SetImage += async () =>
        {
            var file = await StorageFile.GetFileFromPathAsync(CheckInternet.UrlToDownload);
            using (var stream = await file.OpenAsync(FileAccessMode.Read))
            {
                var bitmap = new BitmapImage();
                await bitmap.SetSourceAsync(stream);
                imgAlbumCover.Source = bitmap;
                if (existingalbum != null)
                {
                    existingalbum.Thumbnail = file.Path;
                    existingalbum.AlbumCoverThumbnail = bitmap;
                }

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

            await SettingsLoader.SaveSettingsAsync(currentSettings);
        };
    }
    private void AnimateOpacity(UIElement target, double toOpacity)
    {
        Storyboard storyboard = new Storyboard();
        DoubleAnimation animation = new DoubleAnimation
        {
            To = toOpacity,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)), // 0.2 seconds
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        Storyboard.SetTarget(animation, target);
        Storyboard.SetTargetProperty(animation, "Opacity");

        storyboard.Children.Add(animation);
        storyboard.Begin();
    }

    private void btnRemoveImage_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        AnimateOpacity(HoverOverlay, 0.7);
        AnimateOpacity(btnRemoveImage, 1);
    }

    private void btnRemoveImage_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        AnimateOpacity(HoverOverlay, 0);
        AnimateOpacity(btnRemoveImage, 0);
    }

    private async void btnRemoveImage_Click(object sender, RoutedEventArgs e)
    {
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var albums = currentSettings.AlbumsList;
        var existingAlbum = albums.FirstOrDefault(a => a.Name == txtAlbumName.Text);

        if (existingAlbum != null)
        {
            albums.Remove(existingAlbum);
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        imgAlbumCover.Source = new BitmapImage(new Uri("ms-appx:///Assets/defaultalbum.png"));
    }

    private void mnftCopyFilePath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song)
        {
            CopyToClipboard.CopyStringToClipboard(song.FilePath);
        }

    }
}

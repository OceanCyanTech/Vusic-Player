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
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.AudioProperties;


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

        // Clear UI Collections immediately on the UI Thread
        FoundSongs.Clear();
        uniqueArtists.Clear();
        albumCollection.Clear();
        Singles.Clear();
        AlbumsList.Clear();
        ts = TimeSpan.Zero;

        // Progress UI Initialization
        ttProgress.IsOpen = true;
        prgProgress.IsIndeterminate = true;
        prgProgress.Value = 0;

        string[] searchPaths =
        {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos,
        UserDataPaths.GetDefault().Pictures
    };

        // 1. Fetch settings snapshots cleanly on UI thread
        var settings = await SettingsLoader.LoadSettingsAsync();
        var favourites = settings.Favourites ?? new ObservableCollection<FavouriteItems>();
        var favSet = new HashSet<string>(favourites.Select(f => Path.GetFullPath(f.FilePath)), StringComparer.OrdinalIgnoreCase);
        string currentPlayingPath = !string.IsNullOrEmpty(PlayerService.CurrentPlayingPath) ? Path.GetFullPath(PlayerService.CurrentPlayingPath) : string.Empty;
        bool isMasterPlayerPlaying = PlayerService.Masterplayer?.IsPlaying ?? false;

        // 2. Scan and parse sequentially on a single background thread to prevent COM exceptions
        var resultData = await Task.Run(async () =>
        {
            var localLiteTracks = new List<AudioTrackLite>();
            var localUniqueArtists = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var localAlbumsList = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allFiles = new List<StorageFile>();

            // Step A: Fast structural gathering using WinRT queries
            foreach (var path in searchPaths)
            {
                try
                {
                    if (!Directory.Exists(path)) continue;

                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                    var queryOptions = new QueryOptions(CommonFileQuery.OrderByMusicProperties, AudioExtensions.List);

                    // CRITICAL: Tells Windows to batch-load the metadata (including the accurate duration) upfront!
                    queryOptions.SetPropertyPrefetch(Windows.Storage.FileProperties.PropertyPrefetchOptions.MusicProperties, null);

                    var query = folder.CreateFileQueryWithOptions(queryOptions);
                    var files = await query.GetFilesAsync();
                    allFiles.AddRange(files);
                }
                catch { /* Ignore access restrictions */ }
            }

            int totalFilesFound = allFiles.Count;
            int processedCount = 0;

            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (totalFilesFound > 0)
            {
                dispatcher?.TryEnqueue(() =>
                {
                    prgProgress.IsIndeterminate = false;
                    prgProgress.Maximum = totalFilesFound;
                });
            }

            // Use a local HashSet to completely block duplicate file paths during parsing
            var duplicateCheckSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Step B: Loop SEQUENTIALLY. Sequential execution on one thread prevents the WinRT COM panic.
            foreach (var file in allFiles)
            {
                processedCount++;
                try
                {
                    string normalizedPath = Path.GetFullPath(file.Path);
                    if (duplicateCheckSet.Contains(normalizedPath)) continue;

                    // Grab the highly accurate Windows Indexer metadata properties
                    var musicprops = await file.Properties.GetMusicPropertiesAsync();

                    // Artist filtering logic
                    bool isMatch = (!string.IsNullOrEmpty(musicprops.Artist) && musicprops.Artist.Contains(targetArtist, StringComparison.OrdinalIgnoreCase)) ||
                                   (!string.IsNullOrEmpty(musicprops.AlbumArtist) && musicprops.AlbumArtist.Contains(targetArtist, StringComparison.OrdinalIgnoreCase));

                    if (!isMatch)
                    {
                        if (processedCount % 25 == 0 || processedCount == totalFilesFound)
                        {
                            dispatcher?.TryEnqueue(() => prgProgress.Value = processedCount);
                        }
                        continue;
                    }

                    duplicateCheckSet.Add(normalizedPath);

                    string artistName = !string.IsNullOrWhiteSpace(musicprops.AlbumArtist) ? musicprops.AlbumArtist : musicprops.Artist;
                    if (!string.IsNullOrEmpty(artistName))
                        localUniqueArtists.Add(artistName);

                    string albumName = string.IsNullOrWhiteSpace(musicprops.Album) ? "Unknown Album" : musicprops.Album;
                    string displayArtist = string.IsNullOrWhiteSpace(musicprops.Artist) ? "Unknown Artist" : musicprops.Artist;

                    localAlbumsList.Add(albumName);

                    // DTO mapping using the correct musicprops.Duration!
                    localLiteTracks.Add(new AudioTrackLite
                    {
                        Title = string.IsNullOrEmpty(musicprops.Title) ? file.DisplayName : musicprops.Title,
                        Artist = displayArtist,
                        AlbumName = albumName,
                        FilePath = normalizedPath,
                        SongDuration = musicprops.Duration, // Perfect accurate duration 🎯
                        IsFavourite = favSet.Contains(normalizedPath)
                    });
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, "ArtistPage.Load.BackgroundWorker", Logger.LogLevelType.Error);
                }

                if (processedCount % 25 == 0 || processedCount == totalFilesFound)
                {
                    dispatcher?.TryEnqueue(() => prgProgress.Value = processedCount);
                }
            }

            return (Tracks: localLiteTracks, Artists: localUniqueArtists.OrderBy(a => a).ToList(), Albums: localAlbumsList.ToList());
        });

        if (resultData.Tracks.Count == 0)
        {
            UpdateUIEmptyStates();
            ttProgress.IsOpen = false;
            return;
        }

        // 3. UI Thread Batch Loop Update
        int batchSize = 35;
        var uiDispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        foreach (var artist in resultData.Artists) uniqueArtists.Add(artist);
        foreach (var album in resultData.Albums) AlbumsList.Add(album);

        for (int i = 0; i < resultData.Tracks.Count; i += batchSize)
        {
            var batch = resultData.Tracks.Skip(i).Take(batchSize).ToList();

            uiDispatcher.TryEnqueue(() =>
            {
                foreach (var track in batch)
                {
                    // REMOVED: The incorrect AllAvailableSongs check is gone!

                    var colorbrush = new SolidColorBrush(Microsoft.UI.Colors.White);
                    var glyph = "\uEC4F";

                    if (currentPlayingPath == track.FilePath)
                    {
                        colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
                        glyph = isMasterPlayerPlaying ? "\uE769" : "\uE768";
                    }

                    var song = new SongModel
                    {
                        Title = track.Title,
                        Artist = track.Artist,
                        AlbumName = track.AlbumName,
                        SongDuration = track.SongDuration,
                        FilePath = track.FilePath,
                        Year = System.IO.File.GetCreationTime(track.FilePath).Year,
                        Remove = "Remove from artist",
                        MediaType = "ArtistAll",
                        FavOpacity = track.IsFavourite ? 1.0 : 0.0,
                        FavString = track.IsFavourite ? "Remove from Favourites" : "Add to Favourites",
                        IsFavourite = track.IsFavourite,
                        TitleColor = colorbrush,
                        Glyph = glyph
                    };

                    FoundSongs.Add(song);
                    ts += track.SongDuration ?? TimeSpan.Zero;

                    if (track.AlbumName == "Unknown Album")
                    {
                        Singles.Add(song);
                    }
                }
            });

            await Task.Delay(16);
        }
        lstViewSingles.ItemsSource = Singles;
        lstViewAllSongs.ItemsSource = FoundSongs;

        // 4. Group albums smoothly
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

        //string formatted = ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss");
        //txtTotalDuration.Text = formatted;
        //txtSongCount.Text = $"• {FoundSongs.Count} {(FoundSongs.Count == 1 ? "item" : "items")}";
        //txtAlbumCount.Text = $"• {albumCollection.Count} {(albumCollection.Count == 1 ? "Album" : "Albums")}";

        UpdateUIEmptyStates();

        // Trigger History Loader
        await LoadMostPlayedSongsBackground(settings, targetArtist, favSet, currentPlayingPath, isMasterPlayerPlaying);

        FoundSongs.CollectionChanged -= FoundSongs_CollectionChanged;
        FoundSongs.CollectionChanged += FoundSongs_CollectionChanged;
        ttProgress.IsOpen = false;
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
    private void UpdateUIEmptyStates()

    {

        txtEmptySingles.Visibility = Singles.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        lstViewSingles.Visibility = Singles.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

        txtEmptySongs.Visibility = FoundSongs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        txtEmptyAlbums.Visibility = albumCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

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
                AudioMetadata.ChangeArtistName(file.Path, txtArtistName.Text);
                //var tagFile = TagLib.File.Create(file.Path);
                //string[] albumartists = tagFile.Tag.AlbumArtists;
                //List<string> artistss = albumartists.ToList();
                //var newArtist = txtArtistName.Text;

                //if (!string.IsNullOrEmpty(newArtist) &&
                //    !artistss.Any(a => a.Equals(newArtist, StringComparison.OrdinalIgnoreCase)))
                //{
                //    artistss.Add(newArtist);
                //    tagFile.Tag.AlbumArtists = artistss.ToArray();
                //    tagFile.Save();
                //}
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
    }

    private void MenuFlyoutItem_Click_2(object sender, RoutedEventArgs e)
    {

    }

    private async void OceanContentDialog_PrimaryRequested1()
    {

        OceanContentDialog.HideDlg();
        MainWindow.ShowWindow();
     //   await SearchFiles();


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
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Album", "Close", "", "", OceanDialogWindow.ContentType.AlbumDetails, OceanContentDialogDefault.Primary, XamlRoot, 800, 980, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "", "", "", name);
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested1;
        }
    }

    private void mnftAlbumInfo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel albumModel && albumModel.AlbumName is string name)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Album", "Close", "", "", OceanDialogWindow.ContentType.AlbumDetails, OceanContentDialogDefault.Primary, XamlRoot, 800, 980, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "", "", "", name);
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested1;
        }
    }

    private void mnftViewImage_Click(object sender, RoutedEventArgs e)
    {

    }
}

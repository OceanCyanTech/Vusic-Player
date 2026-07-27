using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
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
        FoundSongs.CollectionChanged -= FoundSongs_CollectionChanged;
        FoundSongs.CollectionChanged += FoundSongs_CollectionChanged;
        FileSystemWatch.FileModified -= FileSystemWatch_FileModified;
        FileSystemWatch.FileModified += FileSystemWatch_FileModified; ;
    }

    private void FileSystemWatch_FileModified(string arg1, string arg2, string arg3, string arg4)
    {
        var collections = new IEnumerable<SongModel>[] { FoundSongs, mostplayedsongs, Singles };

        foreach (var collection in collections)
        {
            var existingSong = collection.FirstOrDefault(p => p.FilePath == arg1);
            if (existingSong != null)
            {
                Debug.WriteLine("UPDATION " + arg1);

                DispatcherQueue.TryEnqueue(() =>
                {
                    existingSong.AlbumName = arg2;
                    existingSong.Artist = arg3;
                    existingSong.Title = arg4;
                });
            }
            else
            {
                Debug.WriteLine("TRURUE " + arg1);
                if(arg3.Contains(txtArtistName.Text))
                {
                    Debug.WriteLine("HAA ARTISTN AE");

                    if (selBarMain.SelectedItem == selBarItemAllSongs)
                    {
                        Debug.WriteLine("HAA SELECTED");
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            FoundSongs.Add(new SongModel { AlbumName = arg2, Artist = arg3, FilePath = arg1, Title = arg4 });
                        });
                    }
                }
            }
        }
    }

    private async void FoundSongs_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //    if (FoundSongs.Count == 0)
        //    {
        //        txtEmptySongs.Visibility = Visibility.Visible;
        //        lstViewAllSongs.Visibility = Visibility.Collapsed;
        //        btnPlayAll.IsEnabled = false;
        //        btnShuffle.IsEnabled = false;
        //        btnRenameArtist.IsEnabled = false;

        //    }
        //    else
        //    {
        //        Debug.WriteLine("DANANA");
        //        txtEmptySongs.Visibility = Visibility.Collapsed;
        //        lstViewAllSongs.Visibility = Visibility.Visible;
        //        btnPlayAll.IsEnabled = true;
        //        btnShuffle.IsEnabled = true;
        //        btnRenameArtist.IsEnabled = true;
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
                    //        ts += track.SongDuration ?? TimeSpan.Zero;

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


        ttProgress.IsOpen = false;
    }
    private async Task LoadAllFiles(List<string> searchPaths)
    {

        Microsoft.UI.Dispatching.DispatcherQueue dispatcherQueue = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var favourites = currentSettings.Favourites?.ToList() ?? new List<FavouriteItems>();

        // 1. Snapshot currently loaded paths
        var existingPaths = FoundSongs
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

                    var searchOption = SearchOption.AllDirectories;
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
                    if (FoundSongs.Any(s => string.Equals(Path.GetFullPath(s.FilePath), liteTrack.FilePath, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    // Instantiate your original SongModel safely inside the UI Thread Context
                    FoundSongs.Add(new SongModel
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
        }

    }

    private async Task LoadFiles()
    {
        string targetArtist = txtArtistName.Text.Trim();
        bool filterByArtist = !string.IsNullOrWhiteSpace(targetArtist);

        string[] searchPaths =
        {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos,
        UserDataPaths.GetDefault().Pictures
    };

        var existingPaths = FoundSongs
            .Select(s => string.IsNullOrWhiteSpace(s.FilePath) ? "" : Path.GetFullPath(s.FilePath))
            .Where(p => !string.IsNullOrEmpty(p))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var currentSettings = await SettingsLoader.LoadSettingsAsync();

        // HashSets provide O(1) fast lookup instead of .Any() O(N) lookup
        var favouritePaths = (currentSettings.Favourites ?? new ObservableCollection<FavouriteItems>())
            .Select(f => Path.GetFullPath(f.FilePath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Run the heavy disk I/O on a background thread
        List<AudioTrackLite> discoveredTracks = await Task.Run(() =>
        {
            var liteList = new List<AudioTrackLite>();
            var scannedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in searchPaths)
            {
                if (!Directory.Exists(path)) continue;

                try
                {
                    var directoryInfo = new DirectoryInfo(path);
                    var files = directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories)
                        .Where(f => AudioExtensions.List.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

                    foreach (var file in files)
                    {
                        string normalizedPath = Path.GetFullPath(file.FullName);

                        // Skip already loaded songs or duplicate paths across directories
                        if (existingPaths.Contains(normalizedPath) || !scannedPaths.Add(normalizedPath))
                            continue;

                        try
                        {
                            // Single-pass read using TagLibSharp
                            using var tagFile = TagLib.File.Create(normalizedPath);
                            var tag = tagFile.Tag;

                            string artist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
                                ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
                                : string.Join(", ", tag.AlbumArtists);

                            // Perform artist match check during single pass
                            if (filterByArtist)
                            {
                                bool matchesArtist = (!string.IsNullOrEmpty(artist) && artist.Contains(targetArtist, StringComparison.OrdinalIgnoreCase)) ||
                                                     (!string.IsNullOrEmpty(tag.FirstPerformer) && tag.FirstPerformer.Contains(targetArtist, StringComparison.OrdinalIgnoreCase));

                                if (!matchesArtist) continue;
                            }

                            string title = string.IsNullOrWhiteSpace(tag.Title)
                                ? Path.GetFileNameWithoutExtension(normalizedPath)
                                : tag.Title;

                            string album = string.IsNullOrWhiteSpace(tag.Album)
                                ? "Unknown Album"
                                : tag.Album;

                            bool isFav = favouritePaths.Contains(normalizedPath);

                            liteList.Add(new AudioTrackLite
                            {
                                Title = title,
                                AlbumName = album,
                                Artist = artist,
                                FilePath = normalizedPath,
                                SongDuration = tagFile.Properties.Duration,
                                IsFavourite = isFav
                            });
                        }
                        catch (Exception ex)
                        {
                            // File may be locked, corrupted, or unreadable
                            Debug.WriteLine($"TagLib error reading {normalizedPath}: {ex.Message}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error scanning directory {path}: {ex.Message}");
                }
            }

            return liteList;
        });

        // Batch update the UI collection
        int batchSize = 40;
        var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();

        for (int i = 0; i < discoveredTracks.Count; i += batchSize)
        {
            var batch = discoveredTracks.Skip(i).Take(batchSize).ToList();

            dispatcher.TryEnqueue(() =>
            {
                foreach (var liteTrack in batch)
                {
                    if (FoundSongs.Any(s => string.Equals(Path.GetFullPath(s.FilePath), liteTrack.FilePath, StringComparison.OrdinalIgnoreCase)))
                        continue;

                    FoundSongs.Add(new SongModel
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

            await Task.Delay(16);
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
            string[] searchPaths = {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Pictures,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos
    }; List<string> fpaths = new();
            foreach (var item in searchPaths)
            {

                fpaths.Add(item);

            }
            FileSystemWatch.WatchFolders(fpaths);
            try
            {
                // 1. Heavy DB work on background thread (returns plain C# objects)
                List<AudioTrackLite> rawSongs = await Task.Run(() => DatabaseService.GetAllSongs());

                // 2. Map and bind back on the UI Thread
                // (Task.Run returns execution to the UI thread automatically after 'await')
                string targetArtist = txtArtistName.Text;
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
                _ = RunBackgroundScannerAsync();
                txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";
                TotalDuration();
                foreach (var item in FoundSongs)
                {
                    Debug.WriteLine("The Duration of " + item.Title + " is " + item.FormattedDuration);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to load songs: {ex.Message}");
            }
        }
        LoadAlbums();
        base.OnNavigatedTo(e);
    }
    private void TotalDuration()
    {
        TimeSpan? timeSpan = TimeSpan.Zero;
        foreach (var item in FoundSongs.ToList())
        {

            timeSpan += item.SongDuration;
        }
        string formatted = timeSpan is TimeSpan ts
          ? (ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss"))
          : "0:00";
        txtTotalDuration.Text = formatted;
    }

    private async Task RunBackgroundScannerAsync()
    {
        var existingPaths = FoundSongs
            .Select(s => Path.GetFullPath(s.FilePath))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        string targetArtist = txtArtistName.Text;

        // Single call to your dedicated service
        await DatabaseService.ScanAndSyncDiskAsync(
            existingPaths,
            dispatcher,
newSong =>
{
    if (string.Equals(newSong.Artist, targetArtist, StringComparison.OrdinalIgnoreCase))
    {
        FoundSongs.Add(new SongModel
        {
            Title = newSong.Title,
            Artist = newSong.Artist,
            AlbumName = newSong.AlbumName,
            FilePath = newSong.FilePath
        });
    }
});
        txtSongCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";
        TotalDuration();

    }
    private async Task LoadFilesProgressiveAsync()
    {
        string targetArtist = txtArtistName.Text.Trim();
        bool filterByArtist = !string.IsNullOrWhiteSpace(targetArtist);

        string[] searchPaths =
        {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos,
        UserDataPaths.GetDefault().Pictures
    };

        var existingPaths = FoundSongs
            .Select(s => string.IsNullOrWhiteSpace(s.FilePath) ? "" : Path.GetFullPath(s.FilePath))
            .Where(p => !string.IsNullOrEmpty(p))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (App.MainWindowInstance != null)
        {
            // -------------------------------------------------------------
            // STAGE 1: Fast Path-Only Scan (< 50ms)
            // -------------------------------------------------------------
            var itemsToEnrich = new List<SongModel>();
            List<AudioTrackLite> discoveredtracks = new List<AudioTrackLite>();
            await Task.Run(() =>
            {
                foreach (var path in searchPaths)
                {
                    if (!Directory.Exists(path)) continue;

                    try
                    {
                        var directoryInfo = new DirectoryInfo(path);
                        var files = directoryInfo.EnumerateFiles("*.*", SearchOption.AllDirectories)
                            .Where(f => AudioExtensions.List.Contains(f.Extension, StringComparer.OrdinalIgnoreCase));

                        foreach (var file in files)
                        {
                            string normalizedPath = Path.GetFullPath(file.FullName);
                            Debug.WriteLine(normalizedPath + " loadingg");

                            if (existingPaths.Contains(normalizedPath)) continue;

                            // Temporary lightweight placeholder
                            var song = new AudioTrackLite
                            {
                                FilePath = normalizedPath,
                                Title = Path.GetFileNameWithoutExtension(normalizedPath), // Instant title fallback
                                Artist = "Loading...",
                                AlbumName = ""
                            };
                            discoveredtracks.Add(song);
                            //        FoundSongs.Add(song);
                            //   itemsToEnrich.Add(song);
                            //       existingPaths.Add(normalizedPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Path scan error: {ex.Message}");
                    }
                }
            });
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            // If target artist filtering is turned OFF, populate UI immediately with placeholders
            //if (!filterByArtist)
            //{
            //    foreach (var song in itemsToEnrich)
            //    {
            //        FoundSongs.Add(song);
            //    }
            //}
            foreach (var liteTrack in discoveredtracks)
            {
                FoundSongs.Add(new SongModel
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
            //   int batchSize = 40;
            //for (int i = 0; i < discoveredtracks.Count; i += batchSize)
            //{
            //    var batch = discoveredtracks.Skip(i).Take(batchSize).ToList();
            //    foreach (var liteTrack in batch)
            //    {
            //        // Direct duplicate guard-check against the UI collection
            //        if (FoundSongs.Any(s => string.Equals(Path.GetFullPath(s.FilePath), liteTrack.FilePath, StringComparison.OrdinalIgnoreCase)))
            //            continue;

            //        // Instantiate your original SongModel safely inside the UI Thread Context
            //        FoundSongs.Add(new SongModel
            //        {
            //            Title = liteTrack.Title,
            //            AlbumName = liteTrack.AlbumName,
            //            Artist = liteTrack.Artist,
            //            FilePath = liteTrack.FilePath,
            //            SongDuration = liteTrack.SongDuration,
            //            IsFavourite = liteTrack.IsFavourite,
            //            Glyph = "\uEC4F"
            //        });
            //    }
            //};

            // Breath frame for the UI engine
            await Task.Delay(16);
        }
        // -------------------------------------------------------------
        // STAGE 2: Background Tag Processing & Progressive UI Update
        // -------------------------------------------------------------
        //_ = Task.Run(() =>
        //{
        //    foreach (var song in itemsToEnrich)
        //    {
        //        try
        //        {
        //            using var tagFile = TagLib.File.Create(song.FilePath);
        //            var tag = tagFile.Tag;

        //            string artist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
        //                ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
        //                : string.Join(", ", tag.AlbumArtists);

        //            string title = string.IsNullOrWhiteSpace(tag.Title)
        //                ? Path.GetFileNameWithoutExtension(song.FilePath)
        //                : tag.Title;

        //            string album = string.IsNullOrWhiteSpace(tag.Album)
        //                ? "Unknown Album"
        //                : tag.Album;

        //            TimeSpan duration = tagFile.Properties.Duration;

        //            // Handle artist filter check on background thread
        //            if (filterByArtist)
        //            {
        //                bool matchesArtist = (!string.IsNullOrEmpty(artist) && artist.Contains(targetArtist, StringComparison.OrdinalIgnoreCase)) ||
        //                                     (!string.IsNullOrEmpty(tag.FirstPerformer) && tag.FirstPerformer.Contains(targetArtist, StringComparison.OrdinalIgnoreCase));

        //                if (!matchesArtist) continue; // Skip items that don't match target artist filter
        //            }

        //            // Batch or individual property push back to UI Thread
        //            dispatcher.TryEnqueue(() =>
        //            {
        //                if (filterByArtist && !FoundSongs.Contains(song))
        //                {
        //                    // Add matching item if filtering was enabled
        //                    FoundSongs.Add(song);
        //                }

        //                // Update properties (INotifyPropertyChanged handles XAML refresh)
        //                song.Title = title;
        //                song.Artist = artist;
        //                song.AlbumName = album;
        //                song.SongDuration = duration;
        //            });
        //        }
        //        catch
        //        {
        //            // Unreadable metadata / corrupted files fallback
        //            dispatcher.TryEnqueue(() =>
        //            {
        //                song.Artist = "Unknown Artist";
        //                song.AlbumName = "Unknown Album";
        //            });
        //        }
        //    }
        //});
    }


    private void UpdateUIEmptyStates()

    {


        txtEmptySongs.Visibility = FoundSongs.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

        txtEmptyAlbums.Visibility = albumCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

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
            //     await SearchFiles();
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
        //await SearchFiles();
    }

    // --- Discography & Album Events ---
    private void ifbError_CloseButtonClick(InfoBar sender, object args)
    {
        sender.Visibility = Visibility.Collapsed;
    }

    private async void btnRefresh_Click(object sender, RoutedEventArgs e)
    {
        //        await SearchFiles();
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

    private void OceanContentDialog_PrimaryRequested2()
    {
    }

    private void mnftRemoveAlbum_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is ArtistDiscAlbumModel albumModel && albumModel.AlbumName is string name)
        {
            var songs = albumModel.Songs;
            foreach (var song in songs)
            {
                Debug.WriteLine($"RENAME ALBUM: {song.FilePath}");

                // Write to file metadata asynchronously if possible, or keep synchronous if required
                AudioMetadata.ChangeAlbumName(song.FilePath, "");
            }
            LoadAlbums();

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

    private void btnRenameAlbum_Click(object sender, RoutedEventArgs e)
    {

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
}

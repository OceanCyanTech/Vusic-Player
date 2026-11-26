// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 241126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.FilePickers;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Storage;


namespace Vusic_Player.Pages.Views;

public class ArtistItemCountString : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return $"• {count} {(count == 1 ? "song" : "songs")}";
        }

        return "• 0 songs";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
public class ArtistAlbumCountString : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int count)
        {
            return $"• {count} {(count == 1 ? "album" : "albums")}";
        }

        return "• 0 albums";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}


public sealed partial class ArtistView : Page, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private readonly PlaylistDurationString _durationConverter = new();
    private readonly QueueDurationString _durationConverterWithBulletPoint = new();
    public ArtistView()
    {
        InitializeComponent();

        albumCollection.CollectionChanged -= AlbumCollection_CollectionChanged;
        albumCollection.CollectionChanged += AlbumCollection_CollectionChanged;
    }
    private ObservableCollection<SongModel> _foundSongs = new();
    public ObservableCollection<SongModel> FoundSongs
    {
        get => _foundSongs;
        set
        {
            if (_foundSongs != value)
            {
                _foundSongs = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FoundSongs)));
            }
        }
    }
    private ObservableCollection<SongModel> _singles = new();
    public ObservableCollection<SongModel> Singles
    {
        get => _singles;
        set
        {
            if (_singles != value)
            {
                _singles = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Singles)));
            }
        }
    }
    private ObservableCollection<SongModel> _mostplayed = new();
    public ObservableCollection<SongModel> MostPlayed
    {
        get => _mostplayed;
        set
        {
            if (_mostplayed != value)
            {
                _mostplayed = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(MostPlayed)));
            }
        }
    }
    public string FormatTotalDuration(int count)
    {
        return (string)_durationConverter.Convert(FoundSongs, typeof(string), null!, string.Empty);
    }

    public string FormatSinglesDuration(int count)
    {
        return (string)_durationConverterWithBulletPoint.Convert(Singles, typeof(string), null!, string.Empty);
    }
    public string FormatMostPlayedDuration(int count)
    {
        return (string)_durationConverterWithBulletPoint.Convert(mostplayedsongs, typeof(string), null!, string.Empty);
    }

    private void AlbumCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        txtEmptyAlbums.Visibility = albumCollection.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        grdViewAlbums.Visibility = albumCollection.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        txtAlbumCount.Text = "• " + $"{albumCollection.Count} {(albumCollection.Count == 1 ? "Album" : "Albums")}";

    }

    private readonly ConcurrentDictionary<string, CancellationTokenSource> _debouncers = new();

    private CancellationTokenSource? _albumReloadCts;

    ObservableCollection<ArtistDiscAlbumModel> albumCollection = new ObservableCollection<ArtistDiscAlbumModel>();
    HashSet<string> uniqueArtists = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    ObservableCollection<string> AlbumsList { get; set; } = new();

    private bool _isRenaming = false;

    private async Task<BitmapImage> LoadExistingThumbnailAsync(string albumname)
    {
        Uri fallbackUri = new Uri(IconHolder.Instance.DefaultAlbum);

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
    }]
    ALBUMS CONTEXT FLYOUT OPTIONS FAIL TO MAKE CHANGES IN RAWSONGS
    private readonly SemaphoreSlim _loadLock = new SemaphoreSlim(1, 1);

    private async void LoadSelectorBarSelectionSettings()
    {
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var selectorBarindex = currentSettings.ArtistView_selectorbarindex;
        if (selectorBarindex == 0)
        {
            selBarMain.SelectedItem = selBarItemMostPlayed;
            await LoadMostPlayed(txtArtistName.Text);
        }
        else if (selectorBarindex == 1)
        {
            selBarMain.SelectedItem = selBarItemAlbum;
            await LoadAlbums();
        }
        else if (selectorBarindex == 2)
        {
            selBarMain.SelectedItem = selBarItemSingles;
            await LoadSingles(txtArtistName.Text);
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
    private async void LoadArtistImage(string artistName)
    {
        Uri fallbackUri = new Uri(IconHolder.Instance.DefaultArtist);
        var currentSettings = await SettingsLoader.LoadSettingsAsync();
        var existingArtist = currentSettings.ArtistsList?

          .FirstOrDefault(a => a.Name == txtArtistName.Text);
        if (existingArtist != null && !string.IsNullOrEmpty(existingArtist.Thumbnail))
        {
            if (!File.Exists(existingArtist.Thumbnail))
            {
                imgArtist.ProfilePicture = new BitmapImage(new Uri(existingArtist.Thumbnail));
                Logger.Log($"Requested Artist profile picture doesn't exist on disk: {existingArtist.Thumbnail}", "ArtistPage.LoadArtistPicture", Logger.LogLevelType.Error);

                return;
            }
            try
            {
                imgArtist.ProfilePicture = new BitmapImage(new Uri(existingArtist.Thumbnail));
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to load artist profile picture, reverting to default: {ex.Message}", "ArtistPage", Logger.LogLevelType.Error);
                imgArtist.ProfilePicture = new BitmapImage(fallbackUri);
            }
        }
        else
        {
            imgArtist.ProfilePicture = new BitmapImage(fallbackUri);
        }
    }
    CancellationTokenSource? ct;
    CancellationTokenSource? ctsingles;
    CancellationTokenSource? ctmostplayed;
    private async Task LoadArtistMedia(string artistName)
    {
        ct?.Cancel();
        ct?.Dispose();
        ct = new CancellationTokenSource();
        CancellationToken token = ct.Token;
        FoundSongs.Clear();
        try
        {
            var songModels = await Task.Run(() =>
            {
                var results = new List<SongModel>();

                foreach (var song in FilesInDatabase.rawSongs)
                {
                    token.ThrowIfCancellationRequested();
                    if (song.Artist is not null && song.Artist.Contains(artistName, StringComparison.OrdinalIgnoreCase))
                    {
                        results.Add(new SongModel
                        {
                            Title = song.Title,
                            AlbumName = song.AlbumName,
                            Artist = song.Artist,
                            FilePath = song.FilePath,
                            SongDuration = song.SongDuration,
                            IsFileMissing = !File.Exists(song.FilePath)
                        });
                    }
                }
                return results;
            }, token);
            token.ThrowIfCancellationRequested();
            FoundSongs = new ObservableCollection<SongModel>(songModels);
        }
        catch (OperationCanceledException)
        {
            //ignore
        }
    }
    private async Task LoadSingles(string artistName)
    {
        ctsingles?.Cancel();
        ctsingles?.Dispose();
        ctsingles = new CancellationTokenSource();
        CancellationToken token = ctsingles.Token;
        Singles.Clear();
        try
        {
            var songModels = await Task.Run(() =>
            {
                var results = new List<SongModel>();
                foreach (var song in FilesInDatabase.rawSongs)
                {
                    token.ThrowIfCancellationRequested();
                    if (song.Artist.Contains(artistName, StringComparison.OrdinalIgnoreCase) && (string.Equals(song.AlbumName, "Unknown Album", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(song.AlbumName)))
                    {
                        results.Add(new SongModel
                        {
                            Title = song.Title,
                            AlbumName = "Unknown Album",
                            Artist = song.Artist,
                            FilePath = song.FilePath,
                            SongDuration = song.SongDuration,
                            IsFileMissing = !File.Exists(song.FilePath)
                        });
                    }
                }
                return results;
            }, token);
            token.ThrowIfCancellationRequested();
            Singles = new ObservableCollection<SongModel>(songModels);
        }
        catch (OperationCanceledException)
        {
            //ignore
        }
    }
    private async Task LoadMostPlayed(string artistName)
    {
        ctmostplayed?.Cancel();
        ctmostplayed?.Dispose();
        ctmostplayed = new CancellationTokenSource();
        CancellationToken token = ctmostplayed.Token;
        mostplayedsongs.Clear();
        try
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var top5 = await Task.Run(() =>
            {
                var sortedSongs = currentSettings.RecentMusic.OrderByDescending(x => x.PlayCount).ToList();
                var results = new ObservableCollection<SongModel>();
                foreach (var song in sortedSongs)
                {
                    token.ThrowIfCancellationRequested();
                    var existingsong = FoundSongs.FirstOrDefault(p => p.FilePath == song.SongPath);
                    if (existingsong != null)
                    {
                        results.Add(existingsong);
                    }
                }
                return results;
            }, token);
            token.ThrowIfCancellationRequested();

            foreach (var item in top5)
            {
                mostplayedsongs.Add(item);
            }
        }
        catch (OperationCanceledException)
        {
            //ignore
        }
    }
    private async Task LoadAlbums()
    {
        if (!await _loadLock.WaitAsync(0))
            return;

        try
        {
            var distinctSongs = FoundSongs
                .Where(song => !string.IsNullOrWhiteSpace(song?.AlbumName) && !string.IsNullOrWhiteSpace(song?.FilePath))
                .DistinctBy(song => song.FilePath.Trim(), StringComparer.OrdinalIgnoreCase)
                .ToList();
            var groupedAlbums = distinctSongs
                .GroupBy(song => song.AlbumName.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(async group =>
                {
                    var yearTasks = group
                        .Select(async song =>
                        {
                            try
                            {
                                var metadatayear = await MetadataFetcher.GetMediaYear(song.FilePath);
                                return metadatayear;
                            }
                            catch
                            {
                                return 0;
                            }
                        })
                        .ToList();
                    int[] years = await Task.WhenAll(yearTasks);
                    int finalYear = 0;
                    if (years.Length > 0)
                    {
                        var yearGroups = years.GroupBy(y => y).OrderByDescending(g => g.Count()).ToList();
                        bool isAllVaried = yearGroups.All(g => g.Count() == yearGroups[0].Count());
                        finalYear = isAllVaried ? years[0] : yearGroups[0].Key;
                    }

                    return new
                    {
                        AlbumName = group.First().AlbumName.Trim(),
                        CalculatedYear = finalYear,
                        Songs = group.ToList()
                    };
                });
            var unsortedAlbums = await Task.WhenAll(groupedAlbums);
            var sortedAlbums = unsortedAlbums.OrderBy(result => result.AlbumName, StringComparer.OrdinalIgnoreCase).ToList();
            var newAlbums = new List<ArtistDiscAlbumModel>();

            foreach (var albumGroup in sortedAlbums)
            {
                int countsongs = albumGroup.Songs.Count;
                string countsong = $"{countsongs} {(countsongs == 1 ? "item" : "items")}";

                BitmapImage img = await LoadExistingThumbnailAsync(albumGroup.AlbumName);

                newAlbums.Add(new ArtistDiscAlbumModel
                {
                    AlbumName = albumGroup.AlbumName,
                    AlbumCoverThumbnail = img,
                    AlbumYear = albumGroup.CalculatedYear > 0 ? albumGroup.CalculatedYear.ToString() : string.Empty,
                    Songs = albumGroup.Songs
                });
            }
            albumCollection.Clear();

            foreach (var album in newAlbums)
            {
                albumCollection.Add(album);
            }
        }
        finally
        {
            _loadLock.Release();
        }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string selectedSongArtist)
        {
            txtArtistName.Text = selectedSongArtist;

            imgArtist.DisplayName = txtArtistName.Text;

            LoadArtistImage(selectedSongArtist);
            try
            {
                await LoadArtistMedia(selectedSongArtist);
                LoadSelectorBarSelectionSettings();
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to load artist songs: " + ex.Message, "ArtistView.LoadSongs", Logger.LogLevelType.Error);
            }
        }
        FilesInDatabase.SongsDiscovered += FilesInDatabase_SongsDiscovered;
        try
        {
            await LoadAlbums();
        }
        catch (Exception ex)
        {
            Logger.Log("Failed to load artist albums: " + ex.Message, "ArtistView.LoadAlbumsInitial", Logger.LogLevelType.Error);
        }
        base.OnNavigatedTo(e);
    }
    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        FilesInDatabase.SongsDiscovered -= FilesInDatabase_SongsDiscovered;
    }

    private void FilesInDatabase_SongsDiscovered(IEnumerable<SongModel> newdiscovery)
    {
        foreach (var song in newdiscovery.ToList())
        {
            if (string.Equals(txtArtistName.Text, song.Artist, StringComparison.OrdinalIgnoreCase))
                FoundSongs.Add(song);
        }
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

            if (currentPlayingPath == item.FilePath)
            {
                colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);

            }

            mostplayedsongs.Add(new SongModel
            {
                Title = item.Title,
                AlbumName = item.AlbumName,
                Artist = item.Artist,
                FilePath = item.FilePath,

                SongDuration = item.SongDuration,
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
        imgArtist.ProfilePicture = new BitmapImage(new Uri(IconHolder.Instance.DefaultArtist));
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


    private void btnOpenRenameTT_Click(object sender, RoutedEventArgs e)
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
    private void OceanContentDialog_PrimaryRequested()
    {
        OceanContentDialog.HideDlg();
        MainWindow.ShowWindow();
    }

    private void btnPlayAll_Click(object sender, RoutedEventArgs e)
    {
        var liststring = new List<string>();
        foreach (var item in FoundSongs)
        {
            liststring.Add(item.FilePath);
        }
        QueueService.AddToQueueAndPlay(liststring, false, false);

    }

    private void btnShuffle_Checked(object sender, RoutedEventArgs e)
    {
        QueueObjects.QueueInstance.IsQueuePageShuffleEnabled = btnShuffle.IsChecked ?? false;
        QueueService.ToggleShuffle(btnShuffle.IsChecked ?? false);
    }


    private async void btnAddSongs_Click(object sender, RoutedEventArgs e)
    {
        if (App.MainWindowInstance == null) return;

        var files = await FilePickers.MediaPicker.PickMultipleAudioFilesAsync(App.MainWindowInstance, "Choose files");
        if (files == null || !files.Any()) return;

        string targetArtist = txtArtistName?.Text?.Trim() ?? "";

        foreach (var song in files)
        {
            var filepath = song.Path;
            var exist = FoundSongs.FirstOrDefault(p => p.FilePath == filepath);
            if (exist == null)
            {
                if (AudioMetadata.ChangeArtistName(song.Path, targetArtist) == false)
                {
                    Debug.WriteLine($"ERROR OCCURED IN ADDING {filepath} TO ARTIST: " + targetArtist);
                }
            }
        }
    }

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
        await LoadAlbums();
    }
    #region Albums Context Flyout Click 
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
            RoutedEventHandler? clickHandler = null;
            clickHandler = (object btnSender, RoutedEventArgs e) =>
            {
                btnRenameAlbum.Click -= clickHandler;
                ttRenameAlbum.IsOpen = false;

                var newName = txtRenameAlbum.Text.Trim();
                if (string.IsNullOrEmpty(newName) || newName == name)
                {
                    return;
                }
                albumModel.AlbumName = newName;
                var songs = albumModel.Songs;

                foreach (var song in songs)
                {
                    AudioMetadata.ChangeAlbumName(song.FilePath, newName);
                }
            };
            btnRenameAlbum.Click += clickHandler;
        }
    }


    #endregion


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
        if (string.IsNullOrWhiteSpace(albumname)) return;

        AlbumSongs.Clear();

        // 1. Fetch matching tracks directly from SQLite
        var songs = await DatabaseService.GetSongsByAlbumAsync(albumname);

        if (songs.Count == 0) return;

        await Task.Run(async () =>
        {
            foreach (var song in songs)
            {
                try
                {
                    if (File.Exists(song.FilePath))
                    {
                        // 2. Modify disk file metadata using TagLib
                        using var tagFile = TagLib.File.Create(song.FilePath);
                        tagFile.Tag.Album = "";
                        tagFile.Save();

                        // 3. Keep SQLite metadata in sync
                        await DatabaseService.UpdateSongMetadataAsync(
                            song.FilePath,
                            song.Title,
                            song.Artist,
                            newAlbum: "",
                            genre: song.Genre ?? "Unknown Genre"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed clearing album tag for '{song.FilePath}': {ex.Message}");
                }
            }
        });

        // 4. Update the in-memory rawSongs collection
        lock (FilesInDatabase.rawSongs)
        {
            foreach (var song in songs)
            {
                var match = FilesInDatabase.rawSongs.FirstOrDefault(s => s.FilePath == song.FilePath);
                if (match != null)
                {
                    match.AlbumName = "";
                }
            }
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
            txtArtistsInvolved.Text = albumModel.AlbumArtists.Replace("•", "");
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
                observablesongs.Add(new SongModel { Artist = song.Artist, Title = song.Title, FilePath = song.FilePath });
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


    private async void selBarMain_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        grdMostPlayed.Visibility = Visibility.Collapsed;
        grdArtistSingles.Visibility = Visibility.Collapsed;
        grdAlbums.Visibility = Visibility.Collapsed;
        grdAllSongs.Visibility = Visibility.Collapsed;
        if (selBarMain.SelectedItem == selBarItemMostPlayed)
        {
            grdMostPlayed.Visibility = Visibility.Visible;
            await LoadMostPlayed(txtArtistName.Text);
        }
        else if (selBarMain.SelectedItem == selBarItemAlbum)
        {
            grdAlbums.Visibility = Visibility.Visible;
            await LoadAlbums();
        }
        else if (selBarMain.SelectedItem == selBarItemSingles)
        {
            grdArtistSingles.Visibility = Visibility.Visible;
            await LoadSingles(txtArtistName.Text);
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

            FoundSongs = new ObservableCollection<SongModel>(songModels);

            LoadSelectorBarSelectionSettings();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Failed to load songs: {ex.Message}");
        }
        if (selBarMain.SelectedItem == selBarItemMostPlayed)
        {
            await LoadMostPlayed(txtArtistName.Text);
        }
        else if (selBarMain.SelectedItem == selBarItemAlbum)
        {
            await LoadAlbums();
        }
        else if (selBarMain.SelectedItem == selBarItemSingles)
        {
            await LoadSingles(txtArtistName.Text);
        }

    }
    private void btnPlayAllMostPlayed_Click(object sender, RoutedEventArgs e)
    {
        var liststring = new List<string>();
        foreach (var item in mostplayedsongs)
        {
            liststring.Add(item.FilePath);
        }
        QueueService.AddToQueueAndPlay(liststring, false, false);
    }

    private void btnPlayAllSingles_Click(object sender, RoutedEventArgs e)
    {
        var liststring = new List<string>();
        foreach (var item in Singles)
        {
            liststring.Add(item.FilePath);
        }
        QueueService.AddToQueueAndPlay(liststring, false, false);
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

        imgAlbumCover.Source = new BitmapImage(new Uri(IconHolder.Instance.DefaultAlbum));
    }

    private void mnftCopyFilePath_Click(object sender, RoutedEventArgs e)
    {
        if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song)
        {
            CopyToClipboard.CopyStringToClipboard(song.FilePath);
        }

    }

    private async void btnAddtoQueue_Click(object sender, RoutedEventArgs e)
    {
        QueueService.AddToQueue(FoundSongs);
        ttAddedtoQueue.IsOpen = true;
        await Task.Delay(2000);
        ttAddedtoQueue.IsOpen = false;
    }

    private async void btnAddtoQueueMostPlayed_Click(object sender, RoutedEventArgs e)
    {
        QueueService.AddToQueue(mostplayedsongs);
        ttAddedtoQueue.IsOpen = true;
        await Task.Delay(2000);
        ttAddedtoQueue.IsOpen = false;
    }

    private void grdViewAlbums_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        btnRemoveSelections.IsEnabled = grdViewAlbums.SelectedItems.Count > 0;
        btnClearSelection.IsEnabled = grdViewAlbums.SelectedItems.Count > 0;
    }
}

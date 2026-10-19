using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Storage;
using Windows.Storage.FileProperties;
using FileInfo = Vusic_Player.Configuration.Helper.FileInfo;

namespace Vusic_Player.Pages.Views
{

    public sealed partial class QueuePage : Page
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        public QueuePage()
        {
            InitializeComponent();
            //          frmNowPlaying.Navigate(typeof(MusicPlayerFull));
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
            UpdateSong(QueueService.VusicQueue?.FirstOrDefault(s => s.FilePath == filePath));
            UpdateSong(QueueService.VusicQueueNext?.FirstOrDefault(s => s.FilePath == filePath));

            void UpdateSong(SongModel? song)
            {
                if (song == null) return;

                song.AlbumName = newAlbum;
                song.Artist = newArtist;
                song.Title = newTitle;
                song.SongDuration = duration;
                Debug.WriteLine($"[Watcher] Queue updated: {filePath}");
            }
        }

        public ObservableCollection<SongModel> VusicQueueList => QueueService.VusicQueueNext;

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            QueueService.SyncQueueLists();
            var currentsettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentsettings.Favourites;
            if (QueueService.IsShuffleTrue)
            {
                tglShuffleQueue.IsChecked = true;
                UpdateAddonText();
                UpdateViews();
            }
            if (btnViewQueue.IsChecked == false)
            {
                int count = QueueService.VusicQueueNext.Count;
                mediacontroller.ItemsCount = $"• {count} {(count == 1 ? "item" : "items")}";
            }
            else
            {
                int count = QueueService.VusicQueue.Count;
                mediacontroller.ItemsCount = $"• {count} {(count == 1 ? "item" : "items")}";
            }
            lstViewQueue.UpdateGlyphs();
            base.OnNavigatedTo(e);
        }
        private async void asbFindQueue_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var currentQueue = (btnViewQueue.IsChecked == true)
              ? QueueService.VusicQueue
              : QueueService.VusicQueueNext;
            searchresults.Clear();
            var query = asbFindQueue.Text.ToLower().Trim();
            var minMatch = Regex.Match(query, @"(\d+)\s*(?:min|m)");
            var secMatch = Regex.Match(query, @"(\d+)\s*(?:sec|s)");
            int searchSeconds = 0;
            if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
            if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);
            var results = currentQueue.Where(s =>
            (s.Title != null && s.Title.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase)) ||
            (s.Artist != null && s.Artist.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase)) ||
            (s.AlbumName != null && s.AlbumName.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase)) ||
            (s.Year.ToString().ToLower().Contains(query, StringComparison.OrdinalIgnoreCase)) ||
            (searchSeconds > 0 && Math.Abs(s.SongDuration!.Value.TotalSeconds - searchSeconds) < 2)
         ).OrderByDescending(s =>
    s.Title?.StartsWith(query, StringComparison.OrdinalIgnoreCase) == true)
                    .ThenBy(s => s.Title)
                    .ToList();

            if (results.Any())
            {
                foreach (var item in results)
                {
                    searchresults.Add(item);
                }
                lstViewQueue.ItemsSource = searchresults;
                //       lstViewQueue.LoadMedia(searchresults, Frame);
            }
            else
            {
                lstViewQueue.Visibility = Visibility.Collapsed;
                if (currentQueue.Count != 0)
                {
                    grdNoSearchResults.Visibility = Visibility.Visible;
                    await Task.Delay(200);
                    frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
                }

            }
        }
        ObservableCollection<SongModel> searchresults = new();

        private void asbFindQueue_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var currentQueue = (btnViewQueue.IsChecked == true)
              ? QueueService.VusicQueue
              : QueueService.VusicQueueNext;
            if (asbFindQueue.Text == "")
            {
                searchresults.Clear();
                asbFindQueue.ItemsSource = null;
                grdNoSearchResults.Visibility = Visibility.Collapsed;

                lstViewQueue.ItemsSource = currentQueue;
                asbFindQueue.ItemsSource = null;
                lstViewQueue.Visibility = Visibility.Visible;
            }
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {

                searchresults.Clear();
                var rawQuery = asbFindQueue.Text.Trim();
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
                var results = currentQueue.Where(s =>
                {
                    // Check if any text matches (only if textQuery isn't empty)
                    bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                        (s.Title?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                        (s.Artist?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                        (s.AlbumName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                        (s.Year.ToString().Contains(textQuery))
                    );

                    // Check if duration matches (within 2 seconds)
                    bool durationMatch = (searchSeconds > 0 && s.SongDuration.HasValue &&
                                         Math.Abs(s.SongDuration.Value.TotalSeconds - searchSeconds) < 2);

                    // Return true if either the text matches OR the duration matches
                    return textMatch || durationMatch;
                })
                .OrderByDescending(s => s.Title?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
                .ThenBy(s => s.Title)
                .ToList();

                if (results.Any())
                {
                    asbFindQueue.ItemsSource = null;
                    foreach (var item in results)
                    {
                        searchresults.Add(item);
                    }
                    lstViewQueue.ItemsSource = searchresults;
                    //    lstViewQueue.LoadMedia(searchresults, Frame);
                }
                else
                {
                    var noresult = new List<string>();
                    noresult.Add("No matches found!");
                    asbFindQueue.ItemsSource = null;
                    asbFindQueue.ItemsSource = noresult;
                }
            }
        }

        private void btnRemoveSelectionFromQueue_Click(object sender, RoutedEventArgs e)
        {

            var itemsToRemove = lstViewQueue.SelectedItems.Cast<SongModel>().ToList();
            foreach (var song in itemsToRemove)
            {
                QueueService.VusicQueueNext.Remove(song);
                QueueService.VusicQueue.Remove(song);
            }
            var currentQueue = (mediacontroller.IsFullQueueMode == true) ? QueueService.VusicQueue : QueueService.VusicQueueNext;

            int count = currentQueue.Count;
            mediacontroller.QueuePageEmptyVisibility = (count == 0)
                   ? Visibility.Visible
                   : Visibility.Collapsed;
            mediacontroller.ItemsCount = $"• {count} {(count == 1 ? "item" : "items")}";
            TimeSpan totalDuration = TimeSpan.Zero;

            foreach (var item in currentQueue)
            {
                totalDuration += item.SongDuration ?? TimeSpan.Zero;
            }

            if (totalDuration.TotalHours < 1)
            {
                mediacontroller.TotalQueueRuntime = $"• {totalDuration.ToString(@"mm\:ss")}";
            }
            else
            {
                mediacontroller.TotalQueueRuntime = $"• {totalDuration.ToString(@"h\:mm\:ss")}";
            }
        }


        private void UpdateAddonText()
        {
            bool isLoop = tglLoopQueue.IsChecked == true;
            bool isShuffle = tglShuffleQueue.IsChecked == true;

            txtShuffled.Text = (isLoop, isShuffle) switch
            {
                (true, true) => "on loop and shuffled",
                (true, false) => "on loop",
                (false, true) => "shuffled",
                _ => ""
            };

            txtShuffled.Visibility = string.IsNullOrEmpty(txtShuffled.Text)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
        private void UpdateViews()
        {
            for (int i = QueueService.VusicQueue.Count - 1; i >= 0; i--)
            {
                var item = QueueService.VusicQueue[i];
                if (item.IsCompleted == false && item.FilePath != PlayerService.CurrentPlayingPath)
                {
                    QueueService.VusicQueue.RemoveAt(i);
                }
            }
            for (int i = 0; i < QueueService.VusicQueueNext.Count; i++)
            {
                var exist = QueueService.VusicQueue.FirstOrDefault(p => p.FilePath == QueueService.VusicQueueNext[i].FilePath);
                if (exist == null)
                {
                    QueueService.VusicQueue.Add(QueueService.VusicQueueNext[i]);
                }
            }
        }
        private void tglShuffleQueue_Checked(object sender, RoutedEventArgs e)
        {
            if (tglShuffleQueue.IsChecked == true)
            {
                QueueService.IsShuffleTrue = true;

                QueueService.ShuffleNext();


            }
            else
            {
                QueueService.IsShuffleTrue = false;

                QueueService.RestoreNext();


            }
            //      UpdateViews();
            UpdateAddonText();
        }

        private void tglLoopQueue_Checked(object sender, RoutedEventArgs e)
        {
            UpdateAddonText();
            QueueService.IsLoopTrue = tglLoopQueue.IsChecked ?? false;
        }



        private void btnClearQueue_Click(object sender, RoutedEventArgs e)
        {
            var currentQueue = (btnViewQueue.IsChecked == true)
               ? QueueService.VusicQueue
               : QueueService.VusicQueueNext;
            if (btnViewQueue.IsChecked == false)
            {
                QueueService.VusicQueueNext.Clear();
            }
            else
            {
                QueueService.VusicQueue.Clear();
                QueueService.VusicQueueNext.Clear();
            }
            mediacontroller.ItemsCount = "• 0 items";


            mediacontroller.TotalQueueRuntime = "• 00:00";
            mediacontroller.QueuePageEmptyVisibility = Visibility.Visible;



        }
        PlaylistCreationValues Instance => PlaylistCreationValues.Instance;

        private async void btnSaveQueue_Click(object sender, RoutedEventArgs e)
        {
            var currentQueue = (btnViewQueue.IsChecked == true)
? QueueService.VusicQueue
: QueueService.VusicQueueNext;
            Instance.MediaPaths.Clear();
            Instance.MediaSongModels.Clear();

            ttEditPlaylist.IsOpen = true;
            Instance.PlCover = new BitmapImage(new Uri("ms-appx:///Assets/playlistdefaultdark.png"));
            Instance.Thumbnail = new Uri("ms-appx:///Assets/playlistdefaultdark.png");

            Instance.PlaylistName = "Queue Playlist";
            Instance.Genre = "";
            Instance.MediaSongModels = new ObservableCollection<SongModel>(currentQueue);
            Instance.IsAppInstanceOceanDialog = false;

            RoutedEventHandler? saveHandler = null;
            saveHandler = async (s, args) =>
            {
                btnSavePlaylistEdited.Click -= saveHandler;
                Debug.WriteLine("SAVE SHOW CALLED");
                if (Instance.PlaylistName == "")
                {
                    Instance.PlaylistName = "Queue Playlist";
                }
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                string baseName = Instance.PlaylistName.Trim();

                string finalName = baseName;

                if (Instance.PlaylistName.Trim() != "Queue Playlist")
                {

                    if (string.IsNullOrEmpty(baseName)) baseName = "Queue Playlist";

                    int counter = 1;
                    while (currentSettings.SavedPlaylists.Any(p =>
                        string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalName = $"{baseName} ({counter++})";
                    }
                }

                var newPlaylist = new PlaylistItem
                {
                    PlaylistId = Guid.NewGuid().ToString(), // Assign a fresh unique ID
                    PlaylistName = finalName,
                    PlaylistGenre = Instance.Genre,
                    plthumb = Instance.PlCover,
                    Thumbnail = Instance.Thumbnail,
                    ThumbnailString = Instance.ThumbnailString,
                    SongsPaths = Instance.MediaPaths
                };

                currentSettings.SavedPlaylists.Add(newPlaylist);



                await SettingsLoader.SaveSettingsAsync(currentSettings);

                // Close popup/flyout and reset flags
                ttEditPlaylist.IsOpen = false;
                Instance.IsAppInstanceOceanDialog = true;
            };
            btnSavePlaylistEdited.Click += saveHandler;

        }
        private async Task UpdateAllMetadataAsync()
        {
            // Combine both collections into one sequence to avoid code duplication
            var allItems = QueueService.VusicQueue.Concat(QueueService.VusicQueueNext);

            // Start all tasks in parallel
            var updateTasks = allItems.Select(async item =>
            {
                try
                {
                    var file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                    var props = await file.Properties.GetMusicPropertiesAsync();

                    // Updating properties on the object will automatically 
                    // notify the UI if MusicItem implements INotifyPropertyChanged
                    item.Title = props.Title;
                    item.AlbumName = props.Album;
                    item.Artist = props.Artist;
                }
                catch (Exception ex)
                {
                    // Handle file not found or access denied
                    Debug.WriteLine($"Error loading {item.FilePath}: {ex.Message}");
                }
            });

            await Task.WhenAll(updateTasks);
        }
        private async void FileInfo_RefreshValues()
        {
            await UpdateAllMetadataAsync();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //Dispose Events
            FileInfo.RefreshValues -= FileInfo_RefreshValues;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            base.OnNavigatedFrom(e);
        }
        private void OceanContentDialog_PrimaryRequested()
        {
            PlaylistCreation.CallPlaylistCreation();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        private void btnViewQueue_Checked(object sender, RoutedEventArgs e)
        {
            QueueService.SyncQueueLists();
            var currentQueue = (btnViewQueue.IsChecked == true)
                ? QueueService.VusicQueue
                : QueueService.VusicQueueNext;
            btnRestartandPlay.Visibility = (btnViewQueue.IsChecked == true) ? Visibility.Visible : Visibility.Collapsed;
            mediacontroller.IsFullQueueMode = btnViewQueue.IsChecked ?? false;
            lstViewQueue.ItemsSource = currentQueue;
            UpdateUI(currentQueue);

            int count = currentQueue.Count;
            mediacontroller.QueuePageEmptyVisibility = (count == 0)
                   ? Visibility.Visible
                   : Visibility.Collapsed;
            mediacontroller.ItemsCount = $"• {count} {(count == 1 ? "item" : "items")}";
            var upnextstring = (btnViewQueue.IsChecked == true) ? "Full Queue" : "Up Next";
            txtUpNext.Text = upnextstring;
        }
        private void UpdateUI(ObservableCollection<SongModel> currentQueue)
        {
            int count = currentQueue.Count;
            mediacontroller.ItemsCount = $"• {count} {(count == 1 ? "item" : "items")}";

            TimeSpan totalDuration = TimeSpan.FromTicks(currentQueue.Sum(item => (item.SongDuration ?? TimeSpan.Zero).Ticks));

            string format = totalDuration.TotalHours < 1 ? @"mm\:ss" : @"h\:mm\:ss";
            mediacontroller.TotalQueueRuntime = $"• {totalDuration.ToString(format)}";
        }
        private async void btnAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var files = await FilePickers.MediaPicker.PickMultipleMediaFilesAsync(App.MainWindowInstance, "Choose files");
            if (files != null)
            {
                foreach (var singlefile in files)
                {
                    var alreadyexist = QueueService.VusicQueue.FirstOrDefault(p => p.FilePath == singlefile.Path);
                    if (alreadyexist == null)
                    {
                        if (File.Exists(singlefile.Path))
                        {
                            StorageFile file = await StorageFile.GetFileFromPathAsync(singlefile.Path);
                            string fileExtension = Path.GetExtension(file.Path).ToLower();
                            if (Extensions.AudioExtensions.List.Contains(fileExtension))
                            {
                                MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();

                                string title = !string.IsNullOrWhiteSpace(properties.Title) ? properties.Title : file.DisplayName;
                                string album = !string.IsNullOrWhiteSpace(properties.Album) ? properties.Album : "Unknown Album";
                                string artist = !string.IsNullOrWhiteSpace(properties.Artist) ? properties.Artist : "Unknown Artist";
                                QueueService.VusicQueue.Add(new SongModel
                                {
                                    Title = title,
                                    AlbumName = album,
                                    Artist = artist,
                                    SongDuration = properties.Duration,
                                    FilePath = file.Path,
                                });
                                QueueService.VusicQueueNext.Add(new SongModel
                                {
                                    Title = title,
                                    AlbumName = album,
                                    Artist = artist,
                                    SongDuration = properties.Duration,
                                    FilePath = file.Path,
                                });
                            }
                            else if (Extensions.VideoExtensions.List.Contains(fileExtension))
                            {
                                VideoProperties properties = await file.Properties.GetVideoPropertiesAsync();

                                string title = !string.IsNullOrWhiteSpace(properties.Title) ? properties.Title : file.DisplayName;
                                QueueService.VusicQueue.Add(new SongModel
                                {
                                    Title = title,

                                    SongDuration = properties.Duration,
                                    FilePath = file.Path,
                                });
                                QueueService.VusicQueueNext.Add(new SongModel
                                {
                                    Title = title,
                                    SongDuration = properties.Duration,
                                    FilePath = file.Path,
                                });
                            }
                        }
                    }

                }
            }

        }
        private void txtArtist_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame == null) return;
            App.NavigationFrame.Navigate(typeof(ArtistView), mediacontroller.ArtistDisplayName);
        }
        private void txtAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame == null) return;
            App.NavigationFrame.Navigate(typeof(AlbumView), mediacontroller.AlbumDisplayName);

        }
        private void sldMain_DragStarted()
        {
            PlayerService.SldMain_DragStarted();
        }

        private void sldMain_DragCompleted()
        {
            PlayerService.SldMain_DragCompleted(sldMain);
        }
        private void btnRelocate_Click(object sender, RoutedEventArgs e)
        {

        }



        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            asbFindQueue.Text = "";
            lstViewQueue.Focus(FocusState.Programmatic);
            asbFindQueue.ItemsSource = null;
        }



        private void btnRestartandPlay_Click(object sender, RoutedEventArgs e)
        {
            var refreshedobservablecoll = new ObservableCollection<SongModel>();
            foreach (var item in QueueService.VusicQueue.ToList())
            {
                item.IsCompleted = false;
                item.VisibilityOfStrikeThrough = Visibility.Collapsed;
                refreshedobservablecoll.Add(item);
            }

            QueueService.PlayMedia(refreshedobservablecoll, tglShuffleQueue.IsChecked ?? false, tglLoopQueue.IsChecked ?? false);
        }

        private void btnCancelEdit_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}

// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 161126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

#region Namespaces
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;

#endregion
namespace Vusic_Player.Pages.Views
{

    #region OceanCyan Header
    /*  OCEANCYAN TECH HEADER
VUSIC PLAYER - VERSION v1.0.7.0
© OCEANCYAN TECH 2026

PAGE - QUEUEPAGE
BUILD 131126
*/ 
    #endregion

    #region Converters
    public class QueueItemCount : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class ControlEnabled : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 0 ? false : true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class QueueItemCountBack : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class QueueItemCountString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return $"• {count} {(count == 1 ? "item" : "items")}";
            }

            return "• 0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class QueueDurationString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ObservableCollection<SongModel> media)
            {
                TimeSpan? ts = new TimeSpan();
                ts = TimeSpan.Zero;
                foreach (var item in media)
                {
                    ts += item.SongDuration;
                }
                return $"• {(ts!.Value.TotalHours >= 1 ? $"{(int)ts.Value.TotalHours}:{ts.Value:mm\\:ss}" : ts.Value.ToString(@"mm\:ss"))}";
            }

            return "• 00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    } 
    #endregion


    public sealed partial class QueuePage : Page
    {
        #region Fields
        private bool _isofficiallyqueuemodifying = false;

        private bool _isshuffling = false;

        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        private readonly QueueDurationString _durationConverter = new();

        public QueueObjects queueObjectsInstance => QueueObjects.QueueInstance;
        public ObservableCollection<SongModel> VusicQueueList = new ObservableCollection<SongModel>();
        ObservableCollection<SongModel> searchresults = new();
        PlaylistCreationValues Instance => PlaylistCreationValues.Instance;

        #endregion

        #region Helper
        public string FormatQueueDuration(int count)
        {
            return (string)_durationConverter.Convert(VusicQueueList, typeof(string), null!, string.Empty);
        }
        private void QueueService_NextMediaInitiated()
        {
            LoadQueue();
        }
        private List<SongModel> QueryResults(string query)
        {
            var rawQuery = query.Trim();
            var minMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:min|m)", RegexOptions.IgnoreCase);
            var secMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:sec|s)", RegexOptions.IgnoreCase);

            int searchSeconds = 0;
            if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
            if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);


            var textQuery = rawQuery;
            if (minMatch.Success) textQuery = textQuery.Replace(minMatch.Value, "");
            if (secMatch.Success) textQuery = textQuery.Replace(secMatch.Value, "");
            textQuery = textQuery.Trim().ToLower();

            var results = VusicQueueList.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.Title?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Artist?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.AlbumName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                    (s.Year.ToString().Contains(textQuery))
                );

                bool durationMatch = (searchSeconds > 0 && s.SongDuration.HasValue &&
                                     Math.Abs(s.SongDuration.Value.TotalSeconds - searchSeconds) < 2);

                return textMatch || durationMatch;
            })
            .OrderByDescending(s => s.Title?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.Title)
            .ToList();
            var ListSongModels = new List<SongModel>();
            if (results.Any())
            {
                foreach (var item in results)
                {
                    ListSongModels.Add(item);
                }
                return ListSongModels;
            }
            return ListSongModels;
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
        private void LoadQueue()
        {
            VusicQueueList.Clear();
            if (btnViewQueue.IsChecked == false)
            {
                var incompletequeueitems = queueObjectsInstance.QueueFull.Where(p => p.IsCompleted == false && p.IsCurrentQueueItem != true);
                foreach (var item in incompletequeueitems)
                {
                    VusicQueueList.Add(item);
                }
            }
            else
            {
                foreach (var item in queueObjectsInstance.QueueFull)
                {
                    VusicQueueList.Add(item);
                }
            }
        }
        private void RemoveSelectionFromQueue()
        {
            var itemsToRemove = lstViewQueue.SelectedItems.Cast<SongModel>().ToList();

            foreach (var item in itemsToRemove)
            {
                if (item.IsCurrentQueueItem) continue;
                queueObjectsInstance.QueueFull.Remove(item);
                VusicQueueList.Remove(item);
                if (tglShuffleQueue.IsChecked == true)
                {
                    queueObjectsInstance.QueueTemporary.Remove(item);
                }

            }
            var currentmediaplaying = QueueObjects.QueueInstance.QueueFull.FirstOrDefault(p => p.IsCurrentQueueItem == true);
            if (currentmediaplaying != null)
            {
                QueueObjects.QueueInstance.QueueIndex = QueueObjects.QueueInstance.QueueFull.IndexOf(currentmediaplaying);
            }
        }

        #endregion

        #region Page Properties
        public QueuePage()
        {
            InitializeComponent();
            QueueService.NextMediaInitiated += QueueService_NextMediaInitiated;
            lstViewQueue.ItemsSource = VusicQueueList;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            LoadQueue();
            base.OnNavigatedTo(e);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            base.OnNavigatedFrom(e);
        }

  

        #endregion

        #region Search
        private async void asbFindQueue_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var currentQueue = VusicQueueList;
            searchresults.Clear();
            searchresults = new ObservableCollection<SongModel>(QueryResults(asbFindQueue.Text));
            if (searchresults.Count != 0)
            {
                lstViewQueue.ItemsSource = searchresults;
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
        private void asbFindQueue_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var currentQueue = VusicQueueList;

            asbFindQueue.ItemsSource = null;
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
                searchresults = new ObservableCollection<SongModel>(QueryResults(asbFindQueue.Text));
                if (searchresults.Count != 0)
                {
                    asbFindQueue.ItemsSource = null;

                    lstViewQueue.ItemsSource = searchresults;
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
        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            asbFindQueue.Text = "";
            lstViewQueue.Focus(FocusState.Programmatic);
            asbFindQueue.ItemsSource = null;
        }

        #endregion

        #region Button/HyperLink/Toggle Click/Checked Events
        private void btnClearQueue_Click(object sender, RoutedEventArgs e)
        {
            QueueService.ClearQueue();
            VusicQueueList.Clear();
        }
        private void btnRemoveSelectionFromQueue_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectionFromQueue();
        }
        private void btnRestartandPlay_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in queueObjectsInstance.QueueFull)
            {
                item.IsCompleted = false;
            }
            foreach (var item in queueObjectsInstance.QueueTemporary)
            {
                item.IsCompleted = false;
            }
            queueObjectsInstance.QueueIndex = 0;
            QueueService.PlayMediaAtIndex(0);
            LoadQueue();
        }

        private void btnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ttEditPlaylist.IsOpen = false;
        }

        private async void tglShuffleQueue_Checked(object sender, RoutedEventArgs e)
        {
            if (_isshuffling) return;
            try
            {
                _isshuffling = true;
                if (tglShuffleQueue.IsChecked == true)
                {
                    QueueService.ShuffleQueue();
                }
                else
                {
                    QueueService.RestoreQueueItems();
                }
                LoadQueue();
                UpdateAddonText();
            }
            finally
            {
                _isshuffling = false;
            }
        }

        private void hypPlaylistCreated_Click(object sender, RoutedEventArgs e)
        {
            if (hypPlaylistCreated.Tag is string str && str == "LogPage")
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(LoggerPage));
                }
            }
            else
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(PlaylistView), hypPlaylistCreated.Tag as PlaylistItem);
                }
            }
        }
        private void tglLoopQueue_Checked(object sender, RoutedEventArgs e)
        {
            UpdateAddonText();
            queueObjectsInstance.IsLoop = tglLoopQueue.IsChecked ?? false;
        }


        private async void btnSaveQueue_Click(object sender, RoutedEventArgs e)
        {
            var currentQueue = VusicQueueList;
            Instance.MediaPaths.Clear();
            Instance.MediaSongModels.Clear();

            string finalName = "";
            ttEditPlaylist.IsOpen = true;
            Instance.PlCover = new BitmapImage(new Uri(IconHolder.Instance.PlaylistIcon));
            Instance.Thumbnail = new Uri(IconHolder.Instance.PlaylistIcon);

            Instance.PlaylistName = "Queue Playlist";
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            int counter2 = 1;
            while (currentSettings.SavedPlaylists.Any(p =>
                string.Equals(p.PlaylistName, Instance.PlaylistName, StringComparison.OrdinalIgnoreCase)))
            {
                Instance.PlaylistName = $"Queue Playlist ({counter2++})";
            }
            Instance.Genre = "";
            Instance.MediaSongModels = new ObservableCollection<SongModel>(currentQueue);
            foreach (var media in Instance.MediaSongModels)
            {
                Instance.MediaPaths.Add(media.FilePath);
            }
            Instance.IsAppInstanceOceanDialog = false;
            PlaylistItem plitem = new PlaylistItem();
            RoutedEventHandler? saveHandler = null;
            saveHandler = async (s, args) =>
            {
                try
                {
                    btnSavePlaylistEdited.Click -= saveHandler;
                    if (Instance.PlaylistName == "")
                    {
                        Instance.PlaylistName = "Queue Playlist";
                    }
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    string baseName = Instance.PlaylistName.Trim();

                    finalName = baseName;

                    if (string.IsNullOrEmpty(baseName)) baseName = "Queue Playlist";

                    int counter = 1;
                    while (currentSettings.SavedPlaylists.Any(p =>
                        string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalName = $"{baseName} ({counter++})";
                    }


                    var newPlaylist = new PlaylistItem
                    {
                        PlaylistId = Guid.NewGuid().ToString(),
                        PlaylistName = finalName,
                        PlaylistGenre = Instance.Genre,
                        plthumb = Instance.PlCover,
                        Thumbnail = Instance.Thumbnail,
                        ThumbnailString = Instance.ThumbnailString,
                        SongsPaths = Instance.MediaPaths
                    };
                    plitem = newPlaylist;

                    currentSettings.SavedPlaylists.Add(newPlaylist);



                    await SettingsLoader.SaveSettingsAsync(currentSettings);

                    ttEditPlaylist.IsOpen = false;
                    Instance.IsAppInstanceOceanDialog = true;
                }
                catch (Exception ex)
                {
                    imgState.Source = new BitmapImage(new Uri(IconHolder.Instance.Error));
                    txtPlaylistCreationStatus.Text = "An unexpected error occured. Check Log Page for more info!";
                    Logger.Log(ex.Message, "QueuePage.PlaylistCreation", Logger.LogLevelType.Error);
                    hypPlaylistCreated.Content = "Open Log Page";
                    hypPlaylistCreated.Tag = "LogPage";
                    ttPlaylistCreated.IsOpen = true;

                }
                finally
                {
                    imgState.Source = new BitmapImage(new Uri(IconHolder.Instance.Success));
                    txtPlaylistCreationStatus.Text = "Queue saved to new playlist successfully! Open below.";
                    hypPlaylistCreated.Content = finalName;
                    hypPlaylistCreated.Tag = plitem;
                    ttPlaylistCreated.IsOpen = true;
                }
            };
            btnSavePlaylistEdited.Click += saveHandler;

        }
        private void btnViewQueue_Checked(object sender, RoutedEventArgs e)
        {
            LoadQueue();
            var upnextstring = (btnViewQueue.IsChecked == true) ? "Full Queue" : "Up Next";
            txtUpNext.Text = upnextstring;
        }
        private async void btnAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var files = await FilePickers.MediaPicker.PickMultipleMediaFilesAsync(App.MainWindowInstance, "Choose files");
            if (files != null)
            {
                foreach (var singlefile in files)
                {
                    var songmodelreq = await MetadataFetcher.GetSongModel(singlefile.Path, true);
                    queueObjectsInstance.QueueFull.Add(songmodelreq);
                    VusicQueueList.Add(songmodelreq);
                    if (tglShuffleQueue.IsChecked == true)
                    {
                        queueObjectsInstance.QueueTemporary.Add(songmodelreq);
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


        #endregion

        #region ListViewMedia
        private void lstViewQueue_DragItemsCompleted()
        {
            var currentmedia = VusicQueueList.FirstOrDefault(p => p.IsCurrentQueueItem == true);

            if (currentmedia != null)
            {
                var index = VusicQueueList.IndexOf(currentmedia);
                QueueObjects.QueueInstance.QueueIndex = index;
                //Every media prior
                for (int i = 0; i < index; i++)
                {
                    var track = VusicQueueList[i];
                    if (track != null)
                    {
                        track.IsCompleted = true;
                    }
                }
                //Every media after
                for (int i = index + 1; i < VusicQueueList.Count; i++)
                {
                    var track = VusicQueueList[i];
                    if (track != null)
                    {
                        track.IsCompleted = false;
                    }
                }
            }
            if (btnViewQueue.IsChecked == false)
            {
                var completeditems = QueueObjects.QueueInstance.QueueFull.Where(p => p.IsCompleted == true);

                if (_isofficiallyqueuemodifying == true) return;
                try
                {
                    _isofficiallyqueuemodifying = true;
                    var currentmediaplaying = QueueObjects.QueueInstance.QueueFull
                        .FirstOrDefault(p => p.IsCurrentQueueItem == true);
                    var newQueue = new List<SongModel>();
                    newQueue.AddRange(completeditems);
                    if (currentmediaplaying != null)
                    {
                        newQueue.Add(currentmediaplaying);
                    }
                    newQueue.AddRange(VusicQueueList);
                    QueueObjects.QueueInstance.QueueFull.Clear();
                    foreach (var item in newQueue)
                    {
                        QueueObjects.QueueInstance.QueueFull.Add(item);
                    }
                    if (currentmediaplaying != null)
                    {
                        QueueObjects.QueueInstance.QueueIndex = newQueue.IndexOf(currentmediaplaying);
                    }
                    else
                    {
                        QueueObjects.QueueInstance.QueueIndex = -1;
                    }
                }
                finally
                {
                    _isofficiallyqueuemodifying = false;
                }
            }
            else
            {
                if (_isofficiallyqueuemodifying == true) return;
                try
                {
                    QueueObjects.QueueInstance.QueueFull.Clear();
                    foreach (var item in VusicQueueList)
                    {
                        QueueObjects.QueueInstance.QueueFull.Add(item);
                    }
                }
                finally
                {
                    _isofficiallyqueuemodifying = false;
                }
            }
        }

        private void lstViewQueue_RemoveSelectionFromQueue(List<SongModel> obj)
        {
            RemoveSelectionFromQueue();
        }
        private void lstViewQueue_RemoveSingleFromQueue(SongModel obj)
        {
            VusicQueueList.Remove(obj);
            queueObjectsInstance.QueueFull.Remove(obj);
            if (tglShuffleQueue.IsChecked == true)
            {
                queueObjectsInstance.QueueTemporary.Remove(obj);
            }
        }

        #endregion

        #region Playback
        private void sldMain_DragStarted()
        {
            PlayerService.SldMain_DragStarted();
        }

        private void sldMain_DragCompleted()
        {
            PlayerService.SldMain_DragCompleted(sldMain);
        }

        #endregion

        #region Miscellaneous 
        private void OceanContentDialog_PrimaryRequested()
        {
            PlaylistCreation.CallPlaylistCreation();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        #endregion

    }

}

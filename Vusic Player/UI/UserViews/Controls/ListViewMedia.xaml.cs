using CommunityToolkit.WinUI;
using FlyleafLib.MediaPlayer;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class ListViewMedia : UserControl
    {
        public bool AllowRearranging
        {
            get => (bool)GetValue(AllowRearrangingProperty);
            set => SetValue(AllowRearrangingProperty, value);
        }

        public static readonly DependencyProperty AllowRearrangingProperty =
            DependencyProperty.Register(
                nameof(AllowRearranging),
                typeof(bool),
                typeof(ListViewMedia),
                new PropertyMetadata(true, OnAllowRearrangingChanged));
        private static void OnAllowRearrangingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ListViewMedia control)
            {
                bool canRearrange = (bool)e.NewValue;
                // Update the internal ListView's properties
                control.lstViewPlaylist.CanReorderItems = canRearrange;
                control.lstViewPlaylist.AllowDrop = canRearrange;
            }
        }
        public IList<object> SelectedItems
        {
            get => (IList<object>)GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }
        public enum ListViewMediaSource
        {
            Playlist,
            QueueList,
            Artist,
            Album,
            AlbumEditor,
            MassEditor,
            ArtistNoRearrange,
            Genre
        }
        public static readonly DependencyProperty SelectedItemsProperty =
           DependencyProperty.Register(
               nameof(SelectedItems),
               typeof(IList<object>),
               typeof(ListViewMedia),
               new PropertyMetadata(new List<object>()));
        public ObservableCollection<SongModel> ItemsSource
        {
            get => (ObservableCollection<SongModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<SongModel>),
        typeof(ListViewMedia),
        new PropertyMetadata(null));

        public Visibility SearchVisibility
        {
            get => (Visibility)GetValue(searchvisiblity);
            set => SetValue(searchvisiblity, value);
        }
        public static readonly DependencyProperty searchvisiblity =
    DependencyProperty.Register(
        nameof(SearchVisibility),
        typeof(Visibility),
        typeof(ListViewMedia),
        new PropertyMetadata(Visibility.Visible));

        public Visibility FolderFileProperties
        {
            get => (Visibility)GetValue(folderfiprops);
            set => SetValue(folderfiprops, value);
        }
        public static readonly DependencyProperty folderfiprops =
    DependencyProperty.Register(
        nameof(FolderFileProperties),
        typeof(Visibility),
        typeof(ListViewMedia),
        new PropertyMetadata(Visibility.Collapsed));
        public bool IsFolderItems
        {
            get => (bool)GetValue(isfolderit);
            set => SetValue(isfolderit, value);
        }
        public static readonly DependencyProperty isfolderit =
    DependencyProperty.Register(
        nameof(IsFolderItems),
        typeof(bool),
        typeof(ListViewMedia),
        new PropertyMetadata(false));
        public Visibility ExtraOptions
        {
            get => (Visibility)GetValue(extraopt);
            set => SetValue(extraopt, value);
        }
        public static readonly DependencyProperty extraopt =
    DependencyProperty.Register(
        nameof(ExtraOptions),
        typeof(Visibility),
        typeof(ListViewMedia),
        new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty MediaSourceProperty =
            DependencyProperty.Register(
                nameof(MediaSource),
                typeof(ListViewMediaSource),            // Type of the property
                typeof(ListViewMedia),          // Owner type
                new PropertyMetadata(ListViewMediaSource.Playlist) // Default value
            );
        public static readonly DependencyProperty ButtonView = DependencyProperty.Register("Value", typeof(Visibility), typeof(ListViewMedia), new PropertyMetadata(Visibility.Collapsed));
        public Visibility VisiblityofViewButton
        {
            get => (Visibility)GetValue(ButtonView);
            set => SetValue(ButtonView, value);
        }

        public static readonly DependencyProperty IsModeListProperty =
          DependencyProperty.Register(
              nameof(isModeList),                      // 1. Matches the property wrapper name
              typeof(bool),                            // 2. Property type
              typeof(ListViewMedia),                   // 3. Owner type (your UserControl)
              new PropertyMetadata(true, OnIsModeListChanged) // 4. Default value + Callback method
          );

        public bool isModeList
        {
            get => (bool)GetValue(IsModeListProperty);
            set => SetValue(IsModeListProperty, value);
        }

        // 5. This static method is triggered whenever the property changes
        private static void OnIsModeListChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Since the method is static, we must cast 'd' back to your actual control instance
            if (d is ListViewMedia control)
            {
                bool newValue = (bool)e.NewValue;
                bool oldValue = (bool)e.OldValue;

                // Call your instance method here
                control.UpdateLayoutForMode(newValue);
            }
        }

        // 6. Your actual instance method where you do the work
        private void UpdateLayoutForMode(bool isList)
        {
            if (isList)
            {
                isViewList = true;

                grdViewMain.Visibility = Visibility.Collapsed;
                lstViewPlaylist.Visibility = Visibility.Visible;
                chckSelectGridView.Visibility = Visibility.Collapsed;
            }
            else
            {
                ViewGrid();
            }
        }


        // The wrapper for XAML access
        public ListViewMediaSource MediaSource
        {
            get => (ListViewMediaSource)GetValue(MediaSourceProperty);
            set => SetValue(MediaSourceProperty, value);
        }
        public Visibility GetVisibilityForSourceType(ListViewMediaSource status, string targetType)
        {
            // Return Visible if the status matches the string we pass from XAML
            if (status.ToString() == targetType)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }
        public ListViewMedia()
        {
            InitializeComponent();
            this.DataContext = this;
            PlayerService.PlayCalled += PlayerService_PlayCalled;
            UpdateGlyphs();
        }
        public async void UpdateGlyphs()
        {
            Debug.WriteLine("Called 1");

            if (this.IsLoaded == false || ItemsSource == null) return;
            Debug.WriteLine("Called 2");

            foreach (var item in ItemsSource.ToList())
            {
                item.TitleColor = new SolidColorBrush(Microsoft.UI.Colors.White);

                var storagefile = await StorageFile.GetFileFromPathAsync(item.FilePath);

                string fileExtension = Path.GetExtension(item.FilePath ?? "").ToLower();
                if (Extensions.VideoExtensions.List.Contains(fileExtension))
                {
                    Debug.WriteLine("Called 3" + item.FilePath);

                    item.Glyph = "\uE8B2";
                }
                else if (Extensions.AudioExtensions.List.Contains(fileExtension))
                {
                    Debug.WriteLine("Called 4" + item.FilePath);

                    item.Glyph = "\uEC4F";
                }
            }
            var exist = ItemsSource.ToList().FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            if (exist == null) return;
            Debug.WriteLine("TRUHAM 1");
            exist.TitleColor = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
            if (PlayerService.Masterplayer!.IsPlaying)
            {
                Debug.WriteLine("Called 5" + exist.FilePath);

                exist.Glyph = "\uE769";
            }
            else
            {
                Debug.WriteLine("Called 6 " + exist.FilePath);

                exist.Glyph = "\uE768";
            }
            //if (Extensions.VideoExtensions.List.Contains(Path.GetExtension(exist.FilePath)))

        }
        private async void PlayerService_PlayCalled()
        {
           UpdateGlyphs();
            //{
            //    exist.Glyph = "\uE8B2";
            //}

        }

        public event EventHandler<SelectionChangedEventArgs>? ListViewSelectionChange;
        public event EventHandler<RoutedEventArgs>? ListViewRemoved;
        private void lstViewPlaylist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isViewList)
            {
                stkMultiOptions.Visibility = lstViewPlaylist.SelectedItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                SelectedItems = lstViewPlaylist.SelectedItems;


            }
            else
            {
                stkMultiOptions.Visibility = grdViewMain.SelectedItems.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
                SelectedItems = grdViewMain.SelectedItems;

            }
            int count = SelectedItems.Count;
            if (ttAddToPlaylist.IsOpen == true)
            {
                txtItemsCountAddtoPl.Text = $"• {count} {(count == 1 ? "item" : "items")} selected";
            }
            if (count == 0)
            {
                btnAddFinalPlaylists.IsEnabled = false;
            }
            else
            {
                btnAddFinalPlaylists.IsEnabled = true;
            }
            var selectedcast = SelectedItems.Cast<SongModel>();


            bool hasVideo = selectedcast.Any(song =>
     !string.IsNullOrEmpty(song.FilePath) &&
     Extensions.VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant())
    );
            if (hasVideo)
            {
                btnEditAlbumMass.Visibility = Visibility.Collapsed;
                btnEditArtistMass.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnEditAlbumMass.Visibility = Visibility.Visible;
                btnEditArtistMass.Visibility = Visibility.Visible;
            }

            ListViewSelectionChange?.Invoke(sender, e);
        }
        public void UpdateAlbumNameForSelected(string name)
        {
            var selected = lstViewPlaylist.SelectedItems.Cast<SongModel>().ToList();
            foreach (var item in selected)
            {
                item.AlbumName = name;
            }
        }
        public void UpdateArtistNameForSelected(string name)
        {
            var selected = lstViewPlaylist.SelectedItems.Cast<SongModel>().ToList();
            foreach (var item in selected)
            {
                item.Artist = name;
            }
        }
        public void HideSort()
        {
            btnSortItems.Visibility = Visibility.Collapsed;
        }
        private async void mnftContext_Opened(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            if (flyout == null) return;
            var item = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Add to Play Queue");

            if (item != null)
            {
                if (MediaSource == ListViewMediaSource.QueueList)
                {
                    item.Visibility = Visibility.Collapsed;
                }
                else
                {
                    item.Visibility = Visibility.Visible;
                }
            }
            var artistitem = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Go to artist");
            var albumitem = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Go to album");
            var navVis = (MediaSource == ListViewMediaSource.Artist || MediaSource == ListViewMediaSource.ArtistNoRearrange || MediaSource == ListViewMediaSource.AlbumEditor)
     ? Visibility.Collapsed
     : Visibility.Visible;
            var navVis2 = (MediaSource == ListViewMediaSource.Album || MediaSource == ListViewMediaSource.AlbumEditor)
     ? Visibility.Collapsed
     : Visibility.Visible;
            if (artistitem != null) artistitem.Visibility = navVis;
            if (albumitem != null) albumitem.Visibility = navVis2;
            var moveup = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Move up");
            var movedown = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Move down");
            var movetotop = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Move to top");
            var movetobottom = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Move to bottom");
            var removefromGenre = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Remove from Genre");
            var remove = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Remove");

            var navVisibility = (MediaSource == ListViewMediaSource.AlbumEditor || MediaSource == ListViewMediaSource.ArtistNoRearrange)
                ? Visibility.Collapsed
                : Visibility.Visible;
            var removegenreVisibility = (MediaSource == ListViewMediaSource.Genre) ? Visibility.Visible : Visibility.Collapsed;
            var StandaloneRemovegenreVisibility = (MediaSource == ListViewMediaSource.Genre) ? Visibility.Collapsed : Visibility.Visible;
            if (moveup != null) moveup.Visibility = navVisibility;
            if (movedown != null) movedown.Visibility = navVisibility;
            if (movetotop != null) movetotop.Visibility = navVisibility;
            if (movetobottom != null) movetobottom.Visibility = navVisibility;
            if (removefromGenre != null) removefromGenre.Visibility = removegenreVisibility;
            if (remove != null) remove.Visibility = StandaloneRemovegenreVisibility;

            var addToPlaylist = flyout?.Items
        .OfType<MenuFlyoutSubItem>()
        .FirstOrDefault(x => x.Text == "Add to Playlist");

            if (addToPlaylist == null)
                return;

            addToPlaylist.Items.Clear();
            var selectedsong = addToPlaylist?.DataContext as SongModel;
            if (selectedsong == null) return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var Playlists = currentSettings.SavedPlaylists;
            foreach (var playliitem in Playlists)
            {
                MenuFlyoutItem playlistitem = new MenuFlyoutItem();
                playlistitem.Text = playliitem.PlaylistName;
                addToPlaylist?.Items.Add(playlistitem);
                playlistitem.Click += async (sender, e) =>
                {
                    var path = selectedsong?.FilePath;

                    if (path != null)
                    {
                        if (playliitem.SongsPaths.Contains(path))
                        {
                            ttAddedToPlaylist.Title = $"{Path.GetFileNameWithoutExtension(path)} already exists in {playliitem.PlaylistName}";
                        }
                        else
                        {
                            playliitem.SongsPaths.Add(path);
                            int count = playliitem.SongsPaths.Count;
                            playliitem.PlaylistCount = $"{count} {(count == 1 ? "item" : "items")}";
                            await SettingsLoader.SaveSettingsAsync(currentSettings);
                            ttAddedToPlaylist.Title = $"{Path.GetFileNameWithoutExtension(path)} has been added to {playliitem.PlaylistName}";

                        }
                        hypPlaylistAdded.Content = playliitem.PlaylistName;
                        hypPlaylistAdded.Tag = playliitem;
                        ttAddedToPlaylist.IsOpen = true;
                        await Task.Delay(3000);
                        ttAddedToPlaylist.IsOpen = false;
                    }
                };

            }

            //Favourites 
            var mnftAddtoFav = flyout?.Items
    .OfType<MenuFlyoutItem>()
    .FirstOrDefault(x => x.Name == "mnftAddToFavourites");

            if (mnftAddtoFav == null) return;
            if (selectedsong.IsFavourite == true)
            {
                mnftAddtoFav.Text = "Remove from Favourites";
            }
            else
            {

                mnftAddtoFav.Text = "Add to Favourites";
            }
        }

        private async void PlaySelection(SongModel selectedSong)
        {
            if (selectedSong.FilePath != null)
            {
                if (File.Exists(selectedSong.FilePath))
                {
                    var file = await StorageFile.GetFileFromPathAsync(selectedSong.FilePath);
                    string fileExtension = file.FileType.ToLowerInvariant();
                    bool isVideo = false;
                    if (Extensions.VideoExtensions.List.Contains(fileExtension))
                    {
                        isVideo = true;
                    }
                    if (isVideo == false)
                    {
                        ObservableCollection<SongModel> single = new();
                        string Title = Path.GetFileNameWithoutExtension(selectedSong.FilePath);
                        single.Add(new SongModel { FilePath = selectedSong.FilePath, Title = Title });
                        QueueService.PlayMedia(single, false, false);
                    }
                    else
                    {
                        //if (App.UltimateFrame != null)
                        //{
                        //    if (App.NavigationFrame == null) return;
                        //    NavigationManager.LastContentPageType = App.NavigationFrame.CurrentSourcePageType;
                        //    App.UltimateFrame.Navigate(typeof(VideoPlay), selectedSong.FilePath);
                        //}

                    }
                }
            }
        }

        private void mnftPlaySong_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song)
            {
                if (MediaSource == ListViewMediaSource.QueueList)
                {
                    PlaySongQueueVersion(song);
                }
                else
                {
                    PlaySelection(song);
                }
            }
        }

        private void mnftPlaySongNext_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song)
            {
                if (MediaSource == ListViewMediaSource.QueueList)
                {
                    PlaySongNextQueueVersion(song);
                }
                else
                {
                    var item = QueueService.VusicQueueNext.FirstOrDefault(p => p.FilePath == song.FilePath);
                    if (item != null && item.FilePath != null)
                    {
                        QueueService.VusicQueueNext.Remove(item);
                        QueueService.VusicQueueNext.Insert(0, item);
                    }
                    else if (item == null)
                    {
                        QueueService.VusicQueueNext.Insert(0, song);
                    }
                }
            }
        }
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        private void mnftRemoveSong_Click(object sender, RoutedEventArgs e)
        {
            if (MediaSource == ListViewMediaSource.QueueList)
            {
                if (sender is FrameworkElement element && element.DataContext is SongModel song)
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
            else if (MediaSource == ListViewMediaSource.AlbumEditor)
            {
                if (sender is FrameworkElement element && element.DataContext is SongModel song)
                {
                    try
                    {
                        string newAlbumName = "";


                        if (song.FilePath != null)
                        {
                            var filelocked = GetLockingProcess.GetLockingProcesses(song.FilePath);
                            if (filelocked.Count == 0)
                            {
                                var file = TagLib.File.Create(song.FilePath);
                                file.Tag.Album = newAlbumName;
                                file.Save();
                                file.Dispose(); // Important: Release TagLib handle
                                song.AlbumName = newAlbumName;
                                ItemsSource.Remove(song);

                            }
                            else
                            {
                                var processNames = string.Join(", ", filelocked.Select(p => p.ProcessName));
                                if (App.MainWindowInstance == null) return;


                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, "ArtistPage.RenameArtist", Logger.LogLevelType.Error);
                    }
                }
            }
            else if (MediaSource == ListViewMediaSource.Artist)
            {
                Debug.WriteLine("shsh");
                if (sender is FrameworkElement element && element.DataContext is SongModel song)
                {
                    try
                    {

                        var stringart = "";
                        if (song.FilePath != null)
                        {
                            var filelocked = GetLockingProcess.GetLockingProcesses(song.FilePath);
                            if (filelocked.Count == 0)
                            {
                                var file = TagLib.File.Create(song.FilePath);
                                file.Tag.AlbumArtists = [stringart];
                                file.Save();
                                file.Dispose();
                                ItemsSource.Remove(song);

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

                                        if (PlayerService.Masterplayer.Status == Status.Playing)
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
                                        var filelocked2 = GetLockingProcess.GetLockingProcesses(song.FilePath);
                                        if (filelocked2.Count == 0)
                                        {
                                            try
                                            {
                                                var file = TagLib.File.Create(song.FilePath);
                                                file.Tag.AlbumArtists = [stringart];
                                                file.Save();
                                                file.Dispose();
                                                ItemsSource.Remove(song);
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
                        Logger.Log(ex.Message, "ArtistPage.RemoveItem", Logger.LogLevelType.Error);
                    }
                }

            }
            else if (MediaSource == ListViewMediaSource.Album)
            {
                if (sender is FrameworkElement element && element.DataContext is SongModel song)
                {
                    try
                    {

                        var stringart = "";
                        if (song.FilePath != null)
                        {
                            var filelocked = GetLockingProcess.GetLockingProcesses(song.FilePath);
                            if (filelocked.Count == 0)
                            {
                                var file = TagLib.File.Create(song.FilePath);
                                file.Tag.Album = stringart;
                                file.Save();
                                file.Dispose();
                                ItemsSource.Remove(song);

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

                                        if (PlayerService.Masterplayer.Status == Status.Playing)
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
                                        var filelocked2 = GetLockingProcess.GetLockingProcesses(song.FilePath);
                                        if (filelocked2.Count == 0)
                                        {
                                            try
                                            {
                                                var file = TagLib.File.Create(song.FilePath);
                                                file.Tag.Album = stringart;
                                                file.Save();
                                                file.Dispose();
                                                ItemsSource.Remove(song);
                                                if (isPaused2 == false)
                                                {

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
                                    OceanContentDialog.Show("Error", "Skip", "", "", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 550, 300, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"Unable to remove item because it is in use by {processNames}", "error");
                                    OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
                                    OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, "AlbumPage.RemoveItem", Logger.LogLevelType.Error);
                    }
                }

            }
            else
            {
                if (sender is FrameworkElement element && element.DataContext is SongModel song)
                {
                    ItemRemoved?.Invoke(song);

                    ItemsSource.Remove(song);
                }
            }
        }
        bool isPaused2 = false;
        public static MediaPlaybackController media => MediaPlaybackController.Instance;
        public event Action<SongModel>? ItemRemoved;
        private static long curtimetemp;
        private static TimeSpan curtime;

        private void OceanContentDialog_PrimaryRequested()
        {
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        private void mnftSongDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song && song.FilePath is string filepath)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(filepath);
                }
            }
        }

        private void mnftAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song)
            {
                QueueService.VusicQueue.Add(song);
                QueueService.VusicQueueNext.Add(song);
            }
        }
        private void mnftGoToArtist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song)
            {
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(ArtistView), song.Artist);
            }
        }

        private void mnftGoToAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song)
            {
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(AlbumView), song.AlbumName);
            }
        }

        private async void mnftAddToFavourites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is SongModel song)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                var pathtocheck = song.FilePath;
                if (pathtocheck == null) return;
                var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);
                if (existing != null)
                {
                    song.IsFavourite = false;
                    Favourites.Remove(existing);
                    song.FavString = "Add to Favourites";

                }
                else
                {
                    Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                    song.IsFavourite = true;
                    song.FavString = "Remove from Favourites";

                }


                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }

        private SongModel? GetSelectedSong(object sender)
        {
            // Helper to get SongModel from sender
            return (sender as MenuFlyoutItem)?.DataContext as SongModel;
        }

        private void mnftMoveup_Click(object sender, RoutedEventArgs e)
        {
            var song = GetSelectedSong(sender);
            if (song == null) return;
            int index = ItemsSource.IndexOf(song);
            if (index <= 0) return;
            ItemsSource.Move(index, index - 1);
        }

        private void mnftMovedown_Click(object sender, RoutedEventArgs e)
        {
            var song = GetSelectedSong(sender);
            if (song == null) return;

            int index = ItemsSource.IndexOf(song);
            if (index >= ItemsSource.Count - 1) return;

            ItemsSource.Move(index, index + 1);
        }

        private void mnftMovetotop_Click(object sender, RoutedEventArgs e)
        {
            var song = GetSelectedSong(sender);
            if (song == null) return;

            int index = ItemsSource.IndexOf(song);
            if (index <= 0) return;


            ItemsSource.Move(index, 0);



        }

        private void mnftMovetobottom_Click(object sender, RoutedEventArgs e)
        {
            var song = GetSelectedSong(sender);
            if (song == null) return;

            int index = ItemsSource.IndexOf(song);
            if (index == ItemsSource.Count - 1) return;

            ItemsSource.Move(index, ItemsSource.Count - 1);
        }

        private void btnGlyph_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hyp && hyp.DataContext is SongModel song)
            {
                if (MediaSource == ListViewMediaSource.MassEditor || MediaSource == ListViewMediaSource.AlbumEditor)
                {
                    return;
                }

                if (song.Glyph == "\uE768")
                {
                    PlayerService.Play();
                }
                else if (song.Glyph == "\uE769")
                {
                    PlayerService.Pause();
                }

            }
        }
        private void PlaySongNextQueueVersion(SongModel song)
        {
            var item = QueueService.VusicQueueNext.FirstOrDefault(p => p.FilePath == song.FilePath);
            if (item != null && item.FilePath != null)
            {
                QueueService.VusicQueueNext.Remove(item);
                QueueService.VusicQueueNext.Insert(0, item);
            }
            else if (item == null)
            {
                QueueService.VusicQueueNext.Insert(0, song);
            }
        }
        private void PlaySongQueueVersion(SongModel song)
        {
            var selectedSong = song;
            if (selectedSong == null || string.IsNullOrEmpty(selectedSong.FilePath)) return;

            try
            {
                // 1. FREEZE HANDLERS: Lock mutations to prevent layout recalculation mid-click
                QueueService.IsLooping = true;

                // 2. Find where the clicked song sits in the master list
                int targetIndex = QueueService.VusicQueue.IndexOf(selectedSong);
                if (targetIndex < 0) return;

                // 3. Update the state of EVERY song based on its new position relative to the clicked song
                for (int i = 0; i < QueueService.VusicQueue.Count; i++)
                {
                    var track = QueueService.VusicQueue[i];

                    if (i < targetIndex)
                    {
                        // Everything before the clicked song becomes historical past
                        track.IsCompleted = true;
                        track.VisibilityOfStrikeThrough = Visibility.Visible;
                        track.QueueControls = Visibility.Collapsed;
                    }
                    else if (i == targetIndex)
                    {
                        // The clicked song is now active playing
                        track.IsCompleted = false;
                        track.VisibilityOfStrikeThrough = Visibility.Collapsed;
                        track.QueueControls = Visibility.Collapsed;
                    }
                    else
                    {
                        // Everything after the clicked song becomes clean upcoming future tracks
                        track.IsCompleted = false;
                        track.VisibilityOfStrikeThrough = Visibility.Collapsed;
                        track.QueueControls = Visibility.Collapsed;
                    }
                }

                // 4. Update VusicQueueNext and Shuffle Backups cleanly using structural updates
                QueueService.VusicQueueNext.Clear();
                QueueService.OriginalVusicQueueNext.Clear();

                for (int i = targetIndex + 1; i < QueueService.VusicQueue.Count; i++)
                {
                    QueueService.VusicQueueNext.Add(QueueService.VusicQueue[i]);
                    QueueService.OriginalVusicQueueNext.Add(QueueService.VusicQueue[i]);
                }

                // 5. If shuffle is currently active, randomize the newly created upcoming layout right away!
                if (QueueService.IsShuffleTrue)
                {
                    // Re-shuffle just the new upcoming pool
                    var shuffledList = QueueService.ShuffleItems(QueueService.VusicQueueNext.ToList());
                    QueueService.VusicQueueNext.Clear();
                    foreach (var item in shuffledList)
                    {
                        QueueService.VusicQueueNext.Add(item);
                    }
                }

                // 6. Set active file path and open stream execution
                PlayerService.CurrentPlayingPath = selectedSong.FilePath;
                PlayerService.OpenPath(selectedSong.FilePath);
            }
            finally
            {
                // 7. Unblock and unleash synchronization
                QueueService.IsLooping = false;
            }

            // 8. Force complete master UI consistency
            QueueService.SyncFullQueueFromNext();
        }
        private void hypTitle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hyperlink && hyperlink.DataContext is SongModel song)
            {
                if (MediaSource == ListViewMediaSource.QueueList)
                {
                    PlaySongQueueVersion(song);
                }
                else
                {
                    PlaySelection(song);
                }
            }
        }

        private void hypArtist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton mnft && mnft.DataContext is SongModel song)
            {
                if (MediaSource == ListViewMediaSource.Artist || MediaSource == ListViewMediaSource.ArtistNoRearrange) return;
                if (App.NavigationFrame == null) return;
                App.NavigationFrame.Navigate(typeof(ArtistView), song.Artist);
            }
        }

        private void hypAlbum_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton mnft && mnft.DataContext is SongModel song && song.AlbumName is string str)
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

        private async void btnFavourite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is Grid rootGrid && btn.DataContext is SongModel song)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                var pathtocheck = song.FilePath;
                if (pathtocheck == null) return;
                var fillHeartIcon = rootGrid.FindName("HeartIcon") as FontIcon;
                if (fillHeartIcon == null) return;
                var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);
                if (existing == null)
                {
                    fillHeartIcon.Glyph = "\uEB52";
                    song.IsFavourite = true;
                    AnimateHeartFull(fillHeartIcon, true);
                    Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                    song.FavString = "Remove from Favourites";

                }
                else
                {
                    song.IsFavourite = false;
                    fillHeartIcon.Glyph = "\uEB51";
                    AnimateHeartFull(fillHeartIcon, false);
                    Favourites.Remove(existing);
                    song.FavString = "Add to Favourites";


                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);

            }
            // Favourite button click logic
        }
        private void AnimateHeartFull(FontIcon targetIcon, bool isBecomingFavorite)
        {
            // 1. Get the Visual and Compositor
            var visual = ElementCompositionPreview.GetElementVisual(targetIcon);
            var compositor = visual.Compositor;

            // 2. Setup Scale Animation (The "Pop")
            var scaleAnim = compositor.CreateScalarKeyFrameAnimation();
            scaleAnim.InsertKeyFrame(0.0f, 1.0f);
            scaleAnim.InsertKeyFrame(0.5f, 1.4f); // Pulse size
            scaleAnim.InsertKeyFrame(1.0f, 1.0f);
            scaleAnim.Duration = TimeSpan.FromMilliseconds(400);

            visual.CenterPoint = new Vector3((float)targetIcon.ActualWidth / 2, (float)targetIcon.ActualHeight / 2, 0);
            visual.StartAnimation("Scale.X", scaleAnim);
            visual.StartAnimation("Scale.Y", scaleAnim);

            // 3. Setup Color Animation (The "Fill")
            // Note: We animate the 'Brush.Color' of the visual
            var colorAnim = compositor.CreateColorKeyFrameAnimation();
            colorAnim.Duration = TimeSpan.FromMilliseconds(400);

            if (isBecomingFavorite)
            {
                colorAnim.InsertKeyFrame(0.0f, Colors.Gray);
                colorAnim.InsertKeyFrame(1.0f, Colors.Red);
            }
            else
            {
                colorAnim.InsertKeyFrame(0.0f, Colors.Red);
                colorAnim.InsertKeyFrame(1.0f, Colors.Gray);
            }

            // Create a Brush if one doesn't exist on the visual layer
            var brush = compositor.CreateColorBrush();
            visual.Properties.InsertColor("Color", isBecomingFavorite ? Colors.Red : Colors.Gray);

            // Start the color transition
            // Note: For FontIcon, it's often easier to just swap the Glyph 
            // and let the Composition color animation handle the tint.
            targetIcon.Foreground = new SolidColorBrush(isBecomingFavorite ? Colors.Red : Colors.Gray);
        }
        private void TriggerHeartAnimation(UIElement target)
        {
            var visual = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(target);
            var compositor = visual.Compositor;

            // Create a Scale animation
            var scaleAnimation = compositor.CreateScalarKeyFrameAnimation();
            scaleAnimation.InsertKeyFrame(0.0f, 1.0f);   // Start size
            scaleAnimation.InsertKeyFrame(0.5f, 1.5f);   // Pop up (Pulse)
            scaleAnimation.InsertKeyFrame(1.0f, 1.0f);   // Back to normal
            scaleAnimation.Duration = TimeSpan.FromMilliseconds(300);
            scaleAnimation.IterationCount = 1;

            // Set the center point for the scale (so it scales from the middle)
            visual.CenterPoint = new System.Numerics.Vector3((float)target.RenderSize.Width / 2, (float)target.RenderSize.Height / 2, 0);

            // Start the animation on the Scale properties
            visual.StartAnimation("Scale.X", scaleAnimation);
            visual.StartAnimation("Scale.Y", scaleAnimation);
        }
        private void btnRemoveSelectionsConfirm_Click(object sender, RoutedEventArgs e)
        {
            var selected2 = lstViewPlaylist.SelectedItems.Cast<SongModel>().ToList();
            if (isViewList == false)
            {
                selected2 = grdViewMain.SelectedItems.Cast<SongModel>().ToList();
            }
            foreach (var item in selected2)
            {
                ItemsSource.Remove(item);

            }
            flyoutDelete.Hide();
            ListViewRemoved?.Invoke(sender, e);
        }

        private void ShowEditOptionsForMultiple()
        {
            // Helper for mass editing
        }

        private void btnEditAlbumMass_Click(object sender, RoutedEventArgs e)
        {

            var selected = lstViewPlaylist.SelectedItems.Cast<SongModel>().ToList();
            if (isViewList == false)
            {
                selected = grdViewMain.SelectedItems.Cast<SongModel>().ToList();
                return;
            }
            var observable = new ObservableCollection<SongModel>();
            foreach (var item in selected)
            {
                observable.Add(item);
            }
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Edit Properties for Multiple", "Close", "", "", OceanDialogWindow.ContentType.MassEditing, OceanContentDialogDefault.Primary, XamlRoot, 950, 900, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", observable, "", "", "", "", "");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested1;
        }

        private void OceanContentDialog_PrimaryRequested1()
        {
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }


        public void SelectAll()
        {
            if (isViewList)
            {
                lstViewPlaylist.SelectAll();
            }
            else
            {
                grdViewMain.SelectAll();
            }
        }
        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAll();
        }

        private void btnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            if (isViewList)
            {
                lstViewPlaylist.DeselectAll();
            }
            else
            {
                grdViewMain.DeselectAll();
                chckSelectGridView.IsChecked = false;
            }
        }

        private async void btnRemoveSelectionsFromFavourites_Click(object sender, RoutedEventArgs e)
        {
            var songss = SelectedItems.Cast<SongModel>().ToList(); // Converting to a list is safer if you evaluate multiple times

            bool allAreFavorites = songss.All(item => item.IsFavourite);

            bool noneAreFavorites = !songss.Any(item => item.IsFavourite);

            bool partialFavorites = songss.Any(item => item.IsFavourite) && !songss.All(item => item.IsFavourite);
            if (songss.Any(item => item.IsFavourite))
            {
                Debug.WriteLine("All/Partial");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                foreach (var item in songss)
                {
                    var pathtocheck = item.FilePath;
                    if (pathtocheck == null) return;
                    var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);

                    if (existing != null)
                    {
                        item.IsFavourite = false;
                        item.FavString = "Add to Favourites";
                        Favourites.Remove(existing);
                    }

                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            else if (noneAreFavorites)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                foreach (var item in songss)
                {
                    var pathtocheck = item.FilePath;
                    if (pathtocheck == null) return;
                    var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);

                    if (existing == null)
                    {
                        item.IsFavourite = true;
                        item.FavString = "Remove from Favourites";

                        Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                    }

                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }

        private void mnftSelectitem_Click(object sender, RoutedEventArgs e)
        {
            // Select item from flyout logic
        }

        private void mnftUnselectItem_Click(object sender, RoutedEventArgs e)
        {
            // Unselect item from flyout logic
        }

        private void mnftSetAlbumName_Click(object sender, RoutedEventArgs e)
        {
            // Set album name logic
        }

        private void mnftSetArtistName_Click(object sender, RoutedEventArgs e)
        {
            // Set artist name logic
        }

        private void btnFixFile_Click(object sender, RoutedEventArgs e)
        {
            // File fix logic
        }

        private async void tbViewEdit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Edit view selection changed logic
        }

        private void btnCreateNewPlaylistUnderEdit_Click(object sender, RoutedEventArgs e)
        {
            // Create new playlist from edit view logic
        }



        private void btnSortItems_Click(object sender, RoutedEventArgs e)
        {
            // Sort button logic
        }

        private void mnftSortName_Click(object sender, RoutedEventArgs e)
        {
            var sorted = this.ItemsSource.OrderBy(p => p.Title).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = this.ItemsSource.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    this.ItemsSource.Move(oldIndex, newIndex);
                }
            }
        }

        private void mnftSortDuration_Click(object sender, RoutedEventArgs e)
        {
            var sorted = this.ItemsSource.OrderBy(p => p.SongDuration).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = this.ItemsSource.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    this.ItemsSource.Move(oldIndex, newIndex);
                }
            }
        }

        private void mnftSortbyArtist_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Sort by artist");
            var sorted = this.ItemsSource.OrderBy(p => p.Artist).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = this.ItemsSource.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    this.ItemsSource.Move(oldIndex, newIndex);
                }
            }
        }

        private void mnftSortbyAlbum_Click(object sender, RoutedEventArgs e)
        {

            var sorted = this.ItemsSource.OrderBy(p => p.AlbumName).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = this.ItemsSource.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    this.ItemsSource.Move(oldIndex, newIndex);
                }
            }
        }
        private void mnftSortbyDateMod_Click(object sender, RoutedEventArgs e)
        {
            foreach (var song in ItemsSource)
            {
                if (song != null && song.FilePath != null)
                {
                    song.DateModified = File.GetLastWriteTime(song.FilePath);
                }

            }
            var sorted = this.ItemsSource.OrderBy(p => p.DateModified).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = this.ItemsSource.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    this.ItemsSource.Move(oldIndex, newIndex);
                }
            }
        }

        private void mnftSortbyDateCreated_Click(object sender, RoutedEventArgs e)
        {
            foreach (var song in ItemsSource)
            {
                if (song != null && song.FilePath != null)
                {
                    song.DateCreated = File.GetCreationTime(song.FilePath);
                }

            }
            var sorted = this.ItemsSource.OrderBy(p => p.DateCreated).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = this.ItemsSource.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    this.ItemsSource.Move(oldIndex, newIndex);
                }
            }

        }

        private void hypPlaylistAdded_Click(object sender, RoutedEventArgs e)
        {
            if (hypPlaylistAdded.Tag is PlaylistItem playlistItem)
            {
                if (App.NavigationFrame != null)
                    App.NavigationFrame.Navigate(typeof(PlaylistView), playlistItem);
            }
        }
        private IEnumerable<SongModel> GetFilteredResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<SongModel>();

            var rawQuery = query.Trim();

            var minMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:min|m)", RegexOptions.IgnoreCase);
            var secMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:sec|s)", RegexOptions.IgnoreCase);

            int searchSeconds = 0;
            if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
            if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);

            var textQuery = rawQuery;
            if (minMatch.Success) textQuery = textQuery.Replace(minMatch.Value, "");
            if (secMatch.Success) textQuery = textQuery.Replace(secMatch.Value, "");
            textQuery = textQuery.Trim();

            return ItemsSource.Where(s =>
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
            .ThenBy(s => s.Title);
        }
        ObservableCollection<SongModel> searchresults = new();
        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

            if (string.IsNullOrEmpty(sender.Text))
            {
                searchresults.Clear();
                grdNoSearchResults.Visibility = Visibility.Collapsed;
                if (isViewList == true)
                {
                    lstViewPlaylist.ItemsSource = ItemsSource;
                    lstViewPlaylist.Visibility = Visibility.Visible;
                }
                else
                {
                    Debug.WriteLine("GridView");
                    grdViewMain.ItemsSource = ItemsSource;
                    grdViewMain.Visibility = Visibility.Visible;

                }
                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = GetFilteredResults(sender.Text);

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);

                sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };
                if (isViewList == true)
                {
                    lstViewPlaylist.ItemsSource = searchresults;
                }
                else
                {
                    Debug.WriteLine("searching in gridview");
                    grdViewMain.ItemsSource = searchresults;

                }
            }
        }

        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var results = GetFilteredResults(sender.Text);

            if (results.Any())
            {
                grdNoSearchResults.Visibility = Visibility.Collapsed;
                if (isViewList == true)
                {
                    lstViewPlaylist.Visibility = Visibility.Visible;
                    Grid.SetRow(grdNoSearchResults, 2);
                }
                else
                {
                    Grid.SetRow(grdNoSearchResults, 3);

                    grdViewMain.Visibility = Visibility.Visible;
                }

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);
            }
            else if (ItemsSource.Count > 0)
            {
                if (isViewList == true)
                {
                    Grid.SetRow(grdNoSearchResults, 2);

                    lstViewPlaylist.Visibility = Visibility.Collapsed;
                }
                else
                {
                    Grid.SetRow(grdNoSearchResults, 3);

                    grdViewMain.Visibility = Visibility.Collapsed;
                }
                grdNoSearchResults.Visibility = Visibility.Visible;
                frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
            }
        }

        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            asbSearch.Text = "";
            lstViewPlaylist.Focus(FocusState.Programmatic);
            asbSearch.ItemsSource = null;
        }

        private void mnftViewList_Click(object sender, RoutedEventArgs e)
        {
            isViewList = true;
            grdViewMain.Visibility = Visibility.Collapsed;
            lstViewPlaylist.Visibility = Visibility.Visible;
            chckSelectGridView.Visibility = Visibility.Collapsed;
            btnEditAlbumMass.Visibility = Visibility.Visible;
            btnEditArtistMass.Visibility = Visibility.Visible;
            lstViewPlaylist.DeselectAll();
            if (grdViewMain.SelectedItems.Count != 0)
            {
                foreach (var item in grdViewMain.SelectedItems)

                {
                    if (item is SongModel song)
                    {
                        // Assuming 'ItemsSource' is the collection bound to grdViewMain
                        int index = ItemsSource.IndexOf(song);

                        if (index >= 0)
                        {
                            // SelectRange(FirstIndex, Length)
                            lstViewPlaylist.SelectRange(new ItemIndexRange(index, 1));
                        }
                    }
                }
            }

        }
        bool isViewList = true;
        private async void ViewGrid()
        {
            if (ItemsSource == null) return;

            chckSelectGridView.Visibility = Visibility.Visible;
            isViewList = false;
            mnftViewIcons.IsChecked = true;
            grdViewMain.Visibility = Visibility.Visible;
            lstViewPlaylist.Visibility = Visibility.Collapsed;

            grdViewMain.DeselectAll();
            if (lstViewPlaylist.SelectedItems.Count != 0)
            {
                foreach (var item in lstViewPlaylist.SelectedItems)

                {
                    if (item is SongModel song)
                    {
                        // Assuming 'ItemsSource' is the collection bound to grdViewMain
                        int index = ItemsSource.IndexOf(song);

                        if (index >= 0)
                        {
                            // SelectRange(FirstIndex, Length)
                            grdViewMain.SelectRange(new ItemIndexRange(index, 1));
                        }
                    }
                }
            }
            foreach (var item in ItemsSource.ToList())
            {
                if (item.FilePath is string str)
                    item.VideoThumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(str);
            }
        }
        private void mnftViewIcons_Click(object sender, RoutedEventArgs e)
        {
            ViewGrid();
        }
        public void VideoPlaylistUI()
        {
            ViewGrid();
        }
        private void chckSelectGridView_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = chckSelectGridView.IsChecked ?? false;

            grdViewMain.SelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
        }

        private void grdViewMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void btnAddtoPlaylistMass_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var playlitss = currentSettings.SavedPlaylists;
            lstViewPlaylists.Items.Clear();
            int count = SelectedItems.Count;
            txtItemsCountAddtoPl.Text = $"• {count} {(count == 1 ? "item" : "items")} selected";

            foreach (var item in playlitss)
            {
                lstViewPlaylists.Items.Add(item.PlaylistName);
            }
            ttAddToPlaylist.IsOpen = true;
        }

        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selected = SelectedItems.Cast<SongModel>().ToList();
                var selectedplaylists = lstViewPlaylists.SelectedItems.ToList();
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var playlists = currentSettings.SavedPlaylists;
                foreach (var item in selected)
                {
                    if (item != null && item.FilePath is string str)
                    {
                        if (File.Exists(str))
                        {
                            foreach (var playlist in selectedplaylists)
                            {
                                var exist = playlists.FirstOrDefault(p => p.PlaylistName == playlist.ToString());
                                if (exist != null)
                                {
                                    var songspaths = exist.SongsPaths;
                                    var defaultitem = songspaths.FirstOrDefault(k => k == item.FilePath);
                                    if (defaultitem == null)
                                    {
                                        exist.SongsPaths.Add(item.FilePath);
                                        exist.PlaylistCount = $"{exist.SongsPaths.Count} {(exist.SongsPaths.Count == 1 ? "item" : "items")}";
                                    }
                                }
                            }
                        }
                    }
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, ex.Message, Logger.LogLevelType.Error);
                ttError.IsOpen = true;
                txtError.Text = "An unexpected error occured. Please check log page for details";
            }
            finally
            {
                ttAddToPlaylist.IsOpen = false;
                ttAddedToPlaylist.IsOpen = true;
                await Task.Delay(2000);
                ttAddedToPlaylist.IsOpen = false;
            }
        }

        private void lstViewPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstViewPlaylists.SelectedItems.Count == 0)
            {
                btnAddFinalPlaylists.IsEnabled = false;
            }
            else
            {
                btnAddFinalPlaylists.IsEnabled = true;
            }

        }

        private void mnftSortbyVideosFirst_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftSortbyAudiosFirst_Click(object sender, RoutedEventArgs e)
        {

        }
        bool isVideoFirst = false;
        private async void mnftSortbyMediaType_Click(object sender, RoutedEventArgs e)
        {
            var observablevideos = new ObservableCollection<SongModel>();
            var observableaudios = new ObservableCollection<SongModel>();
            foreach (var item in ItemsSource)
            {
                var file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                string fileExtension = file.FileType.ToLowerInvariant();

                if (Extensions.VideoExtensions.List.Contains(fileExtension))
                {
                    observablevideos.Add(item);
                }
                else
                {
                    observableaudios.Add(item);
                }
            }
            if (isVideoFirst == true)
            {

                isVideoFirst = false;
                ItemsSource.Clear();
                foreach (var item in observablevideos)
                {
                    ItemsSource.Add(item);
                }
                foreach (var item in observableaudios)
                {
                    ItemsSource.Add(item);
                }
            }
            else
            {
                isVideoFirst = true;
                ItemsSource.Clear();
                foreach (var item in observableaudios)
                {
                    ItemsSource.Add(item);
                }
                foreach (var item in observablevideos)
                {
                    ItemsSource.Add(item);
                }

            }
        }

        private void mnftRenameFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftDeleteFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftPreviewFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftMoveFile_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftRemoveSongGenre_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

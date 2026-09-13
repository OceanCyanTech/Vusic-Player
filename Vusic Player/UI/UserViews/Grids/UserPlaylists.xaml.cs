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
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Linq;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.UI.UserViews.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids
{
    public class CountToVisibility : IValueConverter
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
    public class CountToVisibilityReverse : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class UserPlaylists : UserControl
    {
        public ObservableCollection<PlaylistItem> Playlists { get; set; } = new();
        PlaylistCreationValues Instance => PlaylistCreationValues.Instance;

        public bool IsItemClickDisabled
        {
            get => (bool)GetValue(itemclickdisable);
            set => SetValue(itemclickdisable, value);
        }
        public static readonly DependencyProperty itemclickdisable =
    DependencyProperty.Register(
        nameof(IsItemClickDisabled),
        typeof(bool),
        typeof(UserPlaylists),
        new PropertyMetadata(false));

        public Visibility openVisibility
        {
            get => (Visibility)GetValue(openvisiblity);
            set => SetValue(openvisiblity, value);
        }
        public static readonly DependencyProperty openvisiblity =
    DependencyProperty.Register(
        nameof(openVisibility),
        typeof(Visibility),
        typeof(ListViewMedia),
        new PropertyMetadata(Visibility.Visible));
        public UserPlaylists()
        {
            InitializeComponent();

            //LoadPlaylists();
            //PlaylistCreation.CreationCallAdd -= PlaylistCreation_CreationCallAdd;
            //PlaylistCreation.CreationCallAdd += PlaylistCreation_CreationCallAdd;
        }
        private bool _isLoadingData = false;

        private async void LoadPlaylists()
        {
            if (_isLoadingData) return;
            _isLoadingData = true;
            try
            {
                Playlists.Clear();
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                foreach (var playlist in currentSettings.SavedPlaylists)
                {

                    Playlists.Add(playlist);

                }
                grdViewPlaylists.ItemsSource = Playlists;
                Playlists.CollectionChanged += Playlists_CollectionChanged;
                UpdateUI();
            }
            finally
            {
                _isLoadingData = false;
            }
        }

        private async void Playlists_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isLoadingData || _isSavingPlaylist) return;
            if (e.Action == NotifyCollectionChangedAction.Remove ||
            e.Action == NotifyCollectionChangedAction.Add ||
            e.Action == NotifyCollectionChangedAction.Move)
            {
                Debug.WriteLine("Moved");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                currentSettings.SavedPlaylists = Playlists;
                MasterSearchIndex.PlaylistsMaster = Playlists;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                UpdateUI();
            }
        }

        private bool _isSavingPlaylist = false;

        private async void PlaylistCreation_CreationCallAdd()
        {
            if (_isSavingPlaylist) return;
            _isSavingPlaylist = true;
            try
            {
                if (PlaylistCreation.playlistItem != null)
                {
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    if (PlaylistCreation.playlistItem.PlaylistName is string name)
                    {
                        string baseName = name.Trim();

                        if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                        string finalName = baseName;
                        int counter = 1;
                        while (currentSettings.SavedPlaylists.Any(p =>
                            string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                        {
                            finalName = $"{baseName} ({counter++})";
                        }
                        PlaylistCreation.playlistItem.PlaylistName = finalName;
                    }
                    Playlists.Add(PlaylistCreation.playlistItem);

                    currentSettings.SavedPlaylists.Add(PlaylistCreation.playlistItem);
                    MasterSearchIndex.PlaylistsMaster.Add(PlaylistCreation.playlistItem);
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                }
                UpdateUI();
            }
            finally
            {
                _isSavingPlaylist = false;
            }
        }
        private void UpdateUI()
        {
            if (Playlists.Count == 0)
            {
                grdRecents.Visibility = Visibility.Collapsed;
                grdEmptySuggestions.Visibility = Visibility.Visible;
            }
            else
            {
                grdRecents.Visibility = Visibility.Visible;
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
            }
        }
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {

        }





        private void chkSelect_Toggled(object sender, RoutedEventArgs e)
        {
            bool isChecked = chkSelect.IsChecked ?? false;

            grdViewPlaylists.SelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAll.IsChecked == true)
                grdViewPlaylists.SelectAll();
            else
                grdViewPlaylists.DeselectAll();
        }

     

        public event RoutedEventHandler? OpenPlaylistClick;
        private void mnftOpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            OpenPlaylistClick?.Invoke(sender, e);
        }

        private async void mnftPlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is PlaylistItem playlist)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in playlist.SongsPaths)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, false, false);
            }
        }

        private async void mnftShufflePlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is PlaylistItem playlist)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in playlist.SongsPaths)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, true, false);
            }
        }

        private async void mnftEditPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem playlist)
            {
                Instance.MediaPaths.Clear();
                Instance.MediaSongModels.Clear();
                ObservableCollection<SongModel> songmodels = new ObservableCollection<SongModel>();
                foreach (var song in playlist.SongsPaths)
                {
                    var file = await StorageFile.GetFileFromPathAsync(song);
                    var musicprops = await file.Properties.GetMusicPropertiesAsync();
                    songmodels.Add(new SongModel { Title = string.IsNullOrEmpty(musicprops.Title) ? Path.GetFileNameWithoutExtension(song): musicprops.Title, SongDuration = musicprops.Duration, FilePath = song });
                    Instance.MediaPaths.Add(song);
                }
                ttEditPlaylist.IsOpen = true;
                Instance.PlCover = new BitmapImage(playlist.Thumbnail);
                Instance.Thumbnail = playlist.Thumbnail ??new Uri("ms - appx:///Assets/playlistdefaultdark.png");

                Instance.PlaylistName = playlist.PlaylistName;
                Instance.Genre = playlist.PlaylistGenre ?? "";
                Instance.MediaSongModels = new ObservableCollection<SongModel>(songmodels);
                Instance.IsAppInstanceOceanDialog = false;

                btnSavePlaylistEdited.Click += (async (object sender, RoutedEventArgs e) =>
                {
                    Debug.WriteLine("SAVE SHOW CALLED");
                    if (Instance.PlaylistName == "")
                    {
                        Instance.PlaylistName = playlist.PlaylistName;
                    }
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    string baseName = Instance.PlaylistName.Trim();

                    string finalName = baseName;

                    if (Instance.PlaylistName.Trim() != playlist.PlaylistName.Trim())
                    {

                        if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                        int counter = 1;
                        while (currentSettings.SavedPlaylists.Any(p =>
                            string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                        {
                            finalName = $"{baseName} ({counter++})";
                        }
                    }
                    playlist.PlaylistName = finalName;
                    playlist.PlaylistGenre = Instance.Genre;
                    playlist.PlaylistCount = Instance.PlaylistCount;
                    playlist.plthumb = Instance.PlCover;
                    playlist.Thumbnail = Instance.Thumbnail;
                    playlist.ThumbnailString = Instance.ThumbnailString;
                    playlist.SongsPaths = Instance.MediaPaths;
                    var existingplaylist = currentSettings.SavedPlaylists.FirstOrDefault(p => p.PlaylistId == playlist.PlaylistId);
                    if (existingplaylist != null)
                    {
                        existingplaylist.PlaylistName = finalName;
                        existingplaylist.PlaylistGenre = Instance.Genre;
                        existingplaylist.PlaylistCount = Instance.PlaylistCount;
                        existingplaylist.plthumb = Instance.PlCover;
                        existingplaylist.Thumbnail = Instance.Thumbnail;
                        existingplaylist.ThumbnailString = Instance.ThumbnailString;
                        existingplaylist.SongsPaths = Instance.MediaPaths;
                        await SettingsLoader.SaveSettingsAsync(currentSettings);
                        ttEditPlaylist.IsOpen = false;
                        Instance.IsAppInstanceOceanDialog = true;

                    }
                });
            }

        }

        private void mnftDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem playlist)
            {
                if (App.MainWindowInstance == null) return;
                OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "deleteicon", "", "", new ObservableCollection<SongModel>(), "", $"Are you sure you want to delete the playlist '{playlist.PlaylistName}'? This cannot be undone.", "warning");
                //OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
                OceanContentDialog.PrimaryRequested += (async () =>
                {
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                    Instance.PlaylistsMaster.Remove(playlist);
                    ttEditPlaylist.IsOpen = false;
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var settingToRemove = currentSettings.SavedPlaylists.FirstOrDefault(p => p.PlaylistId == playlist.PlaylistId);
                    if (settingToRemove != null)
                    {
                        currentSettings.SavedPlaylists.Remove(settingToRemove);
                        await SettingsLoader.SaveSettingsAsync(currentSettings); // Save to disk immediately
                    }
                    //       UpdateUI();

                });
            }
        }
        public event ItemClickEventHandler? GridViewItemClick;
        private void grdViewPlaylists_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (chkSelect.IsChecked == false && IsItemClickDisabled == false)
            {
                GridViewItemClick?.Invoke(sender, e);
            }
        }


        private void MenuFlyout_Opened(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            if (flyout == null) return;
            var item = flyout.Items.FirstOrDefault(x => (x as MenuFlyoutItem)?.Text == "Open Playlist");

            if (item != null)
            {
                if (openVisibility == Visibility.Collapsed)
                {
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void btnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ttEditPlaylist.IsOpen = false;
        }
        bool iscreating = false;
        private async void btnNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Playlist", "Create", "", "Cancel", OceanDialogWindow.ContentType.PlaylistCreation, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "Playlist", "", "", "", "", new PlaylistItem(), false, false);
            Debug.WriteLine("BTNNEWPLAYLIST");
            OceanContentDialog.PrimaryRequested += (async () =>
            {
                if (iscreating) return;
                try
                {
                    iscreating = true; 
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    string baseName = Instance.PlaylistName.Trim();

                    if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                    string finalName = baseName;
                    int counter = 1;
                    while (currentSettings.SavedPlaylists.Any(p =>
                        string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalName = $"{baseName} ({counter++})";
                    }
                    var newplaylist = new PlaylistItem { PlaylistName =finalName, PlaylistGenre = Instance.Genre, plthumb = Instance.PlCover, PlaylistId = Instance.PlaylistID, DateCreation = Instance.CreationDate, PlaylistCount = Instance.PlaylistCount, SongsPaths = new HashSet<string>(Instance.MediaPaths), Thumbnail = Instance.Thumbnail };
                    Debug.WriteLine(newplaylist.PlaylistName);
                    Instance.PlaylistsMaster.Add(newplaylist);
                 
                    currentSettings.SavedPlaylists.Add(newplaylist);
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                }
                catch (Exception ex)
                {
                    Logger.Log("An unexpected error occured: " + ex.Message, "PlaylistCreate.MusicLibPage", Logger.LogLevelType.Error);
                }
                finally
                {
                    iscreating = false;
                }
            });
        }

        private void btnRemoveMultiplePlaylists_Click(object sender, RoutedEventArgs e)
        {
            var selected = grdViewPlaylists.SelectedItems.Cast<PlaylistItem>().ToList();
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "deleteicon", "", "", new ObservableCollection<SongModel>(), "", $"Are you sure you want to delete the selected playlists? This cannot be undone.", "warning");
            OceanContentDialog.PrimaryRequested += (async () =>
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                OceanContentDialog.HideDlg();
                MainWindow.ShowWindow();

                foreach (var item in selected)
                {
                    var existingplaylist = currentSettings.SavedPlaylists.FirstOrDefault(p => p.PlaylistId == item.PlaylistId);
                    if (existingplaylist != null)
                    {
                        currentSettings.SavedPlaylists.Remove(existingplaylist);
                    }
                    Instance.PlaylistsMaster.Remove(item);
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                ttEditPlaylist.IsOpen = false;
            });
        }
    }
}

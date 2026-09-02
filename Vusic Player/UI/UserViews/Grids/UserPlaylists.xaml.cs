using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.UserViews.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class UserPlaylists : UserControl
    {
        public ObservableCollection<PlaylistItem> Playlists { get; set; } = new();
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
            LoadPlaylists();
            PlaylistCreation.CreationCallAdd -= PlaylistCreation_CreationCallAdd;
            PlaylistCreation.CreationCallAdd += PlaylistCreation_CreationCallAdd;
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

        private void btnRemoveFromContinueWatchingSelected_Click(object sender, RoutedEventArgs e)
        {
            var selected2 = grdViewPlaylists.SelectedItems.Cast<PlaylistItem>().ToList();

            foreach (var item in selected2)
            {
                Playlists.Remove(item);

            }
        }




        private void mnftAddToQueueCW_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {

        }
        public event RoutedEventHandler? OpenPlaylistClick;
        private void mnftOpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            OpenPlaylistClick?.Invoke(sender, e);
        }

        private async void mnftPlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is  PlaylistItem playlist)
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

        private void mnftEditPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem playlist)
            {
                Playlists.Remove(playlist);
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

        private void grdViewPlaylists_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

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
    }
}

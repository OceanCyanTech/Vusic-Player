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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class GridViewRecentMusic : UserControl
    {
        public GridViewRecentMusic()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public void SelectAll()
        {
            grdViewAllRecentMusic.SelectAll();
        }
        public void ClearSelection()
        {
            grdViewAllRecentMusic.DeselectAll();
        }
        public async void RemoveSelection()
        {
            var selectedItems = grdViewAllRecentMusic.SelectedItems.Cast<RecentMusicModel>().ToList();
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recents = currentSettings.RecentMusic;
            foreach (var item in selectedItems)
            {
                ItemsSource.Remove(item);
                var exist = recents.FirstOrDefault(p => p.SongPath == item.SongPath);
                if (exist != null)
                {
                    recents.Remove(exist);
                }
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);

        }
        public ListViewSelectionMode GridSelectionMode
        {
            get => (ListViewSelectionMode)GetValue(GridSelectionModeProperty);
            set => SetValue(GridSelectionModeProperty, value);
        }

        public static readonly DependencyProperty GridSelectionModeProperty =
            DependencyProperty.Register(
                nameof(GridSelectionMode),
                typeof(ListViewSelectionMode),
                typeof(GridViewRecentMusic),
                new PropertyMetadata(ListViewSelectionMode.Single)
            );
        public ObservableCollection<RecentMusicModel> ItemsSource
        {
            get => (ObservableCollection<RecentMusicModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<RecentMusicModel>),
        typeof(GridViewRecentMusic),
        new PropertyMetadata(null));

        private async void mnftRemoveFromRecentMusic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is RecentMusicModel selectedSong)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var recentmusic = currentSettings.RecentMusic;
                var defaultit = recentmusic.FirstOrDefault(p => p.SongPath == selectedSong.SongPath);
                if (defaultit == null) return;
                recentmusic.Remove(defaultit);

                await SettingsLoader.SaveSettingsAsync(currentSettings);
                ItemsSource.Remove(selectedSong);
            }
        }

        private void mnftGoToFileLocation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is RecentMusicModel selectedSong)
            {
                if (selectedSong.SongPath is string path && File.Exists(path))
                {
                    Process.Start("explorer.exe", $"/select,\"{path}\"");
                }
            }
        }

        private void mnftPlayRecents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is RecentMusicModel selectedSong)
            {
                if (File.Exists(selectedSong.SongPath))
                {
                    ObservableCollection<SongModel> single = new();
                    string Title = Path.GetFileNameWithoutExtension(selectedSong.SongPath);
                    single.Add(new SongModel { FilePath = selectedSong.SongPath, Title = Title });
                    QueueService.PlayMedia(single, false, false);
                }
            }
        }

        private async void mnftAddtoQueueRecentMusic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is RecentMusicModel selectedSong)
            {
                var exist = QueueService.VusicQueue.FirstOrDefault(p => p.FilePath == selectedSong.SongPath);
                if (exist == null)
                {
                    var file = await StorageFile.GetFileFromPathAsync(selectedSong.SongPath);
                    var props = await file.Properties.GetMusicPropertiesAsync();

                    string Title = props.Title ?? file.DisplayName;
                    if (Title == "")
                    {
                        Title = Path.GetFileNameWithoutExtension(file.Path);
                    }
                    string AlbumName = props.Album;

                    string Artist = props.Artist;
                    QueueService.VusicQueueNext.Add(new SongModel { Title = Title, AlbumName = AlbumName, Artist = Artist, SongDuration = props.Duration, FilePath = file.Path });
                    QueueService.VusicQueue.Add(new SongModel { Title = Title, AlbumName = AlbumName, Artist = Artist, SongDuration = props.Duration, FilePath = file.Path });
                }
            }
        }

        private void mnftViewFileInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is RecentMusicModel selectedSong)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
               //     wind.ShowFileInfo(selectedSong.SongPath);
                }
            }
        }

        private void grdViewAllRecentMusic_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (grdViewAllRecentMusic.SelectionMode == ListViewSelectionMode.Single)
            {
                var selectedSong = e.ClickedItem as RecentMusicModel;
                if (selectedSong == null) return;
                if (File.Exists(selectedSong.SongPath))
                {
                    ObservableCollection<SongModel> single = new();
                    string Title = Path.GetFileNameWithoutExtension(selectedSong.SongPath);
                    single.Add(new SongModel { FilePath = selectedSong.SongPath, Title = Title });
                    QueueService.PlayMedia(single, false, false);
                }
            }
        }
    }
}

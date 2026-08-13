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
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class GridViewRecentVideos : UserControl
    {
        public ListViewSelectionMode GridSelectionMode
        {
            get => (ListViewSelectionMode)GetValue(GridSelectionModeProperty);
            set => SetValue(GridSelectionModeProperty, value);
        }

        public static readonly DependencyProperty GridSelectionModeProperty =
            DependencyProperty.Register(
                nameof(GridSelectionMode),
                typeof(ListViewSelectionMode),
                typeof(GridViewRecentVideos),
                new PropertyMetadata(ListViewSelectionMode.Single)
            );

        public ObservableCollection<VideoProgress> ItemsSource
        {
            get => (ObservableCollection<VideoProgress>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register(
        nameof(ItemsSource),
        typeof(ObservableCollection<VideoProgress>),
        typeof(GridViewRecentVideos),
        new PropertyMetadata(null));
        public GridViewRecentVideos()
        {
            InitializeComponent();
        }
        public void SelectAll()
        {
            grdViewAllRecentVideo.SelectAll();

        }
        public void ClearSelection()
        {
            grdViewAllRecentVideo.DeselectAll();
        }
        public async void RemoveSelection()
        {
            var selectedItems = grdViewAllRecentVideo.SelectedItems.Cast<VideoProgress>().ToList();
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recents = currentSettings.SavedVideoProgress;
            foreach (var item in selectedItems)
            {
                ItemsSource.Remove(item);
                var exist = recents.FirstOrDefault(p => p.FilePath == item.FilePath);
                if (exist != null)
                {
                    recents.Remove(exist);
                }
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);

        }

        private void mnftPlayRecents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress selectedSong)
            {
                if (File.Exists(selectedSong.FilePath))
                {
                    ObservableCollection<SongModel> single = new();
                    string Title = Path.GetFileNameWithoutExtension(selectedSong.FilePath);
                    single.Add(new SongModel { FilePath = selectedSong.FilePath, Title = Title });
                    QueueService.PlayMedia(single, false, false);
                }
                else
                {
                    if (App.MainWindowInstance == null) return;
                    OceanContentDialog.Show("Missing File", "Relocate", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{selectedSong.FilePath}' does not exist.");

                    OceanContentDialog.PrimaryRequested += async () =>
                    {
                        if (App.OceanDialogInstance == null) return;

                        var currentSettings = await SettingsLoader.LoadSettingsAsync();
                        var savedmusic = currentSettings.SavedVideoProgress;
                        var file = await FilePickers.MediaPicker.PickSingleVideo(App.OceanDialogInstance, "Select File To Relocate");
                        if (file != null)
                        {
                            var exist = savedmusic.FirstOrDefault(p => p.FilePath == selectedSong.FilePath);
                            if (exist != null)
                            {
                                exist.FilePath = file.Path;
                                var musicproperties = await file.Properties.GetVideoPropertiesAsync();
                                await SettingsLoader.SaveSettingsAsync(currentSettings);

                                string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(file.Path) : musicproperties.Title;
                                selectedSong.FilePath = file.Path;
                                selectedSong.FileName = title;
                                selectedSong.Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(file.Path);
                                ObservableCollection<SongModel> single = new();
                                string Title = Path.GetFileNameWithoutExtension(selectedSong.FilePath);
                                single.Add(new SongModel { FilePath = selectedSong.FilePath, Title = Title });
                                QueueService.PlayMedia(single, false, false);
                                OceanContentDialog.HideDlg();
                                MainWindow.ShowWindow();
                            }
                        }
                    };
                }

            }
        }

        private void grdViewAllRecentVideo_ItemClick(object sender, ItemClickEventArgs e)
        {
            //if (grdViewAllRecentVideo.SelectionMode == ListViewSelectionMode.Single)
            //{
            //    var selectedSong = e.ClickedItem as RecentMusicModel;
            //    if (selectedSong == null) return;
            //    if (File.Exists(selectedSong.SongPath))
            //    {
            //        ObservableCollection<SongModel> single = new();
            //        string Title = Path.GetFileNameWithoutExtension(selectedSong.SongPath);
            //        single.Add(new SongModel { FilePath = selectedSong.SongPath, Title = Title });
            //        QueueService.PlayMedia(single, false, false);
            //    }
            //    else
            //    {
            //        if (App.MainWindowInstance == null) return;
            //        OceanContentDialog.Show("Missing File", "Relocate", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{selectedSong.SongPath}' does not exist.");

            //        OceanContentDialog.PrimaryRequested += async () =>
            //        {
            //            if (App.OceanDialogInstance == null) return;
            //            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            //            var savedmusic = currentSettings.RecentMusic;
            //            var file = await FilePickers.MediaPicker.PickSingleAudio(App.OceanDialogInstance, "Select File To Relocate");
            //            if (file != null)
            //            {
            //                var exist = savedmusic.FirstOrDefault(p => p.SongPath == selectedSong.SongPath);
            //                if (exist != null)
            //                {
            //                    exist.SongPath = file.Path;
            //                    var musicproperties = await file.Properties.GetMusicPropertiesAsync();
            //                    await SettingsLoader.SaveSettingsAsync(currentSettings);
            //                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(file.Path) : musicproperties.Title;
            //                    selectedSong.SongPath = file.Path;
            //                    selectedSong.SongName = title;
            //                    selectedSong.Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(file.Path);
            //                    ObservableCollection<SongModel> single = new();
            //                    string Title = Path.GetFileNameWithoutExtension(selectedSong.SongPath);
            //                    single.Add(new SongModel { FilePath = selectedSong.SongPath, Title = Title });
            //                    QueueService.PlayMedia(single, false, false);
            //                    OceanContentDialog.HideDlg();
            //                    MainWindow.ShowWindow();
            //                }
            //            }
            //        };
            //    }
            //}
        }

        private async void mnftRemoveFromRecents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress selectedSong)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var recentvideo = currentSettings.SavedVideoProgress;
                var defaultit = recentvideo.FirstOrDefault(p => p.FilePath == selectedSong.FilePath);
                if (defaultit == null) return;
                recentvideo.Remove(defaultit);

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

        private async void mnftAddToQueue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress selectedSong)
            {
                var exist = QueueService.VusicQueue.FirstOrDefault(p => p.FilePath == selectedSong.FilePath);
                if (exist == null)
                {
                    var file = await StorageFile.GetFileFromPathAsync(selectedSong.FilePath);
                    var props = await file.Properties.GetVideoPropertiesAsync();

                    string Title = props.Title ?? file.DisplayName;
                    if (Title == "")
                    {
                        Title = Path.GetFileNameWithoutExtension(file.Path);
                    }
               
                    QueueService.VusicQueueNext.Add(new SongModel { Title = Title, SongDuration = props.Duration, FilePath = file.Path });
                    QueueService.VusicQueue.Add(new SongModel { Title = Title, SongDuration = props.Duration, FilePath = file.Path });
                }
            }
        }

        private void mnftViewFileInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is RecentMusicModel selectedSong)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(selectedSong.SongPath);
                }
            }
        }
    }
}

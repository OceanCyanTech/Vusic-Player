// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 161126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Storage;




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

        private async void mnftPlayRecents_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is RecentMusicModel selectedSong)
            {
                if (File.Exists(selectedSong.SongPath))
                {
                    QueueService.AddToQueueAndPlay(selectedSong.SongPath);
                }
                else
                {
                    if (App.MainWindowInstance == null) return;
                    OceanContentDialog.Show("Missing File", "Relocate", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{selectedSong.SongPath}' does not exist.");

                    OceanContentDialog.PrimaryRequested += async () =>
                    {
                        if (App.OceanDialogInstance == null) return;

                        var currentSettings = await SettingsLoader.LoadSettingsAsync();
                        var savedmusic = currentSettings.RecentMusic;
                        var file = await FilePickers.MediaPicker.PickSingleAudio(App.OceanDialogInstance, "Select File To Relocate");
                        if (file != null)
                        {
                            var exist = savedmusic.FirstOrDefault(p => p.SongPath == selectedSong.SongPath);
                            if (exist != null)
                            {
                                exist.SongPath = file.Path;
                                QueueService.AddToQueueAndPlay(selectedSong.SongPath);
                                OceanContentDialog.HideDlg();
                                MainWindow.ShowWindow();
                            }
                        }
                    };
                }

            }
        }

        private async void mnftAddtoQueueRecentMusic_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is RecentMusicModel selectedSong)
            {
                QueueService.AddToQueue(selectedSong.SongPath);
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

        private async void grdViewAllRecentMusic_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (grdViewAllRecentMusic.SelectionMode == ListViewSelectionMode.Single)
            {
                var selectedSong = e.ClickedItem as RecentMusicModel;
                if (selectedSong == null) return;
                if (File.Exists(selectedSong.SongPath))
                {
                    QueueService.AddToQueueAndPlay(selectedSong.SongPath);
                }
                else
                {
                    if (App.MainWindowInstance == null) return;
                    OceanContentDialog.Show("Missing File", "Relocate", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{selectedSong.SongPath}' does not exist.");

                    OceanContentDialog.PrimaryRequested += async () =>
                    {
                        if (App.OceanDialogInstance == null) return;
                        var currentSettings = await SettingsLoader.LoadSettingsAsync();
                        var savedmusic = currentSettings.RecentMusic;
                        var file = await FilePickers.MediaPicker.PickSingleAudio(App.OceanDialogInstance, "Select File To Relocate");
                        if (file != null)
                        {
                            var exist = savedmusic.FirstOrDefault(p => p.SongPath == selectedSong.SongPath);
                            if (exist != null)
                            {
                                exist.SongPath = file.Path;
                                QueueService.AddToQueueAndPlay(selectedSong.SongPath);
                                OceanContentDialog.HideDlg();
                                MainWindow.ShowWindow();
                            }
                        }
                    };
                }
            }
        }


    }
}

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
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{

    public sealed partial class HomeView : Page
    {
        public HomeView()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            expRecentsVideo.Visibility = currentSettings.Panel1VisibilityHome ? Visibility.Visible : Visibility.Collapsed;
            txtDisplay1.Visibility = currentSettings.Panel2VisibilityHome ? Visibility.Visible : Visibility.Collapsed;
            txtDisplay2.Visibility = currentSettings.Panel3VisibilityHome ? Visibility.Visible : Visibility.Collapsed;
            txtDisplay3.Visibility = currentSettings.Panel4VisibilityHome ? Visibility.Visible : Visibility.Collapsed;
            expRecentMusic.Visibility = currentSettings.Panel2VisibilityHome ? Visibility.Visible : Visibility.Collapsed;
            expMusicMix.Visibility = currentSettings.Panel3VisibilityHome ? Visibility.Visible : Visibility.Collapsed;
            expFav.Visibility = currentSettings.Panel4VisibilityHome ? Visibility.Visible : Visibility.Collapsed;
            FoldersOpened.CollectionChanged -= FoldersOpened_CollectionChanged;
            FoldersOpened.Clear();

            if (currentSettings?.FoldersRecent != null)
            {
                // Use a HashSet with OrdinalIgnoreCase to track and filter duplicate paths
                var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var folder in currentSettings.FoldersRecent)
                {
                    string path = folder?.FolderPath?.Trim() ?? string.Empty;
                    if (string.IsNullOrEmpty(path) || !Directory.Exists(path) || !seenPaths.Add(path))
                        continue;
                    if (folder == null) continue;
                    FoldersOpened.Add(new FolderModel
                    {
                        FolderName = string.IsNullOrWhiteSpace(folder.FolderName)
                            ? new DirectoryInfo(path).Name
                            : folder.FolderName,
                        FolderPath = path
                    });
                }
            }

            if (FoldersOpened.Count == 0)
            {
                grdEmptySuggestions.Visibility = Visibility.Visible;
                grdViewFoldersOpened.Visibility = Visibility.Collapsed;
                stkFoldersOptions.Visibility = Visibility.Collapsed;
            }
            else
            {
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
                grdViewFoldersOpened.Visibility = Visibility.Visible;
                stkFoldersOptions.Visibility = Visibility.Visible;
            }

            // Subscribe to collection changes after the initial load is complete
            FoldersOpened.CollectionChanged += FoldersOpened_CollectionChanged;
            base.OnNavigatedTo(e);
        }

        private async void FoldersOpened_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (currentSettings == null) return;

            // Deduplicate by path (case-insensitive) before persisting
            var uniqueFolders = FoldersOpened
                .Where(f => !string.IsNullOrWhiteSpace(f.FolderPath))
                .DistinctBy(f => f.FolderPath, StringComparer.OrdinalIgnoreCase)
                .ToList();

            currentSettings.FoldersRecent = new ObservableCollection<FolderModel>(uniqueFolders);
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        ObservableCollection<FolderModel> FoldersOpened = new ObservableCollection<FolderModel>();
        private async void btnOpenMedia_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var media = await MediaPicker.PickSingle(App.MainWindowInstance, "Open Media");
            if (media != null)
            {
                bool isAudio = AudioExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                bool isVideo = VideoExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                if (isAudio)
                {

                    if (File.Exists(media.Path))
                    {
                        ObservableCollection<SongModel> single = new();
                        string Title = Path.GetFileNameWithoutExtension(media.Path);

                        single.Add(new SongModel { FilePath = media.Path, Title = Title, AlbumName = AudioMetadata.Album(media.Path), Artist = AudioMetadata.Artist(media.Path), SongDuration = await AudioMetadata.GetTimeSpanDuration(media.Path) });
                        QueueService.PlayMedia(single, false, false);
                    }
                }
                else if (isVideo)
                {
                    if (PlayerService.InVideoPage == false)
                    {
                        if (File.Exists(media.Path))
                            Frame.Navigate(typeof(VideoPlayer), media.Path);
                    }
                    else
                    {
                        PlayerService.OpenPath(media.Path);
                    }
                    
                }
                else
                {
                    //Handle other cases
                }
            }
        }

        private async void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Open Folder", Windows.Storage.Pickers.PickerLocationId.Downloads);
            if(folder != null)
            {
                if(App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = Path.GetFileName(folder.Path), FolderPath = folder.Path });
                }
            }
        }

        private void expRecentsVideo_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            if (!this.IsLoaded) return;

            if (sender != expRecentsVideo) expRecentsVideo.IsExpanded = false;

            if (sender != expRecentMusic) expRecentMusic.IsExpanded = false;

            if (sender != expMusicMix) expMusicMix.IsExpanded = false;

            if (sender != expFav) expFav.IsExpanded = false;
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            expRecentMusic.IsExpanded = true;
        }

        private void TextBlock_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            expMusicMix.IsExpanded = true;
        }

        private void TextBlock_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            expFav.IsExpanded = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(DummyPage));

        }

        private void hypViewAllFav_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FavouritesView));
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            grdViewFoldersOpened.SelectionMode = chkSelect.IsChecked == true ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = chkSelect.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void chckSelectAllFolders_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllFolders.IsChecked == true) grdViewFoldersOpened.SelectAll();
            else grdViewFoldersOpened.SelectedItems.Clear();
        }

        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
       
            var selecteditems = grdViewFoldersOpened.SelectedItems.OfType<FolderModel>().ToList();
            foreach(var selecteditem in selecteditems)
            {
                FoldersOpened.Remove(selecteditem);
            }
            if (FoldersOpened.Count == 0)
            {
                grdEmptySuggestions.Visibility = Visibility.Visible;
                grdViewFoldersOpened.Visibility = Visibility.Collapsed;
                stkFoldersOptions.Visibility = Visibility.Collapsed;
            }
            else
            {
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
                grdViewFoldersOpened.Visibility = Visibility.Visible;
                stkFoldersOptions.Visibility = Visibility.Visible;
            }
        }

        private void grdViewFoldersOpened_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (chkSelect.IsChecked == true) return;
            if(e.ClickedItem is FolderModel folder)
            {
                if(App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(FolderView), folder);
                }
            }
        }

        private void mnftOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FolderModel folder)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(FolderView), folder);
                }
            }
        }

        private void mnftOpenFolderLoc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FolderModel folder)
            {
                if (Directory.Exists(folder.FolderPath))
                {

                    Process.Start("explorer.exe", $"\"{folder.FolderPath}\"");
                }
            }
        }

        private void mnftCopyFolderLoc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FolderModel folder)
            {
                CopyToClipboard.CopyStringToClipboard(folder.FolderPath);
            }
        }

        private void mnftRemoveFolderLoc_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FolderModel folder)
            {
                FoldersOpened.Remove(folder);
                if (FoldersOpened.Count == 0)
                {
                    grdEmptySuggestions.Visibility = Visibility.Visible;
                    grdViewFoldersOpened.Visibility = Visibility.Collapsed;
                    stkFoldersOptions.Visibility = Visibility.Collapsed;
                }
                else
                {
                    grdEmptySuggestions.Visibility = Visibility.Collapsed;
                    grdViewFoldersOpened.Visibility = Visibility.Visible;
                    stkFoldersOptions.Visibility = Visibility.Visible;
                }
            }
        }
    }
}

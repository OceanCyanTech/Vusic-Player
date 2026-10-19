// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

#region Namespaces
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
#endregion


/*  OCEANCYAN TECH HEADER
 VUSIC PLAYER - VERSION v1.0.0.0
© OCEANCYAN TECH 2026

PAGE - HOMEVIEW
BUILD 211026
 */

namespace Vusic_Player.Pages.Views
{
    public sealed partial class HomeView : Page
    {
        #region Fields
        IconHolder IconHolderInstance => IconHolder.Instance;
        ObservableCollection<FolderModel> FoldersOpened = new ObservableCollection<FolderModel>();

        #endregion

        #region Page Properties
        public HomeView()
        {
            InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
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
                expFav.IsExpanded = false;
                expRecentMusic.IsExpanded = false;
                expRecentsVideo.IsExpanded = false;
                expMusicMix.IsExpanded = false;
                if (currentSettings.ExpandedPanelHomeView == "expRecentsVideo")
                {
                    expRecentsVideo.IsExpanded = true;
                }
                else if (currentSettings.ExpandedPanelHomeView == "expRecentMusic")
                {
                    expRecentMusic.IsExpanded = true;
                }
                else if (currentSettings.ExpandedPanelHomeView == "expMusicMix")
                {
                    expMusicMix.IsExpanded = true;
                }
                else if (currentSettings.ExpandedPanelHomeView == "expFav")
                {
                    expFav.IsExpanded = true;
                }
                if (currentSettings?.FoldersRecent != null)
                {
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
                ManageVisibilityFolderGrid();
                FoldersOpened.CollectionChanged += FoldersOpened_CollectionChanged;
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.ONNAVIGATEDTO", Logger.LogLevelType.Error);
            }
            base.OnNavigatedTo(e);
        }

        private async void btnOpenMedia_Click(object sender, RoutedEventArgs e)
        {
            try
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
                        ShowException("Error", "Cannot Open file of Invalid Format");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.OPENMEDIA", Logger.LogLevelType.Error);
            }
        }
        private void ShowException(string title, string Message)
        {
            ttEXCEPTION.Title = title;
            txtExceptionError.Text = Message;
            ttEXCEPTION.IsOpen = true;
        }
        #endregion

        #region Expander Properties
        private async void expRecentsVideo_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            try
            {
                if (!this.IsLoaded) return;

                if (sender != expRecentsVideo) expRecentsVideo.IsExpanded = false;

                if (sender != expRecentMusic) expRecentMusic.IsExpanded = false;

                if (sender != expMusicMix) expMusicMix.IsExpanded = false;

                if (sender != expFav) expFav.IsExpanded = false;
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                currentSettings.ExpandedPanelHomeView = sender.Name;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.EXPANDER_EXPANDING", Logger.LogLevelType.Error);
            }
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
        #endregion

        #region Folders Grid
        private async void FoldersOpened_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            try
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                if (currentSettings == null) return;
                var uniqueFolders = FoldersOpened
                    .Where(f => !string.IsNullOrWhiteSpace(f.FolderPath))
                    .DistinctBy(f => f.FolderPath, StringComparer.OrdinalIgnoreCase)
                    .ToList();

                currentSettings.FoldersRecent = new ObservableCollection<FolderModel>(uniqueFolders);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch
            {
                //IGNORE
            }
        }
        private async void btnOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (App.MainWindowInstance == null) return;
                var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Open Folder", Windows.Storage.Pickers.PickerLocationId.Downloads);
                if (folder != null)
                {
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = Path.GetFileName(folder.Path), FolderPath = folder.Path });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.OPENFOLDER", Logger.LogLevelType.Error);
            }
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
            //Remove Folders
            try
            {
                var selecteditems = grdViewFoldersOpened.SelectedItems.OfType<FolderModel>().ToList();
                foreach (var selecteditem in selecteditems)
                {
                    FoldersOpened.Remove(selecteditem);
                }
                ManageVisibilityFolderGrid();
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.REMOVEFOLDER", Logger.LogLevelType.Error);
            }
        }
        private void ManageVisibilityFolderGrid()
        {
            grdEmptySuggestions.Visibility = FoldersOpened.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            grdViewFoldersOpened.Visibility = FoldersOpened.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            stkFoldersOptions.Visibility = FoldersOpened.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        private void grdViewFoldersOpened_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                if (chkSelect.IsChecked == true) return;
                if (e.ClickedItem is FolderModel folder)
                {
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(FolderView), folder);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.FOLDERITEMCLICKED", Logger.LogLevelType.Error);
            }
        }

        private void mnftOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem mnft && mnft.DataContext is FolderModel folder)
                {
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(FolderView), folder);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.OPENFOLDERMENUFLYOUT", Logger.LogLevelType.Error);
            }
        }

        private void mnftOpenFolderLoc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem mnft && mnft.DataContext is FolderModel folder)
                {
                    if (Directory.Exists(folder.FolderPath))
                    {
                        Process.Start("explorer.exe", $"\"{folder.FolderPath}\"");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.OPENFOLDERLOC", Logger.LogLevelType.Error);
            }
        }

        private void mnftCopyFolderLoc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem mnft && mnft.DataContext is FolderModel folder)
                {
                    CopyToClipboard.CopyStringToClipboard(folder.FolderPath);
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.COPYFOLDERLOC", Logger.LogLevelType.Error);
            }
        }

        private void mnftRemoveFolderLoc_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuFlyoutItem mnft && mnft.DataContext is FolderModel folder)
                {
                    FoldersOpened.Remove(folder);
                    ManageVisibilityFolderGrid();
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.REMOVEFOLDERMENUFLYOUT", Logger.LogLevelType.Error);
            }
        }
        #endregion

        #region Miscellaneous
        private void hypViewAllFav_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(FavouritesView));
        }
        #endregion
    }
}

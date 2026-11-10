// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 091126) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

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
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Experience.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FolderSelection : Page
    {
        public FolderSelection()
        {
            InitializeComponent();
        }
        ObservableCollection<FoldersListOpened> foldersListOpened = new();

        private async void btnAddFolder_Click(object sender, RoutedEventArgs e)
        {
            var obser = new ObservableCollection<SongModel>();

            if (App.MainWindowInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Choose Folder", Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
            if (folder != null)
            {
                // Detach old handlers if any, and attach a specific one for THIS button

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                Debug.WriteLine("folder add");
                var check = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                var savedfolders = currentSettings.SavedFoldersOpened;
                if (check == null)
                {
                    Debug.WriteLine("folder add NULL CHECK");

                    savedfolders.Add(new FoldersListOpened { FolderPath = folder.Path, isChecked = true, FolderName = Path.GetFileName(folder.Path) });
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    Debug.WriteLine("folder add NULL CHECK test");
                    var alreadyexist = foldersListOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                    if (alreadyexist != null) return;

                    foldersListOpened.Add(new FoldersListOpened { FolderPath = folder.Path, FolderName = Path.GetFileName(folder.Path), isChecked = true });

                }
            }

        }

        private void mnftOpenFolder_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {

                if (tgl.FolderPath != null)
                {

                    if (Directory.Exists(tgl.FolderPath))
                    {

                        Process.Start("explorer.exe", $"\"{tgl.FolderPath}\"");
                    }
                }
            }
        }

        private void mnftCopyPathFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {
                if (tgl.FolderPath is string str)
                {
                    CopyToClipboard.CopyStringToClipboard(str);
                }
            }
        }

        private async void mnftRemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            //Remove Folder from the List
            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {
                foldersListOpened.Remove(tgl);
                UserDataPaths paths = UserDataPaths.GetDefault();

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                if (tgl.IsHighLevelFolder)
                {
                    bool isChecked = false;

                    switch (tgl.FolderPath)
                    {
                        case var p when p == paths.Documents:
                            currentSettings.IsDocumentEnabled = isChecked;
                            break;
                        case var p when p == paths.Downloads:
                            currentSettings.IsDownloadsEnabled = isChecked;
                            break;
                        case var p when p == paths.Pictures:
                            currentSettings.IsPicturesEnabled = isChecked;
                            break;
                        case var p when p == paths.Videos:
                            currentSettings.IsVideosEnabled = isChecked;
                            break;
                        case var p when p == paths.Music:
                            currentSettings.IsMusicEnabled = isChecked;
                            break;
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);

                }
                else
                {
                    if (tgl.FolderPath is string str)
                    {
                        var folder = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == str);
                        if (folder != null)
                        {
                            Debug.WriteLine(str);
                            currentSettings.SavedFoldersOpened.Remove(folder);
                        }
                        await SettingsLoader.SaveSettingsAsync(currentSettings);
                    }
                }
            }
        }
        private bool _isInitializingSettings = false;
        private async void btnGenericFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tgl && tgl.DataContext is FoldersListOpened folder)
            {
                tgl.IsEnabled = false; // Prevent rapid re-toggling during I/O

                try
                {
                    bool isChecked = tgl.IsChecked ?? false;

                    // 1. Update in-memory state
                    folder.isChecked = isChecked;

                    // 2. Persist to settings
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var saved = currentSettings.SavedFoldersOpened.FirstOrDefault(p =>
                        string.Equals(p.FolderPath, folder.FolderPath, StringComparison.OrdinalIgnoreCase));

                    if (saved != null)
                    {
                        saved.isChecked = isChecked;
                        await SettingsLoader.SaveSettingsAsync(currentSettings);
                    }

                    // 3. Dynamically purge or index without restarting
                 
                }
                finally
                {
                    tgl.IsEnabled = true;
                }
            }
        }
        private async void HighLevelFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitializingSettings || sender is not CheckBox chk)
                return;

            bool isEnabled = chk.IsChecked ?? false;
            var paths = Windows.Storage.UserDataPaths.GetDefault();
            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            string folderPath = "";
            string folderName = chk.Content?.ToString() ?? "";

            // Determine target path and update appropriate setting flag
            if (chk == chkMusic)
            {
                currentSettings.IsMusicEnabled = isEnabled;
                folderPath = paths.Music;
            }
            else if (chk == chkVideos)
            {
                currentSettings.IsVideosEnabled = isEnabled;
                folderPath = paths.Videos;
            }
            else if (chk == chkPictures)
            {
                currentSettings.IsPicturesEnabled = isEnabled;
                folderPath = paths.Pictures;
            }
            else if (chk == chkDocuments)
            {
                currentSettings.IsDocumentEnabled = isEnabled;
                folderPath = paths.Documents;
            }
            else if (chk == chkDownloads)
            {
                currentSettings.IsDownloadsEnabled = isEnabled;
                folderPath = paths.Downloads;
            }

            await SettingsLoader.SaveSettingsAsync(currentSettings);

            // Sync in-memory list
            if (!string.IsNullOrEmpty(folderPath))
            {
                var existing = foldersListOpened.FirstOrDefault(f =>
                    string.Equals(f.FolderPath, folderPath, StringComparison.OrdinalIgnoreCase));

                if (isEnabled && existing == null)
                {
                    foldersListOpened.Add(new FoldersListOpened
                    {
                        FolderName = folderName,
                        FolderPath = folderPath,
                        IsHighLevelFolder = true
                    });
                }
                else if (!isEnabled && existing != null)
                {
                    foldersListOpened.Remove(existing);
                }
            }
        }
    }
}

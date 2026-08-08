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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;

/*
             • ENTIRE VIDEO LIBRARY
             • Display of all videos in user library, playlists and shows created by user. 
             • VUSIC PLAYER VERSION 1.1.0.0
             © OCEANCYAN TECH 2026
*/


namespace Vusic_Player.Pages.Views
{
    public sealed partial class EntireVideoLibrary : Page
    {
        #region Field Declarations

        //Observable Collections
        ObservableCollection<SongModel> AllAvailableSongs = new ObservableCollection<SongModel>();
        ObservableCollection<FoldersListOpened> foldersListOpened = new();

        #endregion
        public EntireVideoLibrary()
        {
            InitializeComponent();
        }


        #region All Videos Grid

        //             All Videos Grid Control Events
        #region Play All
        //Play All Btn
        private void btnPlayAllVideos_Click(object sender, RoutedEventArgs e)
        {
            //Play all videos displayed in Video Library
            QueueService.PlayMedia(AllAvailableSongs, false, false);
        }

        private void mnftPlayShuffled_Click(object sender, RoutedEventArgs e)
        {
            //Play Shuffled
            QueueService.PlayMedia(AllAvailableSongs, true, false);
        }

        private void mnftPlayOnLoop_Click(object sender, RoutedEventArgs e)
        {
            //Play on Loop
            QueueService.PlayMedia(AllAvailableSongs, false, true);
        }

        private void mnftPlayOnLoopShuffled_Click(object sender, RoutedEventArgs e)
        {
            //Play on Loop and Shuffled
            QueueService.PlayMedia(AllAvailableSongs, true, true);
        }
        #endregion

        #region Folder Toggle Button
        private async void btnGenericFolder_Checked(object sender, RoutedEventArgs e)
        {
            //Folder check/uncheck event
            if (sender is ToggleButton tgl && tgl.DataContext is FoldersListOpened folder)
            {
                if (tgl.IsChecked == true)
                {
                    if (tgl.DataContext is FoldersListOpened folder)
                    {
                        var currentSettings = await SettingsLoader.LoadSettingsAsync();
                        var folderssaved = currentSettings.SavedFoldersOpened;
                        var exist = folderssaved.FirstOrDefault(p => p.FolderPath == folder.FolderPath);
                        if (exist != null)
                        {
                            exist.isChecked = true;
                        }
                        var exist2 = foldersListOpened.FirstOrDefault(p => p.FolderPath == folder.FolderPath);

                        if (exist2 != null)
                        {
                            exist2.isChecked = true;
                        }
                        await SettingsLoader.SaveSettingsAsync(currentSettings);
                        var folderpath = folder.FolderPath;
                        var list = new List<string>();
                        list.Add(folderpath);
                        await LoadAllFiles(list);
                    }
                }
                else if (tgl.IsChecked == false)
                {
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var folderssaved = currentSettings.SavedFoldersOpened;
                    var exist = folderssaved.FirstOrDefault(p => p.FolderPath == folder.FolderPath);
                    if (exist != null)
                    {
                        exist.isChecked = false;
                    }
                    var exist2 = foldersListOpened.FirstOrDefault(p => p.FolderPath == folder.FolderPath);

                    if (exist2 != null)
                    {
                        exist2.isChecked = true;
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    var folderpath = folder.FolderPath;
                    string folderWithBackslash = folderpath.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? folderpath
                : folderpath + Path.DirectorySeparatorChar;

                    var songsInThisFolder = AllAvailableSongs.Where(p =>
            p.FilePath.StartsWith(folderWithBackslash, StringComparison.OrdinalIgnoreCase))
            .ToList();

                    foreach (var song in songsInThisFolder)
                    {
                        AllAvailableSongs.Remove(song);
                    }
                }
            }
        }

        private void mnftOpenFolder_Click(object sender, RoutedEventArgs e)
        {
            //Open Folder in File Explorer
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
            //Copy Folder Path
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
                var currentse = await SettingsLoader.LoadSettingsAsync();
                if (tgl.FolderPath is string str)
                {
                    var folder = currentse.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == str);
                    if (folder != null)
                    {
                        Debug.WriteLine(str);
                        currentse.SavedFoldersOpened.Remove(folder);
                    }
                    await SettingsLoader.SaveSettingsAsync(currentse);
                }
                List<string> fpaths = new();
                foreach (var item in foldersListOpened)
                {
                    if (item.isChecked == true)
                    {
                        fpaths.Add(item.FolderPath);
                    }
                }
                AllAvailableSongs.Clear();
                await LoadAllFiles(fpaths);
            }
        }
        #endregion

        #endregion



    }
}

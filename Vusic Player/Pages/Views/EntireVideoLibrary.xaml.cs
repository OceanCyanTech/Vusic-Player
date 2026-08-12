using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Windows.Storage;

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
        ObservableCollection<SongModel> AllAvailableVideos = new ObservableCollection<SongModel>();
        ObservableCollection<FoldersListOpened> foldersListOpened = new();
        ObservableCollection<PlaylistItem> playlistsAll = new();
        ObservableCollection<Show> showsAll = new();
        ObservableCollection<VideoProgress> recentVideo = new();
        #endregion

        public EntireVideoLibrary()
        {
            InitializeComponent();
           LoadFolders();
            LoadSettings();
        }

        #region Initialization

        private async void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            //Legend Toggle Buttons Checked
            var selectedBtn = sender as ToggleButton;
            if (selectedBtn == null) return;
            ToggleButton[] videoButtons = { btnAllVideos, btnPlaylists, btnShows, btnHistory };

            foreach (var btn in videoButtons)
            {
                if (btn != selectedBtn)
                {
                    btn.IsChecked = false;
                }
            }

            selectedBtn.IsChecked = true;
            string category = selectedBtn.Content.ToString()!;
            grdAllVideos.Visibility = Visibility.Collapsed;
            grdShows.Visibility = Visibility.Collapsed;
            grdPlaylists.Visibility = Visibility.Collapsed;
            grdAllVideoHistory.Visibility = Visibility.Collapsed;
            if (category == "Video History")
            {
                grdAllVideoHistory.Visibility = Visibility.Visible;
                LoadHistory();
            }
            else if (category == "All Videos")
            {
                grdAllVideos.Visibility = Visibility.Visible;
            }
            else if (category == "Playlists")
            {
                grdPlaylists.Visibility = Visibility.Visible;

                LoadAllPlaylists();
                Debug.WriteLine("Playlist");
            }
            else if (category == "Shows")
            {
                grdShows.Visibility = Visibility.Visible;
                LoadAllShows();
                Debug.WriteLine("Shows");
            }
        }

        #endregion

        #region File System Database

        private async void LoadFolders()
        {
            UserDataPaths paths = UserDataPaths.GetDefault();
            grdViewFolders.ItemsSource = foldersListOpened;
            // Assigning the system paths to the Tag property

            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            var folders = currentSettings.SavedFoldersOpened;

            foreach (var item in currentSettings.SavedFoldersOpened)
            {
                if (item.Show == true)
                {
                    var exist = foldersListOpened.FirstOrDefault(p => p.FolderPath == item.FolderPath);
                    if (exist == null)
                    {
                        item.FolderName = Path.GetFileName(item.FolderPath);
                        foldersListOpened.Add(item);
                    }

                }
                else
                {
                    Debug.WriteLine("item show = false " + item.FolderPath);
                    var exist = foldersListOpened.FirstOrDefault(p => p.FolderPath == item.FolderPath);
                    if (exist != null)
                    {
                        if (exist.Show == false)
                        {

                            foldersListOpened.Remove(item);
                        }
                    }
                }
                //    ToggleButton toggleButton = new();
                //    if (item.isChecked)
                //    {
                //        toggleButton.IsChecked = true;
                //    }
                //    toggleButton.ContextFlyout = (MenuFlyout)this.Resources["FolderContextMenu"];
                //    var flyout = toggleButton.ContextFlyout as MenuFlyout;
                //    var openItem = flyout.Items[0] as MenuFlyoutItem; // "Open Location"
                //    var removeItem = flyout.Items[2] as MenuFlyoutItem; // "Remove" (skip separator)

                //    // Detach old handlers if any, and attach a specific one for THIS button
                //    openItem.Click += (s, e) =>
                //    {
                //        string path = item.FolderPath;
                //        if (System.IO.Directory.Exists(path))
                //        {
                //            System.Diagnostics.
                //            ("explorer.exe", $"/select,\"{path}\"");
                //        }
                //    };
                //    toggleButton.Tag = item.FolderPath;
                //    toggleButton.CornerRadius = new CornerRadius(16);
                //    toggleButton.FontSize = 16;
                //    ToolTipService.SetToolTip(toggleButton, item.FolderPath);

                //    toggleButton.Padding = new Thickness(10);
                //    toggleButton.Content = Path.GetFileName(item.FolderPath);
                //    if (App.Current.Resources.TryGetValue("SurfaceStrokeColorDefaultBrush", out object resource))
                //    {
                //        toggleButton.BorderBrush = (Brush)resource;
                //    }
                //    toggleButton.Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent);
                //    stkAddedFolders.Children.Add(toggleButton);
                //}
            }
         
            AllAvailableVideos.Clear();
            LoadAllFiles();
            stkLoading.Visibility = Visibility.Collapsed;
        }

        private async void LoadAllFiles()
        {
            bool includeSubDirs = chkIncludeSubDirectories.IsChecked == true;

            AllAvailableVideos.Clear();
            var rawfiles = FilesInDatabase.rawSongs;
            foreach (var folder in foldersListOpened)
            {
                var videofiles = rawfiles.Where(p => !string.IsNullOrEmpty(p.FilePath) && VideoExtensions.List.Contains(Path.GetExtension(p.FilePath).ToLowerInvariant()) && Path.GetDirectoryName(p.FilePath) == folder.FolderPath);

                foreach (var videof in videofiles)
                {
                    if (AllAvailableVideos.Any(v => string.Equals(v.FilePath, videof.FilePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }
                    Debug.WriteLine("ADDING FILE: " + videof.FilePath);
                    AllAvailableVideos.Add(new SongModel { Title = videof.Title, FilePath = videof.FilePath, SongDuration = videof.SongDuration, Genre = videof.Genre, IsAudioItem = false, VisibilityofAudioMeta = Visibility.Collapsed, VisibilityofVideoInfo = Visibility.Visible, Glyph = "\uE8B2" });
                }
            }
            txtVideoCount.Text = $"• {AllAvailableVideos.Count} {(AllAvailableVideos.Count == 1 ? "item":  "items") }";
            grdEmptyLibrary.Visibility = (AllAvailableVideos.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            AllVideosGroupedCollection.Visibility = (AllAvailableVideos.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion

        #region Grids
        #region All Videos Grid

        //             All Videos Grid Control Events
        #region Play All
        //Play All Btn
        private void btnPlayAllVideos_Click(object sender, RoutedEventArgs e)
        {
            //Play all videos displayed in Video Library
            QueueService.PlayMedia(AllAvailableVideos, false, false);
        }

        private void mnftPlayShuffled_Click(object sender, RoutedEventArgs e)
        {
            //Play Shuffled
            QueueService.PlayMedia(AllAvailableVideos, true, false);
        }

        private void mnftPlayOnLoop_Click(object sender, RoutedEventArgs e)
        {
            //Play on Loop
            QueueService.PlayMedia(AllAvailableVideos, false, true);
        }

        private void mnftPlayOnLoopShuffled_Click(object sender, RoutedEventArgs e)
        {
            //Play on Loop and Shuffled
            QueueService.PlayMedia(AllAvailableVideos, true, true);
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
                    LoadAllFiles();

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

                    var songsInThisFolder = AllAvailableVideos.Where(p =>
            p.FilePath.StartsWith(folderWithBackslash, StringComparison.OrdinalIgnoreCase))
            .ToList();

                    foreach (var song in songsInThisFolder)
                    {
                        AllAvailableVideos.Remove(song);
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

                AllAvailableVideos.Clear();
                LoadAllFiles();
            }
        }

        #endregion

        private async void btnAddFolders_Click(object sender, RoutedEventArgs e)
        {
            //Add folders to list
            var obser = new ObservableCollection<SongModel>();

            if (App.MainWindowInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Choose Folder", Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
            if (folder != null)
            {
                var alreadyexist = foldersListOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                if (alreadyexist != null) return;

                foldersListOpened.Add(new FoldersListOpened { FolderPath = folder.Path, FolderName = Path.GetFileName(folder.Path), isChecked = true });
                // Detach old handlers if any, and attach a specific one for THIS button

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                Debug.WriteLine("folder add");
                var check = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                if (check == null)
                {
                    Debug.WriteLine("folder add NULL CHECK");

                    currentSettings.SavedFoldersOpened.Add(new FoldersListOpened { FolderPath = folder.Path, isChecked = true });
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                }
            }
        }

        private async void chkIncludeSubDirectories_Checked(object sender, RoutedEventArgs e)
        {
            //Check box for including sub-directories
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            stkLoading.Visibility = Visibility.Visible;

            currentSettings.IncludeSubDirMusLib = chkIncludeSubDirectories.IsChecked ?? true;
            await SettingsLoader.SaveSettingsAsync(currentSettings);

            LoadFolders();
        }



        #endregion

        #region Video History Grid

        private async void LoadHistory()
        {
            stkLoadingAll.Visibility = Visibility.Visible;
            grdViewAllRecentMusic.ItemsSource = recentVideo;

            recentVideo.CollectionChanged -= RecentVideo_CollectionChanged;
            recentVideo.CollectionChanged += RecentVideo_CollectionChanged;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (currentSettings.IsVideoHistoryDisabled == true)
            {
                btnDisableHistory.Visibility = Visibility.Collapsed;
                grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                asbRecents.Visibility = Visibility.Collapsed;
                chkSelect.Visibility = Visibility.Collapsed;
                stkDisabledHistory.Visibility = Visibility.Visible;
                btnPlayAll.Visibility = Visibility.Collapsed;
                btnClearHistory.Visibility = Visibility.Collapsed;
                stkEmptyHistory.Visibility = Visibility.Collapsed;
                return;
            }
            var recentvideos = currentSettings.SavedVideoProgress;

            foreach (var item in recentvideos)
            {
                Debug.WriteLine("LOADING HISTORY " + item.FilePath);
                recentVideo.Add(new VideoProgress
                {
                    FolderName = new DirectoryInfo(
                            Path.GetDirectoryName(item.FilePath) ?? string.Empty
                        ).Name,
                    FileName = Path.GetFileNameWithoutExtension(item.FilePath),
                    FilePath = item.FilePath,
                    HoverText = $"{Path.GetFileNameWithoutExtension(item.FilePath)}" + Environment.NewLine + "Folder: " + new DirectoryInfo(
                            Path.GetDirectoryName(item.FilePath) ?? string.Empty
                        ).Name,
                    LoadingVisibility = Visibility.Visible

                });
            }
            foreach (var item in recentVideo)
            {
                var fallbackUri = "ms-appx:///Assets/default.png";
                item.Thumbnail = new BitmapImage(new Uri(fallbackUri));
                var task = Task.Run(async () =>
                {
                    var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(item.FilePath, 0.20);
                    Debug.WriteLine("The thumbnail path is " + thumb);

                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        try
                        {
                            var bitmap = new BitmapImage();

                            //    Check if the path is our app asset URI string
                            if (thumb.StartsWith("ms-appx://"))
                            {
                                //    Assign the URI directly to the BitmapImage
                                bitmap.UriSource = new Uri(thumb);
                            }
                            else if (File.Exists(thumb))
                            {
                                //     It's a real generated file path in the Temp folder! Read the stream.
                                using (var stream = File.OpenRead(thumb))
                                {
                                    await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                                }
                                item.Thumbnail = bitmap;
                                item.ThumbnailPath = thumb;
                                //       Delete the file immediately after the stream closes safely
                                File.Delete(thumb);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("An unexpected error occured: " + ex.Message);
                        }
                    });

                });
                item.LoadingVisibility = Visibility.Collapsed;

            }
            //foreach (var item in recentvideos)
            //{
            //    var task = Task.Run(async () =>
            //    {
            //        var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(item.FilePath, 0.20);
            //        Debug.WriteLine("The thumbnail path is " + thumb);

            //        DispatcherQueue.TryEnqueue(async () =>
            //        {
            //            try
            //            {
            //                var bitmap = new BitmapImage();
            //                using (var stream = File.OpenRead(thumb))
            //                {
            //                    await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
            //                }
            //                item.Thumbnail = bitmap;
            //                File.Delete(thumb);
            //            }
            //            catch (Exception ex)
            //            {
            //                Debug.WriteLine("An unexpected error occured: " + ex.Message);
            //            }
            //        });

            //    });
            //}
            if (recentvideos.Count == 0)
            {
                grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                stkEmptyHistory.Visibility = Visibility.Visible;
                btnClearHistory.IsEnabled = false;
                btnPlayAll.IsEnabled = false;
                asbRecents.IsEnabled = false;

            }
            else
            {
                grdViewAllRecentMusic.Visibility = Visibility.Visible;
                stkEmptyHistory.Visibility = Visibility.Collapsed;
                btnClearHistory.IsEnabled = true;
                btnPlayAll.IsEnabled = true;
                asbRecents.IsEnabled = true;

            }
            stkLoadingAll.Visibility = Visibility.Collapsed;

        }

        private void RecentVideo_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
        }

        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            //Play all Videos in Video History
            var videohistory = recentVideo;
            if (videohistory.Count == 0) return;
            ObservableCollection<SongModel> tempTransfer = new();

            foreach (var item in videohistory)
            {
                var existingvideo = AllAvailableVideos.FirstOrDefault(p => p.FilePath == item.FilePath);
                if (existingvideo != null)
                {
                    Debug.WriteLine("ITEM FILE APTHS " + item.FilePath);
                    if (File.Exists(item.FilePath))
                    {
                        tempTransfer.Add(new SongModel { Title = existingvideo.Title, SongDuration = existingvideo.SongDuration, FilePath = existingvideo.FilePath, VisibilityofAudioMeta = Visibility.Collapsed, VisibilityofVideoInfo = Visibility.Visible });
                    }
                }
            }
            QueueService.PlayMedia(tempTransfer, false, false);
        }

        private async void btnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recentvideos = currentSettings.SavedVideoProgress;
            recentVideo.Clear();
            recentvideos.Clear();
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private void btnDisableHistory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void asbRecents_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {

        }

        private void asbRecents_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void btnEnableHistory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnRemoveFromHistory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Playlists Grid
        private async void LoadAllPlaylists()
        {
            //Load All Playlists
          
            var currentset = await SettingsLoader.LoadSettingsAsync();
            var playlists = currentset.SavedPlaylists;
            playlistsAll.Clear();

            foreach (var playlist in playlists)
            {
                Debug.WriteLine("PLAYLIST BEING ADDED IS :" + playlist.PlaylistName);
                playlistsAll.Add(new PlaylistItem { PlaylistId = playlist.PlaylistId, PlaylistName = playlist.PlaylistName, PlaylistCount = playlist.PlaylistCount, Thumbnail = playlist.Thumbnail, SongsPaths = playlist.SongsPaths });
            }
            Debug.WriteLine("Playlst3");
            //UI 

            stkNoPlaylists.Visibility = (playlistsAll.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            AllPlaylistGroupedCollection.Visibility = (playlistsAll.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void btnNewPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Shows Grid

        private async void LoadAllShows()
        {
            //Load All Shows
            var currentset = await SettingsLoader.LoadSettingsAsync();
            var shows = currentset.Shows;
            showsAll.Clear();

            foreach (var show in shows)
            {
                Debug.WriteLine("SHOW BEING ADDED IS :" + show.Name);
                showsAll.Add(new Show { Name = show.Name, AddedSeasons = show.AddedSeasons, Creators = show.Creators, ShowID = show.ShowID, Crew = show.Crew, Description = show.Description, Genre = show.Genre, Poster = show.Poster, ReleaseDate = show.ReleaseDate, SeasonCount = show.SeasonCount, Tags = show.Tags });
            }
            Debug.WriteLine("Show3");
            //UI 
            stkNoShow.Visibility = (showsAll.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            AllShowsGroupedCollection.Visibility = (showsAll.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void btnNewShow_Click(object sender, RoutedEventArgs e)
        {

        }

        #endregion

        #region Settings
        private async void LoadSettings()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            chkIncludeSubDirectories.Checked -= chkIncludeSubDirectories_Checked;
            chkIncludeSubDirectories.Unchecked -= chkIncludeSubDirectories_Checked;

            chkIncludeSubDirectories.IsChecked = currentSettings.IncludeSubDirMusLib;

            // Resubscribe after state is updated
            chkIncludeSubDirectories.Checked += chkIncludeSubDirectories_Checked;
            chkIncludeSubDirectories.Unchecked += chkIncludeSubDirectories_Checked;

        }
        #endregion
        
        #endregion

        private void btnGenericFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void AllPlaylistGroupedCollection_playlistcollectionchanged()
        {

        }
    }
}

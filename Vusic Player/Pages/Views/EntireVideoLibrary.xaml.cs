using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
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
        ObservableCollection<VideoProgress> searchresults = new();

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
            var folderMappings = new (bool IsEnabled, string Path)[]
    {
    (currentSettings.IsDocumentEnabled, paths.Documents),
    (currentSettings.IsDownloadsEnabled, paths.Downloads),
    (currentSettings.IsPicturesEnabled, paths.Pictures),
    (currentSettings.IsVideosEnabled, paths.Videos),
    (currentSettings.IsMusicEnabled, paths.Music),
    };

            foreach (var (isEnabled, path) in folderMappings)
            {
                if (isEnabled)
                {
                    foldersListOpened.Add(new FoldersListOpened { FolderPath = path, IsHighLevelFolder = true, FolderName = Path.GetFileName(path), isChecked = true });
                }
            }
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
            }

            AllAvailableVideos.Clear();
            await LoadAllFilesAsync();
            stkLoading.Visibility = Visibility.Collapsed;
        }

        private async Task LoadAllFilesAsync()
        {
            stkLoading.Visibility = Visibility.Visible;
            await Task.Yield(); // Allow spinner to draw

            bool includeSubDirs = chkIncludeSubDirectories.IsChecked == true;

            var excludedPaths = foldersListOpened
                .Where(f => !f.isChecked && !string.IsNullOrWhiteSpace(f.FolderPath))
                .Select(f => f.FolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar)
                .ToList();

            var activeFolders = foldersListOpened
                .Where(f => f.isChecked && !string.IsNullOrWhiteSpace(f.FolderPath))
                .Select(f => f.FolderPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar)
                .ToList();

            var rawFiles = FilesInDatabase.rawSongs;

            // 1. Background filtering
            var matchingVideos = await Task.Run(() =>
            {
                var resultList = new List<SongModel>();
                var seenPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                var validExtensions = new HashSet<string>(VideoExtensions.List, StringComparer.OrdinalIgnoreCase);

                foreach (var raw in rawFiles)
                {
                    if (string.IsNullOrEmpty(raw.FilePath)) continue;
                    if (!validExtensions.Contains(Path.GetExtension(raw.FilePath))) continue;
                    if (excludedPaths.Any(ex => raw.FilePath.StartsWith(ex, StringComparison.OrdinalIgnoreCase))) continue;

                    bool isMatch = includeSubDirs
                        ? activeFolders.Any(af => raw.FilePath.StartsWith(af, StringComparison.OrdinalIgnoreCase))
                        : activeFolders.Any(af => string.Equals(Path.GetDirectoryName(raw.FilePath), af.TrimEnd(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase));

                    if (isMatch && seenPaths.Add(raw.FilePath))
                    {
                        resultList.Add(new SongModel
                        {
                            Title = raw.Title,
                            FilePath = raw.FilePath,
                            SongDuration = raw.SongDuration,
                            Genre = raw.Genre
                        });
                    }
                }
                return resultList;
            });

            // 2. DETACH the control to prevent UI thrashing
            AllVideosGroupedCollection.ItemsSource = null!;

            // 3. Clear and fill the collection instantly
            AllAvailableVideos.Clear();
            foreach (var item in matchingVideos)
            {
                AllAvailableVideos.Add(item);
            }

            // 4. RE-ATTACH: GroupedCollectionView processes the entire list in ONE single pass
            AllVideosGroupedCollection.ItemsSource = AllAvailableVideos;

            txtVideoCount.Text = $"• {AllAvailableVideos.Count} {(AllAvailableVideos.Count == 1 ? "item" : "items")}";
            grdEmptyLibrary.Visibility = (AllAvailableVideos.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            AllVideosGroupedCollection.Visibility = (AllAvailableVideos.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
            stkLoading.Visibility = Visibility.Collapsed;
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
            if (sender is ToggleButton tgl && tgl.DataContext is FoldersListOpened folder)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                UserDataPaths paths = UserDataPaths.GetDefault();
                bool isChecked = tgl.IsChecked ?? false;

                // 1. Update in-memory state
                folder.isChecked = isChecked;

                // 2. Persist to settings
                if (folder.IsHighLevelFolder)
                {
                    switch (folder.FolderPath)
                    {
                        case var p when p == paths.Documents: currentSettings.IsDocumentEnabled = isChecked; break;
                        case var p when p == paths.Downloads: currentSettings.IsDownloadsEnabled = isChecked; break;
                        case var p when p == paths.Pictures: currentSettings.IsPicturesEnabled = isChecked; break;
                        case var p when p == paths.Videos: currentSettings.IsVideosEnabled = isChecked; break;
                        case var p when p == paths.Music: currentSettings.IsMusicEnabled = isChecked; break;
                    }
                }
                else
                {
                    var saved = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.FolderPath);
                    if (saved != null)
                    {
                        saved.isChecked = isChecked;
                    }
                }

                await SettingsLoader.SaveSettingsAsync(currentSettings);

                // 3. Re-run scan to honor exclusions and checked states
                await LoadAllFilesAsync();
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

                AllAvailableVideos.Clear();
                await LoadAllFilesAsync();
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

        private async void chkIncludeSubDirectories_Checked(object sender, RoutedEventArgs e)
        {
            //Check box for including sub-directories
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            stkLoading.Visibility = Visibility.Visible;
            if (chkIncludeSubDirectories.IsChecked == true)
            {
                currentSettings.IncludeSubDirMusLib = true;
            }
            else
            {
                currentSettings.IncludeSubDirMusLib = false;
            }
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
            if (recentVideo.Count == 0)
            {
                grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                if (stkDisabledHistory.Visibility == Visibility.Collapsed)
                {
                    stkEmptyHistory.Visibility = Visibility.Visible;
                }
                btnClearHistory.IsEnabled = false;
                btnPlayAll.IsEnabled = false;
                asbRecents.IsEnabled = false;

            }
            else
            {
                btnClearHistory.IsEnabled = true;
                btnPlayAll.IsEnabled = true;
                asbRecents.IsEnabled = true;

                grdViewAllRecentMusic.Visibility = Visibility.Visible;
                stkEmptyHistory.Visibility = Visibility.Collapsed;
            }
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
                        tempTransfer.Add(new SongModel { Title = existingvideo.Title, SongDuration = existingvideo.SongDuration, FilePath = existingvideo.FilePath });
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

        private async void btnDisableHistory_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsVideoHistoryDisabled = true;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            btnDisableHistory.Visibility = Visibility.Collapsed;
            asbRecents.Visibility = Visibility.Collapsed;
            chkSelect.Visibility = Visibility.Collapsed;
            grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
            stkDisabledHistory.Visibility = Visibility.Visible;
            stkEmptyHistory.Visibility = Visibility.Collapsed;
            btnPlayAll.Visibility = Visibility.Collapsed;
            btnClearHistory.Visibility = Visibility.Collapsed;
        }

        private void asbRecents_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender is AutoSuggestBox asb)
            {
                if (asb.Text == "")
                {
                    searchresults.Clear();
                    asb.ItemsSource = null;
                    grdNoSearchResults.Visibility = Visibility.Collapsed;

                    grdViewAllRecentMusic.ItemsSource = recentVideo;
                    asb.ItemsSource = null;
                    grdViewAllRecentMusic.Visibility = Visibility.Visible;
                }
                if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
                {

                    searchresults.Clear();
                    var rawQuery = asb.Text.Trim();
                    // 1. Extract time components
                    var minMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:min|m)", RegexOptions.IgnoreCase);
                    var secMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:sec|s)", RegexOptions.IgnoreCase);

                    int searchSeconds = 0;
                    if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
                    if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);


                    var textQuery = rawQuery;
                    if (minMatch.Success) textQuery = textQuery.Replace(minMatch.Value, "");
                    if (secMatch.Success) textQuery = textQuery.Replace(secMatch.Value, "");
                    textQuery = textQuery.Trim().ToLower();

                    // 3. Filter the list
                    var results = recentVideo.Where(s =>
                    {
                        // Check if any text matches (only if textQuery isn't empty)
                        bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                            (s.FileName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                            (s.FolderName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)
                        );

                        return textMatch;
                    })
                    .OrderByDescending(s => s.FileName?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
                    .ThenBy(s => s.FileName)
                    .ToList();

                    if (results.Any())
                    {
                        asb.ItemsSource = null;
                        foreach (var item in results)
                        {
                            searchresults.Add(item);
                        }
                        grdViewAllRecentMusic.ItemsSource = searchresults;
                    }
                    else
                    {
                        var noresult = new List<string>();
                        noresult.Add("No matches found!");
                        asb.ItemsSource = null;
                        asb.ItemsSource = noresult;
                    }
                }
            }
        }

        private async void asbRecents_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (sender is AutoSuggestBox asb)
            {
                searchresults.Clear();
                var query = asb.Text.ToLower().Trim();
                var minMatch = Regex.Match(query, @"(\d+)\s*(?:min|m)");
                var secMatch = Regex.Match(query, @"(\d+)\s*(?:sec|s)");
                int searchSeconds = 0;
                if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
                if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);
                var results = recentVideo.Where(s =>
                (s.FileName != null && s.FileName.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (s.FolderName != null && s.FolderName.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase))
             ).OrderByDescending(s =>
        s.FileName?.StartsWith(query, StringComparison.OrdinalIgnoreCase) == true)
                        .ThenBy(s => s.FileName)
                        .ToList();

                if (results.Any())
                {
                    foreach (var item in results)
                    {
                        searchresults.Add(item);
                    }
                    grdViewAllRecentMusic.ItemsSource = searchresults;
                    //       lstViewQueue.LoadMedia(searchresults, Frame);
                }
                else
                {
                    grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                    if (recentVideo.Count != 0)
                    {
                        grdNoSearchResults.Visibility = Visibility.Visible;
                        await Task.Delay(200);
                        frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
                    }

                }
            }
        }

        private async void btnEnableHistory_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsVideoHistoryDisabled = false;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            btnDisableHistory.Visibility = Visibility.Visible;
            asbRecents.Visibility = Visibility.Visible;
            chkSelect.Visibility = Visibility.Visible;
            grdViewAllRecentMusic.Visibility = Visibility.Visible;
            stkDisabledHistory.Visibility = Visibility.Collapsed;
            btnPlayAll.Visibility = Visibility.Visible;
            btnClearHistory.Visibility = Visibility.Visible;
            LoadHistory();
        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked ?? false)
                grdViewAllRecentMusic.SelectAll();
            else
                grdViewAllRecentMusic.ClearSelection();
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = chkSelect.IsChecked ?? false;

            grdViewAllRecentMusic.GridSelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnRemoveFromHistory_Click(object sender, RoutedEventArgs e)
        {
            selectMoreOptions.Visibility = Visibility.Collapsed;

            grdViewAllRecentMusic.RemoveSelection();
        }

        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            asbRecents.Text = "";
            grdViewAllRecentMusic.Focus(FocusState.Programmatic);
            asbRecents.ItemsSource = null;
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
                playlistsAll.Add(new PlaylistItem { PlaylistId = playlist.PlaylistId, PlaylistName = playlist.PlaylistName, Thumbnail = playlist.Thumbnail, SongsPaths = playlist.SongsPaths });
            }
            Debug.WriteLine("Playlst3");
            //UI 

            stkNoPlaylists.Visibility = (playlistsAll.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            AllPlaylistGroupedCollection.Visibility = (playlistsAll.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
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
                    string baseName = PlInstance.PlaylistName.Trim();

                    if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                    string finalName = baseName;
                    int counter = 1;
                    while (currentSettings.SavedPlaylists.Any(p =>
                        string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalName = $"{baseName} ({counter++})";
                    }
                    var newplaylist = new PlaylistItem { PlaylistName = finalName, PlaylistGenre = PlInstance.Genre, plthumb = PlInstance.PlCover, PlaylistId = PlInstance.PlaylistID, DateCreation = PlInstance.CreationDate, SongsPaths = new HashSet<string>(PlInstance.MediaPaths), Thumbnail = PlInstance.Thumbnail };
                    Debug.WriteLine(newplaylist.PlaylistName);
                    PlInstance.PlaylistsMaster.Add(newplaylist);

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

        private void OceanContentDialog_PrimaryRequested()
        {
            PlaylistCreation.CallPlaylistCreation();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }


        #endregion

        #region Shows Grid
        //PENDING, INCLUDE SHOW TEMPLATE IN GROUPED COLLECTION MODEL CONTROL
        private async void LoadAllShows()
        {
            //Load All Shows
            var currentset = await SettingsLoader.LoadSettingsAsync();
            var shows = currentset.Shows;
            showsAll.Clear();

            foreach (var show in shows)
            {
                Debug.WriteLine("SHOW BEING ADDED IS :" + show.Name);
                showsAll.Add(new Show { Name = show.Name, AddedSeasons = show.AddedSeasons, Creators = show.Creators, ShowID = show.ShowID, Crew = show.Crew, Description = show.Description, Genre = show.Genre, Poster = show.Poster, ReleaseDate = show.ReleaseDate, ReleaseDateString = show.ReleaseDate.ToString("dd MMMM yyyy"), SeasonCountString = $"• {show.SeasonCount} {(show.SeasonCount == 1 ? "season" : "seasons")}", SeasonCount = show.SeasonCount, Tags = show.Tags });
            }
            Debug.WriteLine("Show3");
            //UI 
            stkNoShow.Visibility = (showsAll.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            AllShowsGroupedCollection.Visibility = (showsAll.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
        }
        ShowCreationValues Instance => ShowCreationValues.Instance;
        PlaylistCreationValues PlInstance => PlaylistCreationValues.Instance;

        private void btnNewShow_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Show Model", "Create", "", "Cancel", OceanDialogWindow.ContentType.ShowModel, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "", "", "", "", "", new PlaylistItem(), false, false);
            Debug.WriteLine("BTNNEWSHOW");
            OceanContentDialog.PrimaryRequested += (async () =>
            {
                var newshow = new Show { Name = Instance.ShowName, Description = Instance.Description, Genre = Instance.Genre, Creators = Instance.Creators, Crew = Instance.Cast, Directory = Instance.Directory, ShowID = Instance.ShowID, Tags = Instance.Tags, Poster = Instance.PosterPath };
                Debug.WriteLine(newshow.Name);
                Instance.ShowsMaster.Add(newshow);
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                currentSettings.Shows.Add(newshow);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                OceanContentDialog.HideDlg();
                MainWindow.ShowWindow();
            });
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



        private void AllPlaylistGroupedCollection_playlistcollectionchanged()
        {
            stkNoPlaylists.Visibility = (playlistsAll.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            AllPlaylistGroupedCollection.Visibility = (playlistsAll.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}

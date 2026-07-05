using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EntireMusicLibrary : Page
    {
        public EntireMusicLibrary()
        {
            InitializeComponent();
            stkLoading.Visibility = Visibility.Visible;
            LoadFolders();
            //        LoadDummy();
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string str && str == "EntireHistory")
            {
                ToggleButton[] musicButtons = { btnAllMusic, btnPlaylists, btnArtists, btnAlbums, btnGenres, btnHistory };
                foreach (var btn in musicButtons)
                {
                    btn.IsChecked = false;
                }

                btnHistory.IsChecked = true;
                grdAllMusic.Visibility = Visibility.Collapsed;
                grdGenres.Visibility = Visibility.Collapsed;
                grdPlaylists.Visibility = Visibility.Collapsed;
                grdAlbums.Visibility = Visibility.Collapsed;
                grdArtists.Visibility = Visibility.Collapsed;
                grdAllMusicHistory.Visibility = Visibility.Visible;
                LoadHistory();
            }
            else if (e.Parameter is string str2 && str2 == "Videos")
            {
                txtHistoryHeading.Text = "History of videos you've watched on this app";
                btnAllMusic.Content = "All Videos";
                btnHistory.Content = "Video History";
                txtAllMusicHeader.Text = "All videos across your libraries";
                AllMusicGroupedCollection.SearchPlaceHolderText = "Search for videos...";
                btnAlbums.Visibility = Visibility.Collapsed;
                btnArtists.Visibility = Visibility.Collapsed;
                btnGenres.Visibility = Visibility.Collapsed;
            }
            base.OnNavigatedTo(e);
        }
        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            var selectedBtn = sender as ToggleButton;
            if (selectedBtn == null) return;
            // 1. Define the list of buttons in this group
            ToggleButton[] musicButtons = { btnAllMusic, btnPlaylists, btnArtists, btnAlbums, btnGenres, btnHistory };

            foreach (var btn in musicButtons)
            {
                // 2. Uncheck everything that isn't the button we just clicked
                if (btn != selectedBtn)
                {
                    btn.IsChecked = false;
                }
            }

            selectedBtn.IsChecked = true;
            string category = selectedBtn.Content.ToString()!;
            grdAllMusic.Visibility = Visibility.Collapsed;
            grdGenres.Visibility = Visibility.Collapsed;
            grdPlaylists.Visibility = Visibility.Collapsed;
            grdAlbums.Visibility = Visibility.Collapsed;
            grdArtists.Visibility = Visibility.Collapsed;
            grdAllMusicHistory.Visibility = Visibility.Collapsed;
            if (category == "Music History" || category == "Video History")
            {
                grdAllMusicHistory.Visibility = Visibility.Visible;
                LoadHistory();
            }
            else if (category == "All Music" || category == "All Videos")
            {
                grdAllMusic.Visibility = Visibility.Visible;
            }
        }
        ObservableCollection<RecentMusicModel> recentMusics = new();
        private async void LoadHistory()
        {
            recentMusics.CollectionChanged += RecentMusics_CollectionChanged;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recentmusic = currentSettings.RecentMusic;

            foreach (var item in recentmusic)
            {
                recentMusics.Add(new RecentMusicModel
                {
                    FolderName = new DirectoryInfo(
                            Path.GetDirectoryName(item.SongPath) ?? string.Empty
                        ).Name,
                    SongName = item.SongName,
                    SongPath = item.SongPath,
                    PlayCountDisplay = $"{item.PlayCount} {(item.PlayCount == 1 ? "time" : "times")}",
                    LastPlayed = item.LastPlayed
                });

            }
            foreach (var item in recentMusics)
            {
                var task = Task.Run(async () =>
                {
                    var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(item.SongPath);
                    Debug.WriteLine("The thumbnail path is " + thumb);
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        try
                        {
                            var bitmap = new BitmapImage();
                            using (var stream = File.OpenRead(thumb))
                            {
                                await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                            }
                            item.Thumbnail = bitmap;

                            File.Delete(thumb);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("An unexpected error occured: " + ex.Message);
                        }
                    });

                });

            }
            grdViewAllRecentMusic.ItemsSource = recentMusics;
            if (recentMusics.Count == 0)
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
        }

        private void RecentMusics_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (recentMusics.Count == 0)
            {
                grdViewAllRecentMusic.Visibility = Visibility.Collapsed;
                stkEmptyHistory.Visibility = Visibility.Visible;
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

        private void btnAllMusic_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGlyph_Click(object sender, RoutedEventArgs e)
        {

        }

        private void hypTitle_Click(object sender, RoutedEventArgs e)
        {

        }

        private void hypArtist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void hypAlbum_Click(object sender, RoutedEventArgs e)
        {

        }
        public async Task<List<StorageFile>> GetAllFilesRecursivelyAsync(StorageFolder folder)
        {
            var fileList = new List<StorageFile>();

            // 1. Get surface files
            var surfaceFiles = await folder.GetFilesAsync();
            fileList.AddRange(surfaceFiles);

            // 2. Loop through subfolders and recurse
            var subfolders = await folder.GetFoldersAsync();
            foreach (var subfolder in subfolders)
            {
                var subfolderFiles = await GetAllFilesRecursivelyAsync(subfolder);
                fileList.AddRange(subfolderFiles);
            }

            return fileList;
        }
        public ObservableCollection<SongModel> AllAvailableSongs = new ObservableCollection<SongModel>();
        private async Task LoadAllFiles(List<string> searchPaths)
        {
            List<StorageFile> allFoundFiles = new List<StorageFile>();

            foreach (var path in searchPaths)
            {
                try
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                    var queryOptions = new QueryOptions(CommonFileQuery.OrderByName, AudioExtensions.List);
                    queryOptions.FolderDepth = FolderDepth.Deep; // <--- This does the recursion safely!

                    var queryResult = folder.CreateFileQueryWithOptions(queryOptions);
                    var files = await queryResult.GetFilesAsync();
                    foreach (var file in files)
                    {
                        // No extension check needed here anymore, QueryOptions filtered them already!
                        Debug.WriteLine(file.Path + " is found being added to allFoundFiles");
                        allFoundFiles.Add(file);
                    }

                    Debug.WriteLine($"Finished processing path: {path}. Current total in list: {allFoundFiles.Count}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"CRASHED INSIDE FOREACH LOOP: {ex.GetType().Name} - {ex.Message}");
                }
            }

            Debug.WriteLine($"Total files collected in list: {allFoundFiles.Count}");
            Debug.WriteLine($"Total files collected in list: {allFoundFiles.Count}");
            if (allFoundFiles.Count > 0)
            {
                Debug.WriteLine("IT IS FOUND");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var favourites = currentSettings.Favourites;
                foreach (var file in allFoundFiles)
                {
                    try
                    {
                        Debug.WriteLine("EVALUATING: " + file.Path);
                        var tagFile = TagLib.File.Create(file.Path);
                        var tag = tagFile.Tag;
                        var existingfav = favourites.FirstOrDefault(p => p.FilePath == file.Path);
                        bool isFav = false;
                        if (existingfav != null)
                        {
                            isFav = true;
                        }
                        string title = string.IsNullOrWhiteSpace(tag.Title)
     ? Path.GetFileNameWithoutExtension(file.Path)
     : tag.Title;
                        string album = string.IsNullOrWhiteSpace(tag.Album)
    ? "Unknown Album"
    : tag.Album;
                        string artist = string.IsNullOrWhiteSpace(string.Join("; ", tag.AlbumArtists))
  ? "Unknown Artist"
  : string.Join("; ", tag.AlbumArtists);
                        var song = new SongModel { Title = title, AlbumName = album, Artist = artist, FilePath = file.Path, SongDuration = tagFile.Properties.Duration, IsFavourite = isFav, Glyph = "\uEC4F" };
                        AllAvailableSongs.Add(song);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("ERROR: " + ex.Message);
                    }
                }
            }
        }
        // Install-Package TagLibSharp
        private async Task ScanAllFoldersAsync(List<string> searchPaths)
        {
            var extensions = AudioExtensions.List;

            await Task.Run(() =>
            {
                var discoveredSongs = new List<SongModel>();
                int batchSize = 50;

                foreach (var path in searchPaths)
                {
                    if (!Directory.Exists(path)) continue;

                    IEnumerable<string> files;
                    try
                    {
                        files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
                    }
                    catch { continue; }

                    foreach (var file in files)
                    {
                        string ext = Path.GetExtension(file).ToLower();
                        if (!extensions.Contains(ext)) continue;

                        try
                        {
                            // Use TagLib to read metadata completely offline on the background thread
                            using (var taggedFile = TagLib.File.Create(file))
                            {
                                var props = taggedFile.Tag;

                                var song = new SongModel
                                {
                                    Title = string.IsNullOrEmpty(props.Title)
                                        ? Path.GetFileNameWithoutExtension(file)
                                        : props.Title,
                                    Artist = string.IsNullOrEmpty(props.FirstPerformer)
                                        ? "Unknown"
                                        : props.FirstPerformer,
                                    AlbumName = props.Album ?? "",
                                    SongDuration = taggedFile.Properties.Duration,
                                    FilePath = file
                                };

                                discoveredSongs.Add(song);
                            }

                            if (discoveredSongs.Count >= batchSize)
                            {
                                var batchToPush = discoveredSongs.ToList();
                                discoveredSongs.Clear();

                                _ = DispatcherQueue.EnqueueAsync(() =>
                                {
                                    foreach (var item in batchToPush)
                                    {
                                        AllAvailableSongs.Add(item);
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(ex.Message, "EntireMusicLibrarySearchFiles", Logger.LogLevelType.Error);
                        }
                    }
                }

                if (discoveredSongs.Count > 0)
                {
                    _ = DispatcherQueue.EnqueueAsync(() =>
                    {
                        foreach (var item in discoveredSongs)
                        {
                            AllAvailableSongs.Add(item);
                        }
                    });
                }
            });
        }
        private void btnFavourite_Click(object sender, RoutedEventArgs e)
        {

        }
        private async void LoadItemsFromFolder(string path)
        {

        }
        private async void LoadFolders()
        {
            UserDataPaths paths = UserDataPaths.GetDefault();

            // Assigning the system paths to the Tag property
            btnMusic.Tag = paths.Music;
            btnPictures.Tag = paths.Pictures;
            btnVideos.Tag = paths.Videos;
            btnDocuments.Tag = paths.Documents;
            btnDownloads.Tag = paths.Downloads;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            foldersListOpened.Clear();
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Music, FolderName = "Music Folder", isChecked = true });
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Pictures, FolderName = "Pictures Folder", isChecked = true });
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Videos, FolderName = "Videos Folder", isChecked = true });
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Documents, FolderName = "Documents Folder", isChecked = true });
            foldersListOpened.Add(new FoldersListOpened { FolderPath = paths.Downloads, FolderName = "Downloads Folder", isChecked = true });

            foreach (var item in currentSettings.SavedFoldersOpened)
            {
                item.FolderName = Path.GetFileName(item.FolderPath);
                foldersListOpened.Add(item);
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
            grdViewFolders.ItemsSource = foldersListOpened;
            List<string> fpaths = new();
            foreach (var item in foldersListOpened)
            {
                fpaths.Add(item.FolderPath);
            }
            AllAvailableSongs.Clear();
            await LoadAllFiles(fpaths);
            stkLoading.Visibility = Visibility.Collapsed;
        }
        ObservableCollection<FoldersListOpened> foldersListOpened = new();
        private void LoadDummy()
        {
            var list = new ObservableCollection<SongModel>();
            list.Add(new SongModel { Title = "The Best", Artist = "Conan Gray", AlbumName = "Wishbone Deluxe" });
            list.Add(new SongModel { Title = "Vodka Cranberry", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "This Song", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Nauseous", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Actor", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Care", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Conell", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "Class Clown", Artist = "Conan Gray", AlbumName = "Wishbone" });
            list.Add(new SongModel { Title = "The Exit", Artist = "Conan Gray", AlbumName = "Superache" });
            list.Add(new SongModel { Title = "Memories", Artist = "Conan Gray", AlbumName = "Superache" });
            list.Add(new SongModel { Title = "Footnote", Artist = "Conan Gray", AlbumName = "Superache" });
            list.Add(new SongModel { Title = "Astroonmy", Artist = "Conan Gray", AlbumName = "Superache" });
            list.Add(new SongModel { Title = "Movies", Artist = "Conan Gray", AlbumName = "Superache" });
            AllMusicGroupedCollection.ItemsSource = list;
        }
        private async void LoadItems(List<string> paths)
        {
            var userPaths = UserDataPaths.GetDefault();
            var ObservableCollectioln = AllAvailableSongs;

            var extensions = AudioExtensions.List;

            foreach (var path in paths)
            {
                if (!Directory.Exists(path))
                    continue;

                IEnumerable<string> files;

                try
                {
                    files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories);
                    Debug.WriteLine("check1");
                }
                catch
                {
                    continue;
                }

                foreach (var file in files)
                {
                    string ext = Path.GetExtension(file).ToLower();

                    if (!extensions.Contains(ext))
                        continue;

                    try
                    {
                        StorageFile storageFile =
                            await StorageFile.GetFileFromPathAsync(file);

                        MusicProperties props =
                            await storageFile.Properties.GetMusicPropertiesAsync();
                        Debug.WriteLine("check2");

                        var song = new SongModel
                        {
                            Title = string.IsNullOrEmpty(props.Title)
                                ? Path.GetFileNameWithoutExtension(file)
                                : props.Title,

                            Artist = string.IsNullOrEmpty(props.Artist)
                                ? "Unknown"
                                : props.Artist,

                            AlbumName = props.Album ?? "",

                        };

                        await DispatcherQueue.EnqueueAsync(() =>
                        {
                            Debug.WriteLine("check3");

                            ObservableCollectioln.Add(song);

                        });

                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, "EntireMusicLibrarySearchFiles", Logger.LogLevelType.Error);
                    }
                }

            }
            AllMusicGroupedCollection.ItemsSource = ObservableCollectioln;
        }
        private async void btnAddFolders_Click(object sender, RoutedEventArgs e)
        {
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
                var check = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                if (check == null)
                {
                    currentSettings.SavedFoldersOpened.Add(new FoldersListOpened { FolderPath = folder.Path, isChecked = true });
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                }
            }
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void ToggleButton_Checked1(object sender, RoutedEventArgs e)
        {
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

            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)

            {
                foldersListOpened.Remove(tgl);
                var currentse = await SettingsLoader.LoadSettingsAsync();
                if (tgl.FolderPath is string str)
                {
                    var folder = currentse.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == str);
                    if (folder != null)
                    {
                        currentse.SavedFoldersOpened.Remove(folder);
                    }
                    await SettingsLoader.SaveSettingsAsync(currentse);
                }
            }
        }

        private async void btnGenericFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tgl)
            {
                if (tgl.IsChecked == true)
                {
                    if (tgl.DataContext is FoldersListOpened folder)
                    {
                        var folderpath = folder.FolderPath;
                        var list = new List<string>();
                        list.Add(folderpath);
                        await LoadAllFiles(list);
                    }
                }
            }
            //if (sender is ToggleButton tgl)
            //{

            //    Debug.WriteLine("add");
            //    if (tgl.DataContext is FoldersListOpened fol)
            //    {
            //        Debug.WriteLine("add2");

            //        var path = fol.FolderPath;
            //        Debug.WriteLine(path);

            //               AllAvailableSongs.Clear();
            //        List<string> paths = new();
            //        paths.Add(path);
            //        LoadItems(paths);
            //    }
            //    //        await ScanAllFoldersAsync(paths);
            //    //        AllMusicGroupedCollection.ItemsSource = AllAvailableSongs;

            //    //    }
            //    //}

        }

        private void btnGenericFolder_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn && btn.DataContext is FoldersListOpened folder)
            {
                if (btn.IsChecked == false)
                {
                    var folderpath = folder.FolderPath;
                    string folderWithBackslash = folderpath.EndsWith(Path.DirectorySeparatorChar.ToString())
                ? folderpath
                : folderpath + Path.DirectorySeparatorChar;

                    // 2. Use .Any() to see if AT LEAST ONE song is in this folder
                    var songsInThisFolder = AllAvailableSongs.Where(p =>
            p.FilePath.StartsWith(folderWithBackslash, StringComparison.OrdinalIgnoreCase))
            .ToList();

                    // Now you have the songs! For example, if you want to remove them:
                    foreach (var song in songsInThisFolder)
                    {
                        AllAvailableSongs.Remove(song);
                    }
                }
            }
        }

        private async void btnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recentmusic = currentSettings.RecentMusic;
            recentMusics.Clear();
            recentmusic.Clear();
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private void btnDisableHistory_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftRemoveFromRecentMusic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftGoToFileLocation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftPlayRecents_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftAddtoQueueRecentMusic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftViewFileInfo_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var recentmusic = currentSettings.RecentMusic;
            if (recentmusic.Count == 0) return;
            ObservableCollection<SongModel> tempTransfer = new();
            foreach (var item in recentmusic)
            {
                var file = await StorageFile.GetFileFromPathAsync(item.SongPath);
                var props = await file.Properties.GetMusicPropertiesAsync();
                string Title = props.Title;
                if (Title == "")
                {
                    Title = Path.GetFileNameWithoutExtension(file.Path);
                }
                string AlbumName = props.Album;
                string Artist = props.Artist;
                tempTransfer.Add(new SongModel { Title = Title, AlbumName = AlbumName, Artist = Artist, SongDuration = props.Duration, FilePath = file.Path });
            }
            QueueService.PlayMedia(tempTransfer, false, false);

        }
        ObservableCollection<RecentMusicModel> searchresults = new();

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (sender is AutoSuggestBox asb)
            {
                if (asb.Text == "")
                {
                    searchresults.Clear();
                    asb.ItemsSource = null;
                    grdNoSearchResults.Visibility = Visibility.Collapsed;

                    grdViewAllRecentMusic.ItemsSource = recentMusics;
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
                    var results = recentMusics.Where(s =>
                    {
                        // Check if any text matches (only if textQuery isn't empty)
                        bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                            (s.SongName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true) ||
                            (s.FolderName?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)
                        );

                        return textMatch;
                    })
                    .OrderByDescending(s => s.SongName?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
                    .ThenBy(s => s.SongName)
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
        private void btnCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            asbRecents.Text = "";
            grdViewAllRecentMusic.Focus(FocusState.Programmatic);
            asbRecents.ItemsSource = null;
        }
        private async void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
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
                var results = recentMusics.Where(s =>
                (s.SongName != null && s.SongName.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                (s.FolderName != null && s.FolderName.ToLower().Contains(query, StringComparison.OrdinalIgnoreCase))
             ).OrderByDescending(s =>
        s.SongName?.StartsWith(query, StringComparison.OrdinalIgnoreCase) == true)
                        .ThenBy(s => s.SongName)
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
                    if (recentMusics.Count != 0)
                    {
                        grdNoSearchResults.Visibility = Visibility.Visible;
                        await Task.Delay(200);
                        frmSearchResultsNOMATCH.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
                    }

                }
            }
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = chkSelect.IsChecked ?? false;

            grdViewAllRecentMusic.GridSelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked ?? false)
                grdViewAllRecentMusic.SelectAll();
            else
                grdViewAllRecentMusic.ClearSelection();
        }

        private void btnRemoveFromHistory_Click(object sender, RoutedEventArgs e)
        {
            selectMoreOptions.Visibility = Visibility.Collapsed;

            grdViewAllRecentMusic.RemoveSelection();
        }
    }
}

using CommunityToolkit.WinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.DirectoryServices;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.StartScreen;


namespace Vusic_Player.Pages.Views
{

    public sealed partial class FolderView : Page
    {
        FolderModel? currentFolder;
        public FolderView()
        {
            InitializeComponent();
        }
        ObservableCollection<FileItem> MainItems = new();
        private string GetFileOrFolderSizeInString(long bytes)
        {
            string[] units2 = { "Bytes", "KB", "MB", "GB", "TB" };
            double readableSize2 = bytes;
            int unitIndex2 = 0;

            // Loop to divide by 1024 until we find the right unit scale
            while (readableSize2 >= 1024 && unitIndex2 < units2.Length - 1)
            {
                readableSize2 /= 1024;
                unitIndex2++;
            }
            string finalSizeString2 = $"{readableSize2:0.##} {units2[unitIndex2]}";
            return finalSizeString2;
        }
        private async Task GetFiles(string folPath)
        {
            if (Directory.Exists(folPath))
            {
                MainItems.Clear();
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var favourites = currentSettings.Favourites;
                var folder = await StorageFolder.GetFolderFromPathAsync(folPath);
                var subFolders = await folder.GetFoldersAsync();

                foreach (var sub in subFolders)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(sub.Path);
                    var totalsize2 = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                    string[] units2 = { "Bytes", "KB", "MB", "GB", "TB" };
                    double readableSize2 = totalsize2;
                    int unitIndex2 = 0;

                    // Loop to divide by 1024 until we find the right unit scale
                    while (readableSize2 >= 1024 && unitIndex2 < units2.Length - 1)
                    {
                        readableSize2 /= 1024;
                        unitIndex2++;
                    }
                    string finalSizeString2 = $"{readableSize2:0.##} {units2[unitIndex2]}";
                    MainItems.Add(new FileItem
                    {
                        Path = sub.Path,
                        Thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/foldericon.png")),
                        Name = Path.GetFileName(sub.Path),
                        OpenContext = "Open Folder",
                        OpenContextGlyph = "\uE838",
                        isFolder = true,
                        FileHoverInfo = Path.GetFileName(sub.Path) + Environment.NewLine + "(Folder)" + Environment.NewLine + finalSizeString2,
                        FileInfoContext = "Folder Info",
                        VisibilityOfFileProperties = Visibility.Collapsed

                        //FilePath = sub.Path,
                        //IsAudioItem = false,
                        //FileTypeName = "Folder",
                        //Title = Path.GetFileName(folPath),
                    });
                }
                var files = await folder.GetFilesAsync();
                long totalsize = 0;
                int totalitemcount = 0;
                foreach (var file in files)
                {
                    FileInfo fileInfo = new FileInfo(file.Path);
                    string filesize = GetFileOrFolderSizeInString(fileInfo.Length);
                    if (fileInfo.Exists)
                    {
                        totalsize += fileInfo.Length;
                    }
                    string fileExtension = Path.GetExtension(file.Path).ToLower();

                    if (!VideoExtensions.List.Contains(fileExtension) && !AudioExtensions.List.Contains(fileExtension))
                    {
                        continue;
                    }
                    var colorbrush = new SolidColorBrush(Microsoft.UI.Colors.White);
                    //var glyph = "\uEC4F";
                    //if (VideoExtensions.List.Contains(fileExtension))
                    //{
                    //    glyph = "\uE8B2";
                    //}
                    //if (PlayerService.CurrentPlayingPath == file.Path)
                    //{
                    //    colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
                    //    if (PlayerService.Masterplayer!.IsPlaying)
                    //        glyph = "\uE769";
                    //    else
                    //    {
                    //        glyph = "\uE768";
                    //    }
                    //}
                    if (AudioExtensions.List.Contains(fileExtension))
                    {
                        var musicprops = await file.Properties.GetMusicPropertiesAsync();
                        var exist = favourites.FirstOrDefault(p => p.FilePath == file.Path);
                        bool isFav = false;
                        if (exist != null)
                        {
                            isFav = true;
                        }
                        totalitemcount += 1;
                        string durationText = musicprops.Duration.TotalHours >= 1
    ? musicprops.Duration.ToString(@"h\:mm\:ss")
    : musicprops.Duration.ToString(@"m\:ss");
                        string text = isFav ? "Remove from Favourites" : "Add to Favourites";
                        MainItems.Add(new FileItem
                        {
                            Path = file.Path,
                            Name = Path.GetFileNameWithoutExtension(file.Path),
                            Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(file.Path),
                            IsFavourite = isFav,
                            FavString = text,
                            FileHoverInfo = Path.GetFileName(file.Path) + Environment.NewLine + filesize + Environment.NewLine + "Length: " + durationText

                            //Title = Path.GetFileName(file.Path),
                            //Artist = musicprops.Artist,
                            //AlbumName = musicprops.Album,
                            //SongDuration = musicprops.Duration,
                            //IsFavourite = isFav,
                            //Glyph = glyph,
                            //TitleColor = colorbrush
                        });
                    }
                    else if (VideoExtensions.List.Contains(fileExtension))
                    {
                        var props = await file.Properties.GetVideoPropertiesAsync();
                        string durationText = props.Duration.TotalHours >= 1
    ? props.Duration.ToString(@"h\:mm\:ss")
    : props.Duration.ToString(@"m\:ss");
                        var exist = favourites.FirstOrDefault(p => p.FilePath == file.Path);
                        bool isFav = false;
                        if (exist != null)
                        {
                            isFav = true;
                        }
                        var newFileItem = new FileItem
                        {
                            Path = file.Path,
                            IsFavourite = isFav,
                            Name = Path.GetFileNameWithoutExtension(file.Path),
                            FileSize = fileInfo.Length,
                            FileCreationTime = fileInfo.CreationTime,
                            FileModifiedTime = fileInfo.LastWriteTime,
                            Extension = fileInfo.Extension,
                            FileHoverInfo = Path.GetFileName(file.Path) + Environment.NewLine + filesize + Environment.NewLine + "Length: " + durationText
                        };

                        MainItems.Add(newFileItem);
                        totalitemcount += 1;
                        //  var vidprops = await file.Properties.GetVideoPropertiesAsync();

                        Debug.WriteLine("Requested Path being sent is " + file.Path);
                        var task = Task.Run(async () =>
                        {
                            var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(file.Path);
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
                                    newFileItem.Thumbnail = bitmap;

                                    File.Delete(thumb);
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("An unexpected error occured: " + ex.Message);
                                }
                            });
                        });

                    }


                }
                string[] units = { "Bytes", "KB", "MB", "GB", "TB" };
                double readableSize = totalsize;
                int unitIndex = 0;

                // Loop to divide by 1024 until we find the right unit scale
                while (readableSize >= 1024 && unitIndex < units.Length - 1)
                {
                    readableSize /= 1024;
                    unitIndex++;
                }

                // Format the string to 2 decimal places
                // The "0.##" format automatically drops trailing zeros (e.g., 1.50 MB becomes 1.5 MB, 2.00 GB becomes 2 GB)
                string finalSizeString = $"{readableSize:0.##} {units[unitIndex]}";
                txtFolderInfo.Text = $"• {totalitemcount} {(totalitemcount == 1 ? "media item" : "media items")} | {finalSizeString} in size";


            }
        }
        private async void mnftAddToFavourites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileItem song)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                var pathtocheck = song.Path;
                if (pathtocheck == null) return;
                var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);
                if (existing != null)
                {
                    song.IsFavourite = false;
                    Favourites.Remove(existing);
                    song.FavString = "Add to Favourites";

                }
                else
                {
                    Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                    song.IsFavourite = true;
                    song.FavString = "Remove from Favourites";

                }


                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }
        public ObservableCollection<FolderModel> BreadcrumbItems { get; set; } = new();
        private async void btnFavourite_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is Grid rootGrid && btn.DataContext is FileItem song)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                var pathtocheck = song.Path;
                if (pathtocheck == null) return;
                var fillHeartIcon = rootGrid.FindName("HeartIcon") as FontIcon;
                if (fillHeartIcon == null) return;
                var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);
                if (existing == null)
                {
                    fillHeartIcon.Glyph = "\uEB52";
                    song.IsFavourite = true;
                    AnimateHeartFull(fillHeartIcon, true);
                    Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                    song.FavString = "Remove from Favourites";

                }
                else
                {
                    song.IsFavourite = false;
                    fillHeartIcon.Glyph = "\uEB51";
                    AnimateHeartFull(fillHeartIcon, false);
                    Favourites.Remove(existing);
                    song.FavString = "Add to Favourites";


                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);

            }
            // Favourite button click logic
        }
        private void AnimateHeartFull(FontIcon targetIcon, bool isBecomingFavorite)
        {
            // 1. Get the Visual and Compositor
            var visual = ElementCompositionPreview.GetElementVisual(targetIcon);
            var compositor = visual.Compositor;

            // 2. Setup Scale Animation (The "Pop")
            var scaleAnim = compositor.CreateScalarKeyFrameAnimation();
            scaleAnim.InsertKeyFrame(0.0f, 1.0f);
            scaleAnim.InsertKeyFrame(0.5f, 1.4f); // Pulse size
            scaleAnim.InsertKeyFrame(1.0f, 1.0f);
            scaleAnim.Duration = TimeSpan.FromMilliseconds(400);

            visual.CenterPoint = new Vector3((float)targetIcon.ActualWidth / 2, (float)targetIcon.ActualHeight / 2, 0);
            visual.StartAnimation("Scale.X", scaleAnim);
            visual.StartAnimation("Scale.Y", scaleAnim);

            // 3. Setup Color Animation (The "Fill")
            // Note: We animate the 'Brush.Color' of the visual
            var colorAnim = compositor.CreateColorKeyFrameAnimation();
            colorAnim.Duration = TimeSpan.FromMilliseconds(400);

            if (isBecomingFavorite)
            {
                colorAnim.InsertKeyFrame(0.0f, Colors.Gray);
                colorAnim.InsertKeyFrame(1.0f, Colors.Red);
            }
            else
            {
                colorAnim.InsertKeyFrame(0.0f, Colors.Red);
                colorAnim.InsertKeyFrame(1.0f, Colors.Gray);
            }

            // Create a Brush if one doesn't exist on the visual layer
            var brush = compositor.CreateColorBrush();
            visual.Properties.InsertColor("Color", isBecomingFavorite ? Colors.Red : Colors.Gray);

            // Start the color transition
            // Note: For FontIcon, it's often easier to just swap the Glyph 
            // and let the Composition color animation handle the tint.
            targetIcon.Foreground = new SolidColorBrush(isBecomingFavorite ? Colors.Red : Colors.Gray);
        }

        private void InitializeBreadcrumb(string path)
        {
            BreadcrumbItems.Clear();
            string[] segments = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
            string runningPath = "";

            foreach (var segment in segments)
            {
                // Reconstruct the full path incrementally for each folder level
                if (string.IsNullOrEmpty(runningPath))
                {
                    // Handles the drive root (e.g., "C:")
                    runningPath = segment.Contains(":") ? segment + Path.DirectorySeparatorChar : segment;
                }
                else
                {
                    runningPath = Path.Combine(runningPath, segment);
                }

                // Add the object containing both pieces of data
                BreadcrumbItems.Add(new FolderModel
                {
                    FolderName = segment,
                    FolderPath = runningPath
                });
            }

            brdcrumBarMain.ItemsSource = BreadcrumbItems;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is FolderModel folder && folder.FolderPath is string FolderPath)
            {
                if (Directory.Exists(FolderPath))
                {
                    prgLoading.Visibility = Visibility.Visible;
                    currentFolder = folder;
                    grdViewMain.ItemsSource = MainItems;
                    //txtFolderName.Text = Path.GetFileName(FolderPath);
                    txtFolderName.Text = folder.FolderName;
                    InitializeBreadcrumb(FolderPath);
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var folders = currentSettings.FoldersRecent;
                    var exist = folders.FirstOrDefault(p => p.FolderPath == FolderPath);
                    if (exist != null)
                    {
                        folders.Insert(0, exist);
                    }
                    else
                    {
                        folders.Add(new FolderModel { FolderName = folder.FolderName, FolderPath = FolderPath });
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    await GetFiles(FolderPath);
                    prgLoading.Visibility = Visibility.Collapsed;
                    btnRenameFolder.IsEnabled = true;
                    if (MainItems.Count == 0)
                    {
                        grdViewMain.Visibility = Visibility.Collapsed;
                        grdNoFiles.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        grdViewMain.Visibility = Visibility.Visible;
                        grdNoFiles.Visibility = Visibility.Collapsed;
                    }
                    //             bool hasVideo = MainItems.Any(song =>
                    //    !string.IsNullOrEmpty(song.FilePath) &&
                    //    Extensions.VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant())
                    //);
                    //             if (hasVideo)
                    //             {
                    //                 lstViewMain.VideoPlaylistUI();
                    //             }
                }
                else
                {
                    grdMissingDirectory.Visibility = Visibility.Visible;
                    rootGrid.Visibility = Visibility.Collapsed;
                }
            }
            base.OnNavigatedTo(e);
        }

        private void brdcrumBarMain_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (App.NavigationFrame != null)
            {
                if (args.Item is FolderModel folder)
                {
                    App.NavigationFrame.Navigate(typeof(FolderView), folder);
                }
            }
        }

        private void btnRenameFolder_Click(object sender, RoutedEventArgs e)
        {
            ifbRenameInfo.IsOpen = false;

            ttRenameFolder.IsOpen = true;
            txtRenameFolder.Text = txtFolderName.Text;
        }

        private void btnOpenFolderLoc_Click(object sender, RoutedEventArgs e)
        {
            if (currentFolder == null) return;
            if (Directory.Exists(currentFolder.FolderPath))
            {

                Process.Start("explorer.exe", $"\"{currentFolder.FolderPath}\"");
            }
        }
        private async void AddtoQueue()
        {
            var selecteditems = grdViewMain.SelectedItems.Cast<FileItem>();
            if (selecteditems != null)
            {
                foreach (var item in selecteditems)
                {

                    var storagefile = await StorageFile.GetFileFromPathAsync(item.Path);

                    string fileExtension = Path.GetExtension(item.Path).ToLower();

                    if (AudioExtensions.List.Contains(fileExtension))
                    {
                        var props = await storagefile.Properties.GetMusicPropertiesAsync();
                        QueueService.VusicQueue.Add(new SongModel { FilePath = item.Path, Title = item.Name, AlbumName = props.Album, Artist = props.Artist, SongDuration = props.Duration });
                        QueueService.VusicQueueNext.Add(new SongModel { FilePath = item.Path, Title = item.Name, AlbumName = props.Album, Artist = props.Artist, SongDuration = props.Duration });

                    }
                    else if (VideoExtensions.List.Contains(fileExtension))
                    {
                        var props = await storagefile.Properties.GetVideoPropertiesAsync();
                        QueueService.VusicQueue.Add(new SongModel { FilePath = item.Path, Title = item.Name, SongDuration = props.Duration });
                        QueueService.VusicQueueNext.Add(new SongModel { FilePath = item.Path, Title = item.Name, SongDuration = props.Duration });
                    }



                }
            }
        }
        private async void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            QueueService.VusicQueueNext.Clear();
            AddtoQueue();
            if (QueueService.VusicQueue.Count != 0)
            {
                var firstitem = QueueService.VusicQueueNext[0];
                if (firstitem.FilePath != null)
                {
                    PlayerService.OpenPath(firstitem.FilePath);
                    QueueService.VusicQueueNext.Remove(firstitem);
                }
            }
            foreach (var item in QueueService.VusicQueueNext)
            {
                Debug.WriteLine("ot: " + item.FilePath);
            }

        }

        private void btnRenameFiles_Click(object sender, RoutedEventArgs e)
        {
            ttRenameFiles.IsOpen = true;
            ifbRenameFiles.IsOpen = false;

            if (grdViewMain.SelectedItems.Count == 1)
            {
                var item = grdViewMain.SelectedItem;
                if (item is FileItem file)
                {
                    txtRenameFiles.Text = file.Name;
                }
                rdIncludeNumberAtEnd.Visibility = Visibility.Collapsed;
                rdIncludeNumberAtStart.Visibility = Visibility.Collapsed;
            }
            else
            {
                rdIncludeNumberAtEnd.Visibility = Visibility.Visible;
                rdIncludeNumberAtStart.Visibility = Visibility.Visible;
                txtRenameFiles.Text = "";
            }
        }

        private void btnMoveFiles_Click(object sender, RoutedEventArgs e)
        {
            ttMoveFiles.IsOpen = true;
        }
        ObservableCollection<PlaylistItem> playlistsaddto = new();
        private async void btnAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            playlistsaddto.Clear();
            if (grdViewMain.SelectedItems.Count > 0)
            {
                ttAddtoPlaylist.IsOpen = true;
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var playlists = currentSettings.SavedPlaylists;
                foreach (var playlist in playlists)
                {

                    playlistsaddto.Add(new PlaylistItem { PlaylistName = playlist.PlaylistName, PlaylistId = playlist.PlaylistId });
                }
                lstAddToPlaylists.ItemsSource = playlistsaddto;
            }
        }

        private void btnDeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "The selected items will be sent to the Recycle Bin. You can restore them from the Recycle Bin if needed.");
            UnsubscribeAllEventsOceanDialog();

            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
        }

        private void OceanContentDialog_PrimaryRequested()
        {

            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
            DeleteFiles();
            //OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;

        }
        private async Task DeleteSingleFileAsync(string path)
        {
            try
            {
                var storagefile = await StorageFile.GetFileFromPathAsync(path);
                await storagefile.DeleteAsync(StorageDeleteOption.Default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An unexpected error occured. Check log page for more details");
                Logger.Log(ex.Message, "DeleteFileFolderPage", Logger.LogLevelType.Error);
            }
        }
        private async Task DeleteSingleFolderAsync(string path)
        {
            try
            {
                var storagefolder = await StorageFolder.GetFolderFromPathAsync(path);
                await storagefolder.DeleteAsync(StorageDeleteOption.Default);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An unexpected error occured. Check log page for more details");
                Logger.Log(ex.Message, "DeleteFolder_FolderPage", Logger.LogLevelType.Error);
            }
        }
        ObservableCollection<string> PathsIncomplete = new();

        private async void DeleteFiles()
        {
            PathsIncomplete.Clear();
            var selecteditems = grdViewMain.SelectedItems.Cast<FileItem>().ToList();
            foreach (var item in selecteditems)
            {
                Debug.WriteLine("DELETE FILE: " + item.Path);

                var lockingprocesses = GetLockingProcess.GetLockingProcesses(item.Path);
                if (lockingprocesses.Count == 0)
                {
                    Debug.WriteLine("ZERO PROCESS: " + item.Path);
                    if (item.isFolder)
                    {
                        await DeleteSingleFolderAsync(item.Path);
                    }
                    else
                    {
                        await DeleteSingleFileAsync(item.Path);
                    }
                    var exist = MainItems.FirstOrDefault(p => p.Path == item.Path);
                    if (exist != null)
                    {
                        MainItems.Remove(exist);
                    }
                }
                else
                {
                    Debug.WriteLine("MULTI PROCESS: " + item.Path);
                    //bool onlyVusicPlayer = lockingprocesses.All(p => p.ProcessName == "Vusic Player");
                    //if (onlyVusicPlayer)
                    //{
                    //    if (PlayerService.Masterplayer != null)
                    //    {
                    //        PlayerService.filestreamcurrent?.Dispose();
                    //        var filelocked2 = GetLockingProcess.GetLockingProcesses(item.Path);
                    //        if (filelocked2.Count == 0)
                    //        {


                    //        }
                    //    }

                    Debug.WriteLine("ADDING: " + item.Path);
                    PathsIncomplete.Add(item.Path);
                }

            }
            if (PathsIncomplete.Count == 0)
            {
                ttDeletedFiles.IsOpen = true;
                ifbDeleteFiles.IsOpen = true;
                ifbDeleteFiles.Title = "Completed";
                ifbDeleteFiles.Severity = InfoBarSeverity.Success;
                ifbDeleteFiles.Message = "Successfully deleted! You can restore them from the Recycle Bin if needed.";
            }
            else
            {
                Debug.WriteLine("DDHJLFHJOOFH");
                if (App.MainWindowInstance == null) return;
                string currentFile = PathsIncomplete[0];
                PathsIncomplete.Remove(currentFile);
                currentFileInactive = currentFile;
                ifbFileInUse.IsOpen = true;
                ifbFileInUse.Severity = InfoBarSeverity.Error;
                ifbFileInUse.Message = $"The selected item '{currentFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                chckSkipForAllFiles.IsChecked = false;
                if (App.MainWindowInstance.Content != null)
                {
                    ttInaccessibleFiles.XamlRoot = App.MainWindowInstance.Content.XamlRoot;
                }

                // FIX 2: Prevent "ContentDialog is already open" or visual tree tracking errors
                try
                {
                    await ttInaccessibleFiles.ShowAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Dialog failed to show: {ex.Message}");
                }
                //                ttInaccessibleFiles.IsOpen = true;



            }


        }

        private void OceanContentDialog_PrimaryRequested2()
        {
            if (PathsIncomplete.Count == 0)
            {
                Debug.WriteLine("Yes Compelt");
                OceanContentDialog.HideDlg();
                MainWindow.ShowWindow();
            }
            else
            {
                Debug.WriteLine(PathsIncomplete[0] + " is to be remd");
                PathsIncomplete.RemoveAt(0);
            }
        }

        private void OceanContentDialog_PrimaryRequested1()
        {
            Debug.WriteLine("USd");
            if (PathsIncomplete.Count == 0)
            {
                Debug.WriteLine("Yes Compelt");
                OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
                return;
            }

            if (App.MainWindowInstance == null) return;

            // Pull the next file from the top of the list
            string nextFile = PathsIncomplete[0];
            Debug.WriteLine(nextFile + " is to be removed");
            PathsIncomplete.Remove(nextFile);
            foreach (var item in PathsIncomplete)
            {
                Debug.WriteLine(item);
            }

            // Show the dialog for the next file
            OceanContentDialog.Show(
                "File in Used", "Skip", "", "Try Again",
                OceanDialogWindow.ContentType.MessageShow,
                OceanContentDialogDefault.Primary,
                XamlRoot, 500, 500,
                OceanContentDialogType.Elevated,
                App.MainWindowInstance, "", "", "",
                new ObservableCollection<SongModel>(), "",
                $"The selected item '{nextFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again."
            );
        }

        private void btnCreateShow_Click(object sender, RoutedEventArgs e)
        {
            if (currentFolder == null) return;
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Show Model", "Create", "", "Cancel", OceanDialogWindow.ContentType.ShowModel, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "", "", "", "", "", new PlaylistItem(), false, false);
            UnsubscribeAllEventsOceanDialog();

            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested4;
            PlaylistCreation.CallExistingShowDirectory(currentFolder.FolderPath);
        }
        public async Task<bool> RenameStorageFileAsync(StorageFile file, string newName)
        {
            try
            {
                if (File.Exists(file.Path))
                {
                    await file.RenameAsync(newName, NameCollisionOption.FailIfExists);
                    return true;
                }
                else
                {
                    Debug.WriteLine("File not exist");
                    return false;
                }
            }
            catch (Exception ex)
            {
                // Handle unauthorized access, file in use, or duplicate name exceptions
                System.Diagnostics.Debug.WriteLine($"Rename failed: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> RenameFolderAsync(string oldFolderPath, string newName)
        {
            try
            {
                if (currentFolder == null) return false;
                var lockingprocess = GetLockingProcess.GetLockingProcesses(oldFolderPath);
                if (lockingprocess.Count == 0)
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(oldFolderPath);
                    await folder.RenameAsync(newName, NameCollisionOption.FailIfExists);
                    currentFolder.FolderPath = folder.Path;
                    currentFolder.FolderName = newName;
                    InitializeBreadcrumb(currentFolder.FolderPath);
                    foreach (var file in MainItems)
                    {
                        string fileName = Path.GetFileName(file.Path);

                        file.Path = Path.Combine(folder.Path, fileName);
                    }
                    return true;

                }
            }
            catch (FileNotFoundException ex)
            {
                System.Diagnostics.Debug.WriteLine("Error: The specified folder could not be found. " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // Handle other potential exceptions (e.g., Access Denied, Invalid Characters)
                System.Diagnostics.Debug.WriteLine($"Error renaming folder: {ex.Message}");
                return false;
            }
            return false;

        }
        private async void btnConfirmRename_Click(object sender, RoutedEventArgs e)
        {
            if (currentFolder == null) return;
            if (txtRenameFolder.Text == "")
            {
                return;
            }
            if (await RenameFolderAsync(currentFolder.FolderPath, txtRenameFolder.Text))
            {
                txtFolderName.Text = txtRenameFolder.Text;
                ifbRenameInfo.Severity = InfoBarSeverity.Success;
                ifbRenameInfo.Title = "Completed";
                ifbRenameInfo.Message = "Folder has been renamed";
                ifbRenameInfo.IsOpen = true;
            }
            else
            {
                ifbRenameInfo.Severity = InfoBarSeverity.Error;
                ifbRenameInfo.Title = "Error";
                ifbRenameInfo.Message = "An unexpected error occured, check log page for details.";
                ifbRenameInfo.IsOpen = true;
            }
        }


        private void btnAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            AddtoQueue();
        }

        private void grdViewMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (grdViewMain.SelectedItems.Count >= 1)
            {
                ItemsSelected();
            }
            else if (grdViewMain.SelectedItems.Count == 0)
            {
                ItemsDeselected();
            }
        }
        private void ItemsDeselected()
        {
            btnAddtoQueue.Visibility = Visibility.Collapsed;
            stkMultiOptions.Visibility = Visibility.Collapsed;
            btnAddToPlaylist.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Collapsed;
            btnRenameFiles.Visibility = Visibility.Collapsed;
            //    btnMoveFiles.Visibility = Visibility.Collapsed;
            btnDeleteFiles.Visibility = Visibility.Collapsed;
        }
        private void ItemsSelected()
        {
            btnPlay.Visibility = Visibility.Visible;

            btnAddtoQueue.Visibility = Visibility.Visible;
            btnAddToPlaylist.Visibility = Visibility.Visible;
            btnRenameFiles.Visibility = Visibility.Visible;
            //     btnMoveFiles.Visibility = Visibility.Visible;
            btnDeleteFiles.Visibility = Visibility.Visible;
            stkMultiOptions.Visibility = Visibility.Visible;
            btnEditAlbumMass.Visibility = Visibility.Visible;
            btnEditArtistMass.Visibility = Visibility.Visible;
            btnRemoveSelectionsFromFavourites.Visibility = Visibility.Visible;
            var selecteditems = grdViewMain.SelectedItems.Cast<FileItem>();
            var anyselected = selecteditems.Any(p => p.isFolder == true);
            bool hasVideo = selecteditems.Any(video =>
     !string.IsNullOrEmpty(video.Path) &&
     Extensions.VideoExtensions.List.Contains(Path.GetExtension(video.Path).ToLowerInvariant())
    );
            if (hasVideo)
            {
                btnEditAlbumMass.Visibility = Visibility.Collapsed;
                btnEditArtistMass.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnEditAlbumMass.Visibility = Visibility.Visible;
                btnEditArtistMass.Visibility = Visibility.Visible;
            }
            if (anyselected)
            {
                btnPlay.Visibility = Visibility.Collapsed;
                btnAddtoQueue.Visibility = Visibility.Collapsed;
                btnAddToPlaylist.Visibility = Visibility.Collapsed;
                btnEditAlbumMass.Visibility = Visibility.Collapsed;
                btnEditArtistMass.Visibility = Visibility.Collapsed;
                btnRemoveSelectionsFromFavourites.Visibility = Visibility.Collapsed;
                ToolTipService.SetToolTip(btnDeleteFiles, "Delete Selected");
            }
            else
            {
                if (grdViewMain.SelectedItems.Count == 1)
                {
                    ToolTipService.SetToolTip(btnDeleteFiles, "Delete file");
                }
                else
                {
                    ToolTipService.SetToolTip(btnDeleteFiles, "Delete files");
                }
            }

        }
        private async void btnCopyFolderLocation_Click(object sender, RoutedEventArgs e)
        {
            if (currentFolder == null) return;
            CopyToClipboard.CopyStringToClipboard(currentFolder.FolderPath);
            imgCopyButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/tickicon.png"));
            await Task.Delay(2000);
            //CHANGE ICON
            imgCopyButton.Source = new BitmapImage(new Uri("ms-appx:///Assets/appicon.png"));

        }
        ObservableCollection<FileItem> searchresults = new();

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
            {
                searchresults.Clear();
                frmNoSearchResults.Visibility = Visibility.Collapsed;
                grdViewMain.Visibility = Visibility.Visible;
                grdViewMain.ItemsSource = MainItems;

                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = GetFilteredResults(sender.Text);

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);

                sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };
                grdViewMain.ItemsSource = searchresults;
            }

        }
        private IEnumerable<FileItem> GetFilteredResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<FileItem>();

            var rawQuery = query.Trim();

            var minMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:min|m)", RegexOptions.IgnoreCase);
            var secMatch = Regex.Match(rawQuery, @"(\d+)\s*(?:sec|s)", RegexOptions.IgnoreCase);

            int searchSeconds = 0;
            if (minMatch.Success) searchSeconds += int.Parse(minMatch.Groups[1].Value) * 60;
            if (secMatch.Success) searchSeconds += int.Parse(secMatch.Groups[1].Value);

            var textQuery = rawQuery;
            if (minMatch.Success) textQuery = textQuery.Replace(minMatch.Value, "");
            if (secMatch.Success) textQuery = textQuery.Replace(secMatch.Value, "");
            textQuery = textQuery.Trim();

            return MainItems.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.Name?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true)

                );



                return textMatch;
            })
            .OrderByDescending(s => s.Name?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true)
            .ThenBy(s => s.Name);
        }

        private void AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var results = GetFilteredResults(sender.Text);

            if (results.Any())
            {
                frmNoSearchResults.Visibility = Visibility.Collapsed;
                grdViewMain.Visibility = Visibility.Visible;

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);
            }
            else if (MainItems.Count > 0)
            {

                grdViewMain.Visibility = Visibility.Collapsed;

                frmNoSearchResults.Visibility = Visibility.Visible;
                frmNoSearchResults.Navigate(typeof(NoSearchResultsPage), null, new DrillInNavigationTransitionInfo());
            }

        }

        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var sorted = MainItems.OrderBy(p => p.Name).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = MainItems.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    MainItems.Move(oldIndex, newIndex);
                }
            }
        }

        private void chckSelect_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelect.IsChecked == true)
            {
                grdViewMain.SelectionMode = ListViewSelectionMode.Multiple;
            }
            else
            {
                grdViewMain.SelectionMode = ListViewSelectionMode.Single;
            }
        }

        private void mnftSortByDateCreation_Click(object sender, RoutedEventArgs e)
        {
            var sorted = MainItems.OrderBy(p => p.FileCreationTime).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = MainItems.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    MainItems.Move(oldIndex, newIndex);
                }
            }
        }

        private void mnftSortByDateModified_Click(object sender, RoutedEventArgs e)
        {
            var sorted = MainItems.OrderBy(p => p.FileModifiedTime).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = MainItems.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    MainItems.Move(oldIndex, newIndex);
                }
            }
        }

        private void mnftSortBySize_Click(object sender, RoutedEventArgs e)
        {
            var sorted = MainItems.OrderBy(p => p.FileSize).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = MainItems.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    MainItems.Move(oldIndex, newIndex);
                }
            }
        }



        private void mnftByExtension_Click(object sender, RoutedEventArgs e)
        {
            var sorted = MainItems.OrderBy(p => p.Extension).ToList();
            for (int i = 0; i < sorted.Count; i++)
            {
                var oldIndex = MainItems.IndexOf(sorted[i]);
                var newIndex = i;

                if (oldIndex != newIndex)
                {
                    MainItems.Move(oldIndex, newIndex);
                }
            }
        }

        private async void hypItemName_Click(object sender, RoutedEventArgs e)
        {
            if (sender is HyperlinkButton hyp && hyp.DataContext is FileItem item)
            {
                if (item.isFolder)
                {
                    if (App.NavigationFrame == null) return;
                    App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = item.Name, FolderPath = item.Path });

                }
                else
                {
                    string fileExtension = Path.GetExtension(item.Path).ToLower();
                    //          var lockedprocesses = GetLockingProcess.GetLockingProcesses(item.Path);
                    var storagefile = await StorageFile.GetFileFromPathAsync(item.Path);
                    if (await IsStorageFileReadableAsync(storagefile))
                    {
                        if (AudioExtensions.List.Contains(fileExtension))
                        {
                            PlayerService.OpenPath(item.Path);
                        }
                        else if (VideoExtensions.List.Contains(fileExtension))
                        {
                            if (File.Exists(item.Path))
                                Frame.Navigate(typeof(VideoPlayer), item.Path);
                        }
                    }
                    else
                    {
                        if (App.MainWindowInstance == null) return;
                        OceanContentDialog.Show("File Locked", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"The file {item.Path} is denied read access by one or more processes");
                        UnsubscribeAllEventsOceanDialog();

                        OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested6;
                    }
                }
            }
        }
        public async Task<bool> IsStorageFileReadableAsync(StorageFile file)
        {
            try
            {
                // Attempt to open the stream
                using (var stream = await file.OpenAsync(FileAccessMode.Read))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                // 0x80070020 = Sharing Violation (Locked)
                // 0x80070005 = Access Denied
                if (ex.HResult == unchecked((int)0x80070020) || ex.HResult == unchecked((int)0x80070005))
                {
                    return false;
                }
                throw;
            }
        }

        private void OceanContentDialog_PrimaryRequested6()
        {
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        private void grdViewMain_ItemClick(object sender, ItemClickEventArgs e)
        {
            Debug.WriteLine("Okay bye");
            if (chckSelect.IsChecked == false)
            {
                Debug.WriteLine("Okay bye2");

                if (e.ClickedItem is FileItem item)
                {
                    if (item.isFolder)
                    {
                        if (App.NavigationFrame == null) return;
                        App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = item.Name, FolderPath = item.Path });

                    }
                    else
                    {
                        string fileExtension = Path.GetExtension(item.Path).ToLower();

                        if (AudioExtensions.List.Contains(fileExtension))
                        {
                            PlayerService.OpenPath(item.Path);
                        }
                        else if (VideoExtensions.List.Contains(fileExtension))
                        {
                            if (File.Exists(item.Path))
                                Frame.Navigate(typeof(VideoPlayer), item.Path);
                        }
                    }

                }
            }
        }
        private async void MassEdit()
        {
            var selected = grdViewMain.SelectedItems.Cast<FileItem>().ToList();

            var observable = new ObservableCollection<SongModel>();
            foreach (var item in selected)
            {
                var file = await StorageFile.GetFileFromPathAsync(item.Path);
                var props = await file.Properties.GetMusicPropertiesAsync();
                var title = props.Title;
                if (string.IsNullOrEmpty(title))
                {
                    title = Path.GetFileNameWithoutExtension(item.Path);
                }
                observable.Add(new SongModel { Title = title, FilePath = item.Path, SongDuration = props.Duration, AlbumName = props.Album, Artist = props.Artist, Glyph = "\uEC4F", IsAudioItem = true });
            }
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Edit Properties for Multiple", "Close", "", "", OceanDialogWindow.ContentType.MassEditing, OceanContentDialogDefault.Primary, XamlRoot, 950, 900, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", observable, "", "", "", "", "");
            UnsubscribeAllEventsOceanDialog();
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested5;
        }
        private void btnEditAlbumMass_Click(object sender, RoutedEventArgs e)
        {
            MassEdit();
        }

        private void OceanContentDialog_PrimaryRequested5()
        {
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        private void btnEditArtistMass_Click(object sender, RoutedEventArgs e)
        {
            MassEdit();
        }

        private void btnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            //Select all items
            grdViewMain.SelectionMode = ListViewSelectionMode.Multiple;
            chckSelect.IsChecked = true;
            grdViewMain.SelectAll();
            ItemsSelected();

        }

        private void btnClearSelection_Click(object sender, RoutedEventArgs e)
        {
            //Clear selection of all items
            grdViewMain.SelectionMode = ListViewSelectionMode.Single;
            chckSelect.IsChecked = false;
            grdViewMain.DeselectAll();
            ItemsDeselected();

        }

        private async void btnRemoveSelectionsFromFavourites_Click(object sender, RoutedEventArgs e)
        {
            var songss = grdViewMain.SelectedItems.Cast<FileItem>().ToList(); // Converting to a list is safer if you evaluate multiple times
            bool allAreFavorites = songss.All(item => item.IsFavourite);

            bool noneAreFavorites = !songss.Any(item => item.IsFavourite);

            bool partialFavorites = songss.Any(item => item.IsFavourite) && !songss.All(item => item.IsFavourite);
            if (songss.Any(item => item.IsFavourite))
            {
                Debug.WriteLine("All/Partial");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                foreach (var item in songss)
                {
                    var pathtocheck = item.Path;
                    if (pathtocheck == null) return;
                    var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);

                    if (existing != null)
                    {
                        item.IsFavourite = false;
                        item.FavString = "Add to Favourites";
                        Favourites.Remove(existing);
                    }

                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            else if (noneAreFavorites)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var Favourites = currentSettings.Favourites;
                foreach (var item in songss)
                {
                    var pathtocheck = item.Path;
                    if (pathtocheck == null) return;
                    var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);

                    if (existing == null)
                    {
                        item.IsFavourite = true;
                        item.FavString = "Remove from Favourites";

                        Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                    }

                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(HomeView));
            }
        }
        private async void RenameSingleFile(FileItem file, string newname, bool toreopen = false, bool issinglefile = false)
        {
            Debug.WriteLine("Renameoccured");

            var storagefile = await StorageFile.GetFileFromPathAsync(file.Path);
            string directory = "";
            FileInfo fileInfo = new FileInfo(file.Path);
            if (fileInfo.Exists && fileInfo.DirectoryName != null)
            {
                directory = fileInfo.DirectoryName;
            }
            string newPath = Path.Combine(directory, newname + Path.GetExtension(file.Path));
            InfoBar ifb = new InfoBar();
            ifb = ifbRenameFiles;
            if (issinglefile)
            {
                ifb = ifbRenameFile;
            }
            if (await RenameStorageFileAsync(storagefile, newname + Path.GetExtension(file.Path)))
            {
                file.Name = newname;
                file.Path = newPath;
                string fileExtension = Path.GetExtension(newPath).ToLower();
                var newstoragefile = await StorageFile.GetFileFromPathAsync(newPath);

                ifb.Severity = InfoBarSeverity.Success;
                ifb.Title = "Completed";
                ifb.Message = "File has been renamed";
                ifb.IsOpen = true;
                if (toreopen == true)
                {
                    PlayerService.JustDisposed = true;
                    PlayerService.CurrentPlayingPath = newPath;
                    if (PlayerService.Masterplayer != null)
                    {
                        if (PlayerService.Masterplayer.IsPlaying)
                        {
                            PlayerService.OpenPath(newPath);

                        }
                    }
                }
                if (AudioExtensions.List.Contains(fileExtension))
                {
                    var musicprops = await newstoragefile.Properties.GetMusicPropertiesAsync();
                    string durationText = musicprops.Duration.TotalHours >= 1
  ? musicprops.Duration.ToString(@"h\:mm\:ss")
  : musicprops.Duration.ToString(@"m\:ss");
                    FileInfo fileInfo2 = new FileInfo(newPath);
                    string filesize = GetFileOrFolderSizeInString(fileInfo2.Length);
                    file.FileHoverInfo = Path.GetFileName(newPath) + Environment.NewLine + filesize + Environment.NewLine + "Length: " + durationText;
                }
                else if (VideoExtensions.List.Contains(fileExtension))
                {
                    var musicprops = await newstoragefile.Properties.GetVideoPropertiesAsync();
                    string durationText = musicprops.Duration.TotalHours >= 1
  ? musicprops.Duration.ToString(@"h\:mm\:ss")
  : musicprops.Duration.ToString(@"m\:ss");
                    FileInfo fileInfo2 = new FileInfo(newPath);
                    string filesize = GetFileOrFolderSizeInString(fileInfo2.Length);
                    file.FileHoverInfo = Path.GetFileName(newPath) + Environment.NewLine + filesize + Environment.NewLine + "Length: " + durationText;
                }

            }
            else
            {
                ifb.Severity = InfoBarSeverity.Error;
                ifb.Title = "Error";
                ifb.Message = "An unexpected error occured, check log page for details.";
                ifb.IsOpen = true;
                if (toreopen == true)
                {
                    PlayerService.JustDisposed = true;
                    PlayerService.CurrentPlayingPath = newPath;

                    if (PlayerService.Masterplayer != null)
                    {
                        if (PlayerService.Masterplayer.IsPlaying)
                        {
                            PlayerService.OpenPath(newPath);

                        }
                    }
                }
            }
        }
        private async void RenameSingleFolder(FileItem file, bool issinglefile = false)
        {
            InfoBar ifb = new InfoBar();
            ifb = ifbRenameFiles;
            if (issinglefile)
            {
                ifb = ifbRenameFile;
            }
            try
            {

                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(file.Path);

                await folder.RenameAsync(txtRenameFiles.Text, NameCollisionOption.FailIfExists);
                DirectoryInfo dirInfo = new DirectoryInfo(folder.Path);
                var totalsize2 = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                var finalsize = GetFileOrFolderSizeInString(totalsize2);
                ifb.Severity = InfoBarSeverity.Success;
                ifb.Title = "Completed";
                ifb.Message = "Folder has been renamed";
                ifb.IsOpen = true;
                file.FileHoverInfo = Path.GetFileName(folder.Path) + Environment.NewLine + "(Folder)" + Environment.NewLine + finalsize;
                file.Path = folder.Path;
                file.Name = txtRenameFiles.Text;

            }
            catch
            {
                ifb.Severity = InfoBarSeverity.Error;
                ifb.Title = "Error";
                ifb.Message = "An unexpected error occured, check log page for details.";
                ifb.IsOpen = true;
            }
        }
        private async void btnConfirmRenameFiles_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Test1");
            if (grdViewMain.SelectedItems.Count == 1)
            {
                Debug.WriteLine("Yes");
                var item = grdViewMain.SelectedItem;
                if (item is FileItem file)
                {
                    Debug.WriteLine("Yes2");

                    if (txtRenameFiles.Text == file.Name)
                    {
                        Debug.WriteLine("Yes3");

                        ifbRenameFiles.IsOpen = false;
                        return;
                    }
                }
            }

            if (txtRenameFiles.Text == "") return;
            char[] invalidChars = Path.GetInvalidFileNameChars();
            if (txtRenameFiles.Text.Any(ch => invalidChars.Contains(ch)))
            {
                ifbRenameFiles.Severity = InfoBarSeverity.Error;
                ifbRenameFiles.Title = "Error";
                ifbRenameFiles.Message = "Name contains invalid characters!";
                ifbRenameFiles.IsOpen = true;
                return;
            }
            var newname = txtRenameFiles.Text;
            if (grdViewMain.SelectedItems.Count == 1)
            {

                var selecteditem = grdViewMain.SelectedItem;
                if (selecteditem is FileItem file)
                {

                    var lockingprocesses = GetLockingProcess.GetLockingProcesses(file.Path);
                    if (lockingprocesses.Count == 0)
                    {
                        Debug.WriteLine("JUST 1 PROCESS LOCKING");
                        if (file.isFolder)
                        {
                            RenameSingleFolder(file);
                        }
                        else
                        {
                            RenameSingleFile(file, newname);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Yes Multiple");
                        bool onlyVusicPlayer = lockingprocesses.All(p => p.ProcessName == "Vusic Player");
                        if (onlyVusicPlayer)
                        {
                            Debug.WriteLine("Yes Only Vusic Player");

                            if (PlayerService.Masterplayer != null)
                            {
                                Debug.WriteLine("Yes Not Null");

                                var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                                PlayerService.curtime = curTime;
                                PlayerService.curtimetemp = PlayerService.Masterplayer.CurTime;
                                if (PlayerService.filestreamcurrent == null)
                                {
                                    Debug.WriteLine("File stream is null");
                                    Debug.WriteLine("File req is " + file.Path);
                                }
                                PlayerService.filestreamcurrent?.Dispose();
                                var filelocked2 = GetLockingProcess.GetLockingProcesses(file.Path);
                                if (filelocked2.Count == 0)
                                {
                                    Debug.WriteLine("Yes Disposesd");

                                    if (file.isFolder)
                                    {
                                        RenameSingleFolder(file);
                                    }
                                    else
                                    {
                                        RenameSingleFile(file, newname, true);
                                    }
                                }
                            }
                        }
                        else
                        {
                            ifbRenameFiles.IsOpen = true;
                            ifbRenameFiles.Severity = InfoBarSeverity.Error;
                            ifbRenameFiles.Title = "Error";
                            var stringprocess = lockingprocesses.Count == 1 ? "Process" : "Processes";
                            ifbRenameFiles.Message = $"Unable to Rename as selected file(s) may be in use by {lockingprocesses.Count} other {stringprocess}";
                        }
                    }
                }

            }
            else
            {
                int counter = 0;
                var selecteditems = grdViewMain.SelectedItems.Cast<FileItem>();
                foreach (var item in selecteditems)
                {
                    counter++;
                    var lockingprocesses = GetLockingProcess.GetLockingProcesses(item.Path);
                    if (lockingprocesses.Count == 0)
                    {
                        var newName = $"{newname} {counter}";
                        if (rdIncludeNumberAtStart.IsChecked == true)
                        {
                            newName = $"{counter} {newname}";
                        }
                        if (item.isFolder)
                        {
                            txtRenameFiles.Text = newName;
                            RenameSingleFolder(item);
                        }
                        else
                        {
                            RenameSingleFile(item, newname);
                        }
                    }
                    else
                    {
                        bool onlyVusicPlayer = lockingprocesses.All(p => p.ProcessName == "Vusic Player");
                        if (onlyVusicPlayer)
                        {
                            if (PlayerService.Masterplayer != null)
                            {
                                var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                                PlayerService.curtime = curTime;
                                PlayerService.curtimetemp = PlayerService.Masterplayer.CurTime;

                                PlayerService.filestreamcurrent?.Dispose();
                                var filelocked2 = GetLockingProcess.GetLockingProcesses(item.Path);
                                if (filelocked2.Count == 0)
                                {
                                    if (item.isFolder)
                                    {
                                        RenameSingleFolder(item);
                                    }
                                    else
                                    {
                                        RenameSingleFile(item, newname, true);
                                    }
                                    //     RenameSingleFile(item, newname, true);
                                }
                            }
                        }
                        else
                        {
                            ifbRenameFiles.IsOpen = true;
                            ifbRenameFiles.Severity = InfoBarSeverity.Error;
                            ifbRenameFiles.Title = "Error";
                            var stringprocess = lockingprocesses.Count == 1 ? "Process" : "Processes";
                            ifbRenameFiles.Message = $"Unable to Rename as selected file(s) may be in use by {lockingprocesses.Count} other {stringprocess}";
                        }
                    }
                }

            }
        }

        private void btnOpenRecycleBin_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = "shell:RecycleBinFolder",
                UseShellExecute = true
            });

        }

        private async void ttInaccessibleFiles_CloseButtonClick(TeachingTip sender, object args)
        {


        }

        private async void ttInaccessibleFiles_CloseButtonClick_1(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (PathsIncomplete.Count > 0)
            {
                args.Cancel = true;
                // Remove the file they just read about
                string removedFile = PathsIncomplete[0];
                Debug.WriteLine(removedFile + " is removed from queue");
                PathsIncomplete.RemoveAt(0); // More efficient than Remove(string)

                // If there is ANOTHER file left in the queue, show the tip again
                //  await ttInaccessibleFiles.ShowAsync();

                string nextFile = PathsIncomplete[0];
                ifbFileInUse.Message = $"The selected file '{nextFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";

                // Re-open cleanly now that the previous one is fully shut
                //      ttInaccessibleFiles.IsOpen = true;


                //if(PathsIncomplete.Count != 0)
                //{

                //    string nextFile = PathsIncomplete[0];
                //    Debug.WriteLine(nextFile + " is to be removed");
                //    PathsIncomplete.Remove(nextFile);
                //    ifbFileInUse.Message = $"The selected file '{nextFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                //    ttInaccessibleFiles.IsOpen = true;
                //}
            }
        }
        private string currentFileInactive = "";
        private void ttInaccessibleFiles_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (PathsIncomplete.Count > 0)
            {
                if (chckSkipForAllFiles.IsChecked == false)
                {
                    args.Cancel = true;
                    Debug.WriteLine("yess");
                    string removedFile = PathsIncomplete[0];
                    currentFileInactive = removedFile;
                    Debug.WriteLine(removedFile + " is removed from queue");
                    PathsIncomplete.Remove(removedFile);
                    ifbFileInUse.Message = $"The selected file '{removedFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                }
            }
        }

        private async void ttInaccessibleFiles_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            args.Cancel = true;
            var item = currentFileInactive;
            var lockingprocesses = GetLockingProcess.GetLockingProcesses(item);
            if (lockingprocesses.Count == 0)
            {
                var exist2 = PathsIncomplete.FirstOrDefault(p => p == currentFileInactive);
                if (exist2 != null)
                {
                    PathsIncomplete.Remove(exist2);
                }
                Debug.WriteLine("ZERO PROCESS: " + item);
                if (Directory.Exists(item))
                {
                    await DeleteSingleFolderAsync(item);
                }
                else if (File.Exists(item))
                {
                    await DeleteSingleFileAsync(item);
                }
                var exist = MainItems.FirstOrDefault(p => p.Path == item);
                if (exist != null)
                {
                    MainItems.Remove(exist);
                }
                if (PathsIncomplete.Count != 0)
                {
                    var currentFile = PathsIncomplete[0];
                    ifbFileInUse.IsOpen = true;
                    ifbFileInUse.Severity = InfoBarSeverity.Error;
                    ifbFileInUse.Message = $"The selected file '{currentFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                }
                else
                {
                    ttInaccessibleFiles.Hide();
                }
            }
            else
            {
                if (App.MainWindowInstance == null) return;
                string currentFile = item;

                ifbFileInUse.IsOpen = true;
                ifbFileInUse.Severity = InfoBarSeverity.Error;
                ifbFileInUse.Message = $"The selected file '{currentFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                //                ttInaccessibleFiles.IsOpen = true;



            }

        }

        private void mnftRename_Click(object sender, RoutedEventArgs e)
        {
            ttRenameFileSingle.IsOpen = true;
            ifbRenameFile.IsOpen = false;

            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileItem file)
            {
                txtRenameFile.Text = file.Name;
                currentcontextfileitem = file;
            }

        }

        private void mnftDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileItem file)
            {
                if (App.MainWindowInstance == null) return;
                currentcontextfileitem = file;
                OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{file.Name}' will be sent to the Recycle Bin. You can restore it from the Recycle Bin if needed.");
                UnsubscribeAllEventsOceanDialog();

                OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested7;
            }
        }

        private async void OceanContentDialog_PrimaryRequested7()
        {
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
            PathsIncomplete.Clear();
            var item = currentcontextfileitem;
            Debug.WriteLine("DELETE FILE: " + item.Path);

            var lockingprocesses = GetLockingProcess.GetLockingProcesses(item.Path);
            if (lockingprocesses.Count == 0)
            {
                Debug.WriteLine("ZERO PROCESS: " + item.Path);
                if (item.isFolder)
                {
                    await DeleteSingleFolderAsync(item.Path);
                }
                else
                {
                    await DeleteSingleFileAsync(item.Path);
                }
                var exist = MainItems.FirstOrDefault(p => p.Path == item.Path);
                if (exist != null)
                {
                    MainItems.Remove(exist);
                }
            }
            else
            {
                Debug.WriteLine("MULTI PROCESS: " + item.Path);
                //bool onlyVusicPlayer = lockingprocesses.All(p => p.ProcessName == "Vusic Player");
                //if (onlyVusicPlayer)
                //{
                //    if (PlayerService.Masterplayer != null)
                //    {
                //        PlayerService.filestreamcurrent?.Dispose();
                //        var filelocked2 = GetLockingProcess.GetLockingProcesses(item.Path);
                //        if (filelocked2.Count == 0)
                //        {


                //        }
                //    }

                Debug.WriteLine("ADDING: " + item.Path);
                PathsIncomplete.Add(item.Path);


            }
            if (PathsIncomplete.Count == 0)
            {
                ttDeletedFiles.IsOpen = true;
                ifbDeleteFiles.IsOpen = true;
                ifbDeleteFiles.Title = "Completed";
                ifbDeleteFiles.Severity = InfoBarSeverity.Success;
                ifbDeleteFiles.Message = "Successfully deleted! You can restore it from the Recycle Bin if needed.";
            }
            else
            {
                Debug.WriteLine("DDHJLFHJOOFH");
                if (App.MainWindowInstance == null) return;
                string currentFile = PathsIncomplete[0];
                PathsIncomplete.Remove(currentFile);
                currentFileInactive = currentFile;
                ifbFileInUse.IsOpen = true;
                ifbFileInUse.Severity = InfoBarSeverity.Error;
                ifbFileInUse.Message = $"The selected item '{currentFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again.";
                chckSkipForAllFiles.IsChecked = false;
                if (App.MainWindowInstance.Content != null)
                {
                    ttInaccessibleFiles.XamlRoot = App.MainWindowInstance.Content.XamlRoot;
                }

                // FIX 2: Prevent "ContentDialog is already open" or visual tree tracking errors
                try
                {
                    await ttInaccessibleFiles.ShowAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Dialog failed to show: {ex.Message}");
                }
                //                ttInaccessibleFiles.IsOpen = true;



            }
        }

        private async void btnNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            ifbPlaylistAddTo.IsOpen = false;
            var selecteditems = grdViewMain.SelectedItems.Cast<FileItem>().ToList();
            var temporaryobservable = new ObservableCollection<SongModel>();
            foreach (var item in selecteditems)
            {
                var storagefile = await StorageFile.GetFileFromPathAsync(item.Path);
                string fileExtension = Path.GetExtension(item.Path).ToLower();

                if (AudioExtensions.List.Contains(fileExtension))
                {
                    var properties = await storagefile.Properties.GetMusicPropertiesAsync();
                    temporaryobservable.Add(new SongModel { FilePath = item.Path, Title = Path.GetFileNameWithoutExtension(item.Path), IsAudioItem = true, Glyph = "\uEC4F", SongDuration = properties.Duration });
                }
                else
                {
                    var properties = await storagefile.Properties.GetVideoPropertiesAsync();
                    temporaryobservable.Add(new SongModel { FilePath = item.Path, Title = Path.GetFileNameWithoutExtension(item.Path), IsAudioItem = true, Glyph = "\uE8B2", SongDuration = properties.Duration });
                }
            }
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Playlist", "Create", "", "Cancel", OceanDialogWindow.ContentType.PlaylistCreation, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", temporaryobservable, "Playlist");
            UnsubscribeAllEventsOceanDialog();

            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested3;
            PlaylistCreation.CallExistingItems(temporaryobservable);
        }
        private void UnsubscribeAllEventsOceanDialog()
        {
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested2;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested3;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested4;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested5;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested6;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested7;
        }
        private void OceanContentDialog_PrimaryRequested4()
        {
            PlaylistCreation.CallShowCreation();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        private void OceanContentDialog_PrimaryRequested3()
        {
            PlaylistCreation.CallPlaylistCreation();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
            ifbPlaylistAddTo.IsOpen = true;
            ifbPlaylistAddTo.Severity = InfoBarSeverity.Success;
            ifbPlaylistAddTo.Title = "Playlist Created";
            ifbPlaylistAddTo.Message = "Requested playlist has been created successfully!";

        }

        private async void btnAddToPlaylists_Click(object sender, RoutedEventArgs e)
        {
            var selecteditems = grdViewMain.SelectedItems.Cast<FileItem>().ToList();
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var playlists = currentSettings.SavedPlaylists;
            foreach (var selectedplaylist in lstAddToPlaylists.SelectedItems)
            {
                if (selectedplaylist is PlaylistItem playlist)
                {
                    Debug.WriteLine("Yes selected item is playlist: " + playlist.PlaylistName);
                    var existplaylist = playlists.FirstOrDefault(p => p.PlaylistId == playlist.PlaylistId);
                    if (existplaylist != null)
                    {
                        Debug.WriteLine("Yes selected item is not null: " + existplaylist.PlaylistName);

                        foreach (var item in selecteditems)
                        {
                            var exist = existplaylist.SongsPaths.FirstOrDefault(p => p == item.Path);
                            if (exist == null)
                            {
                                existplaylist.SongsPaths.Add(item.Path);
                            }
                        }
                        var count = existplaylist.SongsPaths.Count;
                        existplaylist.PlaylistCount = $"{count} {(count == 1 ? "item" : "items")}";
                        ;
                    }

                }
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            ifbPlaylistAddTo.IsOpen = true;
            ifbPlaylistAddTo.Severity = InfoBarSeverity.Success;
            ifbPlaylistAddTo.Title = "Added";
            ifbPlaylistAddTo.Message = "Selected item(s) have been added to the selected playlist(s)";
        }

        private async void MenuFlyout_Opened(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            if (flyout == null) return;
            var addToPlaylist = flyout?.Items
      .OfType<MenuFlyoutSubItem>()
      .FirstOrDefault(x => x.Text == "Add to Playlist");

            if (addToPlaylist == null)
                return;

            addToPlaylist.Items.Clear();
            var selectedsong = addToPlaylist?.DataContext as FileItem;
            if (selectedsong == null) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var Playlists = currentSettings.SavedPlaylists;
            foreach (var playliitem in Playlists)
            {
                MenuFlyoutItem playlistitem = new MenuFlyoutItem();
                playlistitem.Text = playliitem.PlaylistName;
                addToPlaylist?.Items.Add(playlistitem);
                playlistitem.Click += async (sender, e) =>
                {
                    var path = selectedsong?.Path;

                    if (path != null)
                    {
                        if (playliitem.SongsPaths.Contains(path))
                        {
                            ttAddedToPlaylist.Title = $"{Path.GetFileNameWithoutExtension(path)} already exists in {playliitem.PlaylistName}";
                        }
                        else
                        {
                            playliitem.SongsPaths.Add(path);
                            int count = playliitem.SongsPaths.Count;
                            playliitem.PlaylistCount = $"{count} {(count == 1 ? "item" : "items")}";
                            await SettingsLoader.SaveSettingsAsync(currentSettings);
                            ttAddedToPlaylist.Title = $"{Path.GetFileNameWithoutExtension(path)} has been added to {playliitem.PlaylistName}";

                        }
                        hypPlaylistAdded.Content = playliitem.PlaylistName;
                        hypPlaylistAdded.Tag = playliitem;
                        ttAddedToPlaylist.IsOpen = true;
                        await Task.Delay(3000);
                        ttAddedToPlaylist.IsOpen = false;
                    }
                };

            }
            var mnftAddtoFav = flyout?.Items
   .OfType<MenuFlyoutItem>()
   .FirstOrDefault(x => x.Name == "mnftAddToFavourites");

            if (mnftAddtoFav == null) return;
            if (selectedsong.IsFavourite == true)
            {
                mnftAddtoFav.Text = "Remove from Favourites";
            }
            else
            {

                mnftAddtoFav.Text = "Add to Favourites";
            }
        }
        private void hypPlaylistAdded_Click(object sender, RoutedEventArgs e)
        {
            if (hypPlaylistAdded.Tag is PlaylistItem playlistItem)
            {
                if (App.NavigationFrame != null)
                    App.NavigationFrame.Navigate(typeof(PlaylistView), playlistItem);
            }
        }
        private async void mnftPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is FileItem item)
            {
                if (item.isFolder)
                {
                    if (App.NavigationFrame == null) return;
                    App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = item.Name, FolderPath = item.Path });

                }
                else
                {
                    string fileExtension = Path.GetExtension(item.Path).ToLower();
                    //          var lockedprocesses = GetLockingProcess.GetLockingProcesses(item.Path);
                    var storagefile = await StorageFile.GetFileFromPathAsync(item.Path);
                    if (await IsStorageFileReadableAsync(storagefile))
                    {
                        if (AudioExtensions.List.Contains(fileExtension))
                        {
                            PlayerService.OpenPath(item.Path);
                        }
                        else if (VideoExtensions.List.Contains(fileExtension))
                        {
                            if (File.Exists(item.Path))
                                Frame.Navigate(typeof(VideoPlayer), item.Path);
                        }
                    }
                    else
                    {
                        if (App.MainWindowInstance == null) return;
                        OceanContentDialog.Show("File Locked", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"The file {item.Path} is denied read access by one or more processes");
                        UnsubscribeAllEventsOceanDialog();

                        OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested6;
                    }
                }
            }

        }

        private void mnftFileInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FileItem file)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(file.Path);
                }
            }
        }
        FileItem currentcontextfileitem = new();
        private void btnConfirmRenameFile_Click(object sender, RoutedEventArgs e)
        {
            if (txtRenameFile.Text == "") return;
            if (currentcontextfileitem is FileItem file)
            {

                var newname = txtRenameFile.Text;
                var lockingprocesses = GetLockingProcess.GetLockingProcesses(file.Path);
                if (lockingprocesses.Count == 0)
                {
                    Debug.WriteLine("JUST 1 PROCESS LOCKING");
                    if (file.isFolder)
                    {
                        RenameSingleFolder(file, true);
                    }
                    else
                    {
                        RenameSingleFile(file, newname, false, true);
                    }
                }
                else
                {
                    Debug.WriteLine("Yes Multiple");
                    bool onlyVusicPlayer = lockingprocesses.All(p => p.ProcessName == "Vusic Player");
                    if (onlyVusicPlayer)
                    {
                        Debug.WriteLine("Yes Only Vusic Player");

                        if (PlayerService.Masterplayer != null)
                        {
                            Debug.WriteLine("Yes Not Null");

                            var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                            PlayerService.curtime = curTime;
                            PlayerService.curtimetemp = PlayerService.Masterplayer.CurTime;
                            if (PlayerService.filestreamcurrent == null)
                            {
                                Debug.WriteLine("File stream is null");
                                Debug.WriteLine("File req is " + file.Path);
                            }
                            PlayerService.filestreamcurrent?.Dispose();
                            var filelocked2 = GetLockingProcess.GetLockingProcesses(file.Path);
                            if (filelocked2.Count == 0)
                            {
                                Debug.WriteLine("Yes Disposesd");

                                if (file.isFolder)
                                {
                                    RenameSingleFolder(file);
                                }
                                else
                                {
                                    RenameSingleFile(file, newname, true, true);
                                }
                            }
                        }
                    }
                    else
                    {
                        ifbRenameFile.IsOpen = true;
                        ifbRenameFile.Severity = InfoBarSeverity.Error;
                        ifbRenameFile.Title = "Error";
                        var stringprocess = lockingprocesses.Count == 1 ? "Process" : "Processes";
                        ifbRenameFile.Message = $"Unable to Rename as selected file(s) may be in use by {lockingprocesses.Count} other {stringprocess}";
                    }
                }
            }

        }
    }
}

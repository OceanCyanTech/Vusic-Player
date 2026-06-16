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
                        MainItems.Add(new FileItem
                        {
                            Path = file.Path,
                            Name = Path.GetFileNameWithoutExtension(file.Path),
                            Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(file.Path),
                            isFavourite = isFav,
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
                            isFavourite = isFav,
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
        public ObservableCollection<FolderModel> BreadcrumbItems { get; set; } = new();
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

        private void btnAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (grdViewMain.SelectedItems.Count > 0)
            {
                ttAddtoPlaylist.IsOpen = true;
            }
        }

        private void btnDeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "The selected items will be sent to the Recycle Bin. You can restore them from the Recycle Bin if needed.");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
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
                if (App.MainWindowInstance == null) return;
                string currentFile = PathsIncomplete[0];
                PathsIncomplete.Remove(currentFile);

                // 2. Safely hook up the event handler (unsubscribe first to avoid duplicates)

                // 3. Show the dialog for this specific file
                OceanContentDialog.Show(
                    "File in Use", "Skip", "", "Try Again",
                    OceanDialogWindow.ContentType.MessageShow,
                    OceanContentDialogDefault.Primary,
                    XamlRoot, 500, 500,
                    OceanContentDialogType.Elevated,
                    App.MainWindowInstance, "", "", "",
                    new ObservableCollection<SongModel>(), "",
                    $"The selected file '{currentFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again."
                );
                OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
                OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested1;

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
                $"The selected file '{nextFile}' cannot be deleted because it is locked by one or more processes. Close those processes and then try again."
            );
        }

        private void btnCreateShow_Click(object sender, RoutedEventArgs e)
        {

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

        private void hypItemName_Click(object sender, RoutedEventArgs e)
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

        private void btnEditAlbumMass_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnEditArtistMass_Click(object sender, RoutedEventArgs e)
        {

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

        private void btnRemoveSelectionsFromFavourites_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(HomeView));
            }
        }
        private async void RenameSingleFile(FileItem file, string newname, bool toreopen = false)
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
            if (await RenameStorageFileAsync(storagefile, newname + Path.GetExtension(file.Path)))
            {
                file.Name = newname;
                file.Path = newPath;
                string fileExtension = Path.GetExtension(newPath).ToLower();
                var newstoragefile = await StorageFile.GetFileFromPathAsync(newPath);
                ifbRenameFiles.Severity = InfoBarSeverity.Success;
                ifbRenameFiles.Title = "Completed";
                ifbRenameFiles.Message = "File has been renamed";
                ifbRenameFiles.IsOpen = true;
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
                ifbRenameFiles.Severity = InfoBarSeverity.Error;
                ifbRenameFiles.Title = "Error";
                ifbRenameFiles.Message = "An unexpected error occured, check log page for details.";
                ifbRenameFiles.IsOpen = true;
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
        private async void RenameSingleFolder(FileItem file)
        {
            try
            {
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(file.Path);

                await folder.RenameAsync(txtRenameFiles.Text, NameCollisionOption.FailIfExists);
                DirectoryInfo dirInfo = new DirectoryInfo(folder.Path);
                var totalsize2 = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
                var finalsize = GetFileOrFolderSizeInString(totalsize2);
                ifbRenameFiles.Severity = InfoBarSeverity.Success;
                ifbRenameFiles.Title = "Completed";
                ifbRenameFiles.Message = "Folder has been renamed";
                ifbRenameFiles.IsOpen = true;
                file.FileHoverInfo = Path.GetFileName(folder.Path) + Environment.NewLine + "(Folder)" + Environment.NewLine + finalsize;
                file.Path = folder.Path;
                file.Name = txtRenameFiles.Text;

            }
            catch
            {
                ifbRenameFiles.Severity = InfoBarSeverity.Error;
                ifbRenameFiles.Title = "Error";
                ifbRenameFiles.Message = "An unexpected error occured, check log page for details.";
                ifbRenameFiles.IsOpen = true;
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
    }
}

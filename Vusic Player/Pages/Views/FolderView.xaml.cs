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
using System.DirectoryServices;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;


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
                prgLoading.Visibility = Visibility.Visible;
                currentFolder = folder;
                grdViewMain.ItemsSource = MainItems;
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
                //             bool hasVideo = MainItems.Any(song =>
                //    !string.IsNullOrEmpty(song.FilePath) &&
                //    Extensions.VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant())
                //);
                //             if (hasVideo)
                //             {
                //                 lstViewMain.VideoPlaylistUI();
                //             }
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

        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnRenameFiles_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnMoveFiles_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddToPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnDeleteFiles_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "The selected items will be sent to the Recycle Bin. This cannot be undone.");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
        }

        private void OceanContentDialog_PrimaryRequested()
        {
            DeleteFiles();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }
        private void DeleteFiles()
        {
            //var selecteditems = lstViewMain.SelectedItems.Cast<SongModel>();
            //if(selecteditems != null)
            //{
            //    foreach(var item in selecteditems)
            //    {
            //       FileSystem.DeleteFile
            //    }
            //}
        }
        private void btnCreateShow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConfirmRename_Click(object sender, RoutedEventArgs e)
        {

        }

        
        private void btnAddtoQueue_Click(object sender, RoutedEventArgs e)
        {

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
            btnMoveFiles.Visibility = Visibility.Collapsed;
            btnDeleteFiles.Visibility = Visibility.Collapsed;
        }
        private void ItemsSelected()
        {
            btnPlay.Visibility = Visibility.Visible;

            btnAddtoQueue.Visibility = Visibility.Visible;
            btnAddToPlaylist.Visibility = Visibility.Visible;
            btnRenameFiles.Visibility = Visibility.Visible;
            btnMoveFiles.Visibility = Visibility.Visible;
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
                    App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = item.Name, FolderPath = item.Path});

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
    }
}

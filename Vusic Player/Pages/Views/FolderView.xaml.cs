using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
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
        ObservableCollection<SongModel> MainItems = new();
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
                    MainItems.Add(new SongModel
                    {
                        FilePath = sub.Path,
                        IsAudioItem = false,
                        FileTypeName = "Folder",
                        Title = Path.GetFileName(folPath),
                    });
                }
                var files = await folder.GetFilesAsync();
                foreach (var file in files)
                {
                    string fileExtension = file.FileType.ToLower();

                    if (!VideoExtensions.List.Contains(fileExtension) && !AudioExtensions.List.Contains(fileExtension))
                    {
                        continue;
                    }
                    var colorbrush = new SolidColorBrush(Microsoft.UI.Colors.White);
                    var glyph = "\uEC4F";
                    if (VideoExtensions.List.Contains(fileExtension))
                    {
                        glyph = "\uE8B2";
                    }
                    if (PlayerService.CurrentPlayingPath == file.Path)
                    {
                        colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
                        if (PlayerService.Masterplayer!.IsPlaying)
                            glyph = "\uE769";
                        else
                        {
                            glyph = "\uE768";
                        }
                    }
                    if (AudioExtensions.List.Contains(fileExtension))
                    {
                        var musicprops = await file.Properties.GetMusicPropertiesAsync();
                        var exist = favourites.FirstOrDefault(p => p.FilePath == file.Path);
                        bool isFav = false;
                        if (exist != null)
                        {
                            isFav = true;
                        }
                        MainItems.Add(new SongModel
                        {
                            FilePath = file.Path,
                            Title = Path.GetFileName(file.Path),
                            Artist = musicprops.Artist,
                            AlbumName = musicprops.Album,
                            SongDuration = musicprops.Duration,
                            IsFavourite = isFav,
                            Glyph = glyph,
                            TitleColor = colorbrush
                        });
                    }
                    else if (VideoExtensions.List.Contains(fileExtension))
                    {
                        var vidprops = await file.Properties.GetVideoPropertiesAsync();
                        var exist = favourites.FirstOrDefault(p => p.FilePath == file.Path);
                        bool isFav = false;
                        if (exist != null)
                        {
                            isFav = true;
                        }
                        MainItems.Add(new SongModel
                        {
                            FilePath = file.Path,
                            VisibilityofAudioMeta = Visibility.Collapsed,
                            Title = Path.GetFileName(file.Path),
                            Glyph = glyph,
                            TitleColor = colorbrush,
                            SongDuration = vidprops.Duration,
                            IsFavourite = isFav,

                        });
                    }


                }
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
                currentFolder = folder;
                lstViewMain.ItemsSource = MainItems;
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
                bool hasVideo = MainItems.Any(song =>
       !string.IsNullOrEmpty(song.FilePath) &&
       Extensions.VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant())
   );
                if (hasVideo)
                {
                    lstViewMain.VideoPlaylistUI();
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
            OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 600, 600, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "The selected items will be sent to the Recycle Bin. This cannot be undone.");
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

        private void lstViewMain_ListViewSelectionChange(object sender, SelectionChangedEventArgs e)
        {
            if (lstViewMain.SelectedItems.Count >= 1)
            {
                btnPlay.Visibility = Visibility.Visible;

                btnAddtoQueue.Visibility = Visibility.Visible;
                btnRenameFiles.Visibility = Visibility.Visible;
                btnMoveFiles.Visibility = Visibility.Visible;
                btnDeleteFiles.Visibility = Visibility.Visible;

            }
            else if (lstViewMain.SelectedItems.Count == 0)
            {
                btnAddtoQueue.Visibility = Visibility.Collapsed;
                btnPlay.Visibility = Visibility.Collapsed;
                btnRenameFiles.Visibility = Visibility.Collapsed;
                btnMoveFiles.Visibility = Visibility.Collapsed;
                btnDeleteFiles.Visibility = Visibility.Collapsed;
            }
        }

        private void btnAddtoQueue_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

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
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;


namespace Vusic_Player.Pages.Views
{

    public sealed partial class FolderView : Page
    {
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
                        Title = Path.GetDirectoryName(folPath),
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
                            Title = Path.GetDirectoryName(folPath),
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
                            Title = Path.GetDirectoryName(folPath),
                            Glyph = glyph,
                            TitleColor = colorbrush,
                            SongDuration = vidprops.Duration,
                            IsFavourite = isFav,

                        });
                    }


                }
            }
        }
        public ObservableCollection<string> BreadcrumbItems { get; set; } = new();
        private void InitializeBreadcrumb(string path)
        {
            BreadcrumbItems.Clear();
            string[] segments = path.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

            foreach (var segment in segments)
            {
                BreadcrumbItems.Add(segment);
            }

            brdcrumBarMain.ItemsSource = BreadcrumbItems;
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is FolderModel folder && folder.FolderPath is string FolderPath)
            {
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
            if(App.NavigationFrame != null)
            {

            }
        }
    }
}

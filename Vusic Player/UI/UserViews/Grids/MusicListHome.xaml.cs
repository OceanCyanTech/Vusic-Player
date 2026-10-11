using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class MusicListHome : UserControl
    {
        public MusicListHome()
        {
            InitializeComponent();
            InitializeMusicSuggestions();
        }
        private async void InitializeMusicSuggestions()
        {
            // 1. Fetch library tracks from SQLite / Memory cache
            var allTracks = FilesInDatabase.rawSongs;

            if (allTracks == null || allTracks.Count == 0)
            {
                ShowEmptyMusicSuggestions();
                return;
            }

            Random rng = new Random();

            // 2. Filter valid, existing audio files (exclude missing paths or videos)
            var candidateAudios = allTracks
                .Where(t => !string.IsNullOrEmpty(t.FilePath)
                         && AudioExtensions.List.Contains(Path.GetExtension(t.FilePath), StringComparer.OrdinalIgnoreCase)
                         && File.Exists(t.FilePath))
                .OrderBy(_ => rng.Next())
                .Take(6) // Pick 6 recommended music tracks
                .ToList();

            if (candidateAudios.Count == 0)
            {
                ShowEmptyMusicSuggestions();
                return;
            }

            FinalList.Clear();

            // 3. Populate immediate placeholders to render the GridView instantly
            foreach (var track in candidateAudios)
            {
                string title = !string.IsNullOrWhiteSpace(track.Title)
                    ? track.Title
                    : Path.GetFileNameWithoutExtension(track.FilePath);

                var recommendItem = new FavouritesRecommend
                {
                    FilePath = track.FilePath,
                    FileName = title,
                    Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(track.FilePath),
                };

                FinalList.Add(recommendItem);

                // 4. Asynchronously extract embedded artwork without blocking UI responsiveness


                // 5. Update UI visibility
                if (FinalList.Count > 0)
                {
                    grdEmptySuggestions.Visibility = Visibility.Collapsed;
                    grdMusicMix.Visibility = Visibility.Visible;
                }
                else
                {
                    ShowEmptyMusicSuggestions();
                }
            }
        }
        private void mnftGoToFileLocation_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                OpenFileLocation.PathSelect(selectedSong.FilePath);
            }
        }
        private async void mnftAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                var exist = QueueService.VusicQueue.FirstOrDefault(p => p.FilePath == selectedSong.FilePath);
                if (exist == null)
                {
                    if (!File.Exists(selectedSong.FilePath)) return;
                    var file = await StorageFile.GetFileFromPathAsync(selectedSong.FilePath);
                    var props = await file.Properties.GetMusicPropertiesAsync();

                    string Title = props.Title ?? file.DisplayName;
                    if (Title == "")
                    {
                        Title = Path.GetFileNameWithoutExtension(file.Path);
                    }
                    string AlbumName = props.Album;

                    string Artist = props.Artist;
                    QueueService.VusicQueueNext.Add(new SongModel { Title = Title, AlbumName = AlbumName, Artist = Artist, SongDuration = props.Duration, FilePath = file.Path });
                    QueueService.VusicQueue.Add(new SongModel { Title = Title, AlbumName = AlbumName, Artist = Artist, SongDuration = props.Duration, FilePath = file.Path });
                }
            }
        }
        private void mnftPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                if (File.Exists(selectedSong.FilePath))
                {
                    ObservableCollection<SongModel> single = new();
                    string Title = Path.GetFileNameWithoutExtension(selectedSong.FilePath);
                    single.Add(new SongModel { FilePath = selectedSong.FilePath, Title = Title });
                    QueueService.PlayMedia(single, false, false);
                }
                else
                {
                    if (App.MainWindowInstance == null) return;
                    OceanContentDialog.Show("Missing File", "Relocate", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{selectedSong.FilePath}' does not exist.");

                    OceanContentDialog.PrimaryRequested += async () =>
                    {
                        if (App.OceanDialogInstance == null) return;


                        var file = await FilePickers.MediaPicker.PickSingleAudio(App.OceanDialogInstance, "Select File To Relocate");
                        if (file != null)
                        {
                            selectedSong.FilePath = file.Path;
                            selectedSong.Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(file.Path);
                            ObservableCollection<SongModel> single = new();
                            string Title = Path.GetFileNameWithoutExtension(selectedSong.FilePath);

                            selectedSong.FileName = Title;
                            single.Add(new SongModel { FilePath = selectedSong.FilePath, Title = Title });
                            QueueService.PlayMedia(single, false, false);
                            OceanContentDialog.HideDlg();
                            MainWindow.ShowWindow();
                        }
                    };
                }

            }

        }
        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = new ObservableCollection<SongModel>();
            foreach (var item in FinalList)
            {
                if (File.Exists(item.FilePath))
                {
                    var currentfile = await StorageFile.GetFileFromPathAsync(item.FilePath);
                    var musicprops = await currentfile.Properties.GetMusicPropertiesAsync();
                    observablecollection.Add(new SongModel { Title = item.FileName, FilePath = item.FilePath, AlbumName = musicprops.Album, Artist = musicprops.Artist });
                }
            }
            QueueService.PlayMedia(observablecollection, false, false);
        }
    
        private void mnftViewFileInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(selectedSong.FilePath);
                }
            }
        }

        private void ShowEmptyMusicSuggestions()
        {
            FinalList.Clear();
            grdEmptySuggestions.Visibility = Visibility.Visible;
            grdMusicMix.Visibility = Visibility.Collapsed;
        }
        ObservableCollection<FavouritesRecommend> FinalList = new ObservableCollection<FavouritesRecommend>();
        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            grdvFavourites.SelectionMode = chkSelect.IsChecked == true ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = chkSelect.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllMusicMix_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllMusicMix.IsChecked == true) grdvFavourites.SelectAll();
            else grdvFavourites.SelectedItems.Clear();
        }


       

        private void grdvFavourites_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (grdvFavourites.SelectionMode == ListViewSelectionMode.Single)
            {
                var selectedSong = e.ClickedItem as FavouritesRecommend;
                if (selectedSong == null) return;
                if (File.Exists(selectedSong.FilePath))
                {
                    ObservableCollection<SongModel> single = new();
                    string Title = Path.GetFileNameWithoutExtension(selectedSong.FilePath);
                    single.Add(new SongModel { FilePath = selectedSong.FilePath, Title = Title });
                    QueueService.PlayMedia(single, false, false);
                }
                else
                {
                    if (App.MainWindowInstance == null) return;
                    OceanContentDialog.Show("Missing File", "Relocate", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{selectedSong.FilePath}' does not exist.");

                    OceanContentDialog.PrimaryRequested += async () =>
                    {
                        if (App.OceanDialogInstance == null) return;

                        var file = await FilePickers.MediaPicker.PickSingleAudio(App.OceanDialogInstance, "Select File To Relocate");
                        if (file != null)
                        {

                            var musicproperties = await file.Properties.GetMusicPropertiesAsync();
                            string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(file.Path) : musicproperties.Title;
                            selectedSong.FilePath = file.Path;
                            selectedSong.FileName = title;
                            selectedSong.Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(file.Path);
                            ObservableCollection<SongModel> single = new();
                            string Title = Path.GetFileNameWithoutExtension(selectedSong.FilePath);
                            single.Add(new SongModel { FilePath = selectedSong.FilePath, Title = Title });
                            QueueService.PlayMedia(single, false, false);
                            OceanContentDialog.HideDlg();
                            MainWindow.ShowWindow();

                        }
                    };
                }
            }

        }
    }
}

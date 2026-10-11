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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vortice.Direct2D1.Effects;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class Favourites : UserControl
    {
        public Favourites()
        {
            InitializeComponent();
            InitializeRecommendations();
        }
        ObservableCollection<FavouritesRecommend> FinalList = new();
        private async void InitializeRecommendations()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings?.Favourites;

            if (favourites == null || favourites.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            Random rng = new Random();

            // 1. Filter existing audio and video paths
            var validVideos = new List<string>();
            var validAudios = new List<string>();

            foreach (var item in favourites)
            {
                string path = item.FilePath;
                if (string.IsNullOrEmpty(path) || !File.Exists(path)) continue;

                string extension = Path.GetExtension(path).ToLowerInvariant();

                if (VideoExtensions.List.Contains(extension))
                {
                    validVideos.Add(path);
                }
                else if (AudioExtensions.List.Contains(extension))
                {
                    validAudios.Add(path);
                }
            }

            // 2. Sample candidate items
            var selectedCandidates = validVideos.Concat(validAudios)
                .OrderBy(_ => rng.Next())
                .Take(5)
                .ToList();

            if (selectedCandidates.Count == 0)
            {
                ShowEmptyState();
                return;
            }

            FinalList.Clear();

            // 3. Add placeholders immediately and dispatch async thumbnail work
            foreach (var path in selectedCandidates)
            {
                if (!File.Exists(path)) continue;
                string ext = Path.GetExtension(path).ToLowerInvariant();
                bool isVideo = VideoExtensions.List.Contains(ext);

                string fallbackUri = isVideo
                    ? "ms-appx:///Assets/videonoteicon.png"
                    : "ms-appx:///Assets/musicnoteicon.png";

                var favItem = new FavouritesRecommend
                {
                    FilePath = path,
                    FileName = Path.GetFileNameWithoutExtension(path),
                    Thumbnail = new BitmapImage(new Uri(fallbackUri))
                };

                FinalList.Add(favItem);

                // Load thumbnails in the background without stalling the loop
                if (isVideo)
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // Grab frame at ~10% mark as preview thumbnail
                            var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(path, 0.10);
                            if (!string.IsNullOrEmpty(thumb) && File.Exists(thumb))
                            {
                                DispatcherQueue.TryEnqueue(async () =>
                                {
                                    try
                                    {
                                        var bitmap = new BitmapImage();
                                        using (var stream = File.OpenRead(thumb))
                                        {
                                            await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                                        }
                                        favItem.Thumbnail = bitmap;
                                    }
                                    finally
                                    {
                                        if (File.Exists(thumb))
                                        {
                                            File.Delete(thumb);
                                        }
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[Favs] Video thumbnail failed for {path}: {ex.Message}");
                        }
                    });
                }
                else
                {
                    try
                    {
                        var audioThumb = await FileThumbnailObtain.GetFileThumbnailAsync(path);
                        if (audioThumb != null)
                        {
                            favItem.Thumbnail = audioThumb;

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"[Favs] Audio thumbnail failed for {path}: {ex.Message}");
                    }
                }
            }

            // 4. Update UI containers right away
            if (FinalList.Count > 0)
            {
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
                grdRecents.Visibility = Visibility.Visible;
            }
            else
            {
                ShowEmptyState();
            }
        }
        private void ShowEmptyState()
        {
            FinalList.Clear();
            grdEmptySuggestions.Visibility = Visibility.Visible;
            grdRecents.Visibility = Visibility.Collapsed;
        }
        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            grdvFavourites.SelectionMode = chkSelect.IsChecked == true ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = chkSelect.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked == true) grdvFavourites.SelectAll();
            else grdvFavourites.SelectedItems.Clear();
        }

        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            var selectedSongs = grdvFavourites.SelectedItems.OfType<FavouritesRecommend>().ToList();
            if (!selectedSongs.Any()) return;

            var paths = selectedSongs.Select(s => s.FilePath);
            await GlobalFavourites.Instance.SetFavouritesRangeAsync(paths, false);

            // Remove only the items that were unfavorited
            foreach (var item in selectedSongs)
            {
                FinalList.Remove(item);
            }

            if (FinalList.Count > 0)
            {
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
                grdRecents.Visibility = Visibility.Visible;
            }
            else
            {
                ShowEmptyState();
            }
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

        private async void mnftRemoveFromFav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                await GlobalFavourites.Instance.ToggleFavouriteAsync(selectedSong.FilePath);
                FinalList.Remove(selectedSong);
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
    }
}

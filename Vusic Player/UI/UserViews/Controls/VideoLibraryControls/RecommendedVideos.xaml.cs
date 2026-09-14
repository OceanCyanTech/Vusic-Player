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
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using FileInfo = Vusic_Player.Configuration.Helper.FileInfo;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.VideoLibraryControls
{
    public sealed partial class RecommendedVideos : UserControl
    {
        ObservableCollection<VideoProgress> RecommendedVideoList = new ObservableCollection<VideoProgress>();
        public RecommendedVideos()
        {
            InitializeComponent();
            LoadSettings();
        }
        private async void LoadSettings()
        {
            await LoadRecommendations();
        }

        private async void btnEnableRecommendations_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsRecommendationsDisabled = false;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            grdNoVideosRecommended.Visibility = Visibility.Visible;
            grdDisabledVideosRecommended.Visibility = Visibility.Collapsed;
            LoadSettings();
        }

        private async void btnDisableRecommendations_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsRecommendationsDisabled = true;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            grdNoVideosRecommended.Visibility = Visibility.Collapsed;
            grdDisabledVideosRecommended.Visibility = Visibility.Visible;
        }
        private async Task LoadRecommendations()
        {
            var videosongs = FilesInDatabase.rawSongs.Where(p => VideoExtensions.List.Contains(Path.GetExtension(p.FilePath).ToLower()));
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var donotshowrecommendations = currentSettings.DoNotShowRecommendations;
            var excludedPaths = currentSettings.DoNotShowRecommendations?
                .Where(vp => vp?.FilePath != null)
                .Select(vp => vp!.FilePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
                ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var randomitems = FilesInDatabase.rawSongs
    .Where(p => !string.IsNullOrEmpty(p.FilePath)
             && VideoExtensions.List.Contains(Path.GetExtension(p.FilePath).ToLower())
             && !excludedPaths.Contains(p.FilePath)
             && File.Exists(p.FilePath))
    .OrderBy(_ => Random.Shared.Next())
    .Take(5)
    .ToList();
            foreach (var item in randomitems)
            {
                if (item.FilePath is string path)
                {
                    if (File.Exists(path))
                    {
                        var videoprogressitem = new VideoProgress { FilePath = path };
                        if (Extensions.VideoExtensions.List.Contains(Path.GetExtension(path).ToLower()))
                        {
                            videoprogressitem.FileName = Path.GetFileNameWithoutExtension(path);
                        }

                        RecommendedVideoList.Add(videoprogressitem);
                        var fallbackUri = "ms-appx:///Assets/default.png";
                        videoprogressitem.Thumbnail = new BitmapImage(new Uri(fallbackUri));

                        var task = Task.Run(async () =>
                        {
                            var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(path);
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
                                    videoprogressitem.Thumbnail = bitmap;
                                    videoprogressitem.ThumbnailPath = thumb;
                                    File.Delete(thumb);

                                }
                                catch (Exception ex)
                                {
                                    videoprogressitem.Thumbnail = new BitmapImage(new Uri(fallbackUri));
                                    Debug.WriteLine("An unexpected error occured: " + ex.Message);
                                }
                            });

                        });

                    }
                }
            }
        }
        private void mnftStartPreviousRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vdprg)
            {
                if (File.Exists(vdprg.FilePath))
                {
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(VideoPlayer), vdprg);
                    }
                }
            }
        }

        private void mnftStartFirstRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vdprg)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(VideoPlayer), vdprg.FilePath);
                }
            }
        }

        private async void mnftRemoveRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vdprg)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var donotshow = currentSettings.DoNotShowRecommendations;
                donotshow.Add(vdprg);
                RecommendedVideoList.Remove(vdprg);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }

        private async void mnftAddToFavRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vdprg)
            {
                var settings = await SettingsLoader.LoadSettingsAsync();
                var favourites = settings.Favourites;

                if (mnft.Text == "Add to Favourites")
                {
                    var exist = favourites.FirstOrDefault(p => p.FilePath == vdprg.FilePath);
                    if (exist == null)
                    {
                        favourites.Add(new FavouriteItems { FilePath = vdprg.FilePath });
                    }
                }
                else
                {
                    var exist = favourites.FirstOrDefault(p => p.FilePath == vdprg.FilePath);
                    if (exist != null)
                    {
                        favourites.Remove(exist);
                    }
                }
                await SettingsLoader.SaveSettingsAsync(settings);
            }

        }

        private void mnftFileInfoRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vd)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    FileInfo.RefreshValues -= FileInfo_RefreshValues1;
                    FileInfo.RefreshValues += FileInfo_RefreshValues1;
                    wind.ShowFileInfo(vd.FilePath);
                }
            }
        }
        private async void FileInfo_RefreshValues1()
        {
            RecommendedVideoList.Clear();
            //  await Task.Delay(1500);
            Debug.WriteLine("CKAUH");
            await LoadRecommendations();

        }
        private void mnftOpenFileLocRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vdprg)
            {
                if (File.Exists(vdprg.FilePath))
                {
                    Process.Start("explorer.exe", $"/select,\"{vdprg.FilePath}\"");
                }
            }
        }

        private void mnftCopyFilePathRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vdprg)
            {
                CopyToClipboard.CopyStringToClipboard(vdprg.FilePath);
            }
        }

        private async void MenuFlyout_Opened(object sender, object e)
        {
            var flyout = sender as MenuFlyout;
            if (flyout == null) return;
            var mnftAddtoFav = flyout?.Items
    .OfType<MenuFlyoutItem>()
    .FirstOrDefault(x => x.Name == "mnftAddToFavRec");


            var selectedsong = mnftAddtoFav?.DataContext as VideoProgress;
            if (selectedsong == null) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites;
            var exist = favourites.FirstOrDefault(o => o.FilePath == selectedsong.FilePath);
            bool isFav = false;

            if (exist != null)
            {
                isFav = true;
            }
            if (mnftAddtoFav == null) return;
            if (isFav == true)
            {
                mnftAddtoFav.Text = "Remove from Favourites";
            }
            else
            {

                mnftAddtoFav.Text = "Add to Favourites";
            }
        }

        private void grdvRecents_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is VideoProgress videoprogress)
            {
                if (chkSelect.IsChecked == false)
                {
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(VideoPlayer), videoprogress.FilePath);
                    }
                }
            }
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            grdvRecents.SelectionMode = chkSelect.IsChecked == true ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = chkSelect.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked == true) grdvRecents.SelectAll();
            else grdvRecents.SelectedItems.Clear();
        }

        private async void btnDoNotRecommend_Click(object sender, RoutedEventArgs e)
        {
            var selecteditems = grdvRecents.SelectedItems.Cast<VideoProgress>().ToList();
            var settings = await SettingsLoader.LoadSettingsAsync();
            var recommendations = settings.DoNotShowRecommendations;
            foreach (var item in selecteditems)
            {
                Debug.WriteLine(item.FilePath + " is going to be removed");
                RecommendedVideoList.Remove(item);

            }
            foreach (var item in selecteditems)
            {
                recommendations.Add(item);
            }
            await SettingsLoader.SaveSettingsAsync(settings);
            if (RecommendedVideoList.Count == 0)
            {
                LoadSettings();
            }
        }

        private void btnRefreshRecommendations_Click(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            ObservableCollection<SongModel> temp = new ObservableCollection<SongModel>();
            foreach (var item in RecommendedVideoList)
            {
                temp.Add(new SongModel { IsAudioItem = false, VisibilityofVideoInfo = Visibility.Visible, Glyph = "\uE8B2", VisibilityofAudioMeta = Visibility.Collapsed, FilePath = item.FilePath, Title = item.FileName });
            }

            QueueService.PlayMedia(temp, false, false);
        }
    }
}

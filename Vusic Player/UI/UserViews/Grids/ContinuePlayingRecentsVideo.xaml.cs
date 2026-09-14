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
using System.Threading.Tasks;
using Vortice.Direct2D1.Effects;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;
using Windows.Storage;
using FileInfo = Vusic_Player.Configuration.Helper.FileInfo;

namespace Vusic_Player.UI.UserViews.Grids
{
    public class CountToVisibility : IValueConverter
    {
        public class CountToVisibilityReverse : IValueConverter
        {
            public object Convert(object value, Type targetType, object parameter, string language)
            {
                if (value is int count)
                {
                    return count == 0 ? Visibility.Collapsed : Visibility.Visible;
                }

                return Visibility.Visible;
            }

            public object ConvertBack(object value, Type targetType, object parameter, string language)
            {
                throw new NotImplementedException();
            }
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed partial class ContinuePlayingRecentsVideo : UserControl
    {
        string HighlightVideoPath = "";

        public ContinuePlayingRecentsVideo()
        {
            InitializeComponent();
            grdvRecents.ItemsSource = VideoProgressList;

            ContinuePlaying.InvokeList += ContinuePlaying_InvokeList;
            LoadItems();
        }
        private async void LoadItems()
        {
            await LoadSettings();
        }
        ObservableCollection<VideoProgress> VideoProgressList = new();
        private async Task LoadSettings()
        {
            var settings = await SettingsLoader.LoadSettingsAsync();
            if (settings.IsVideoHistoryDisabled == true)
            {
                settings.SavedVideoProgress.Clear();
                grdHighlightVideo.Visibility = Visibility.Collapsed;
                stkEnabledEmptyRecents.Visibility = Visibility.Collapsed;
                stkDisabledEmptyRecents.Visibility = Visibility.Visible;
                await SettingsLoader.SaveSettingsAsync(settings);
                return;
            }
            else
            {
               
                stkEnabledEmptyRecents.Visibility = Visibility.Visible;
                stkDisabledEmptyRecents.Visibility = Visibility.Collapsed;
            }
  
            if (settings.SavedVideoProgress.Count == 0)
            {
                grdHighlightVideo.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Retrieve the item at the end of the list
                var lastSavedItem = settings.SavedVideoProgress.LastOrDefault();
                var remainingItems = settings.SavedVideoProgress.SkipLast(1).Reverse().ToList();

                if (lastSavedItem != null)
                {
                    ContinuePlaying.videoProgressMain = lastSavedItem;
                    ContinuePlaying.InvokeCall();

                }
                else
                {
                    grdHighlightVideo.Visibility = Visibility.Collapsed;
                }
                if (remainingItems.Count > 0)
                {

                    grdRecents.Visibility = Visibility.Visible;
                    grdEmptyRecents.Visibility = Visibility.Collapsed;

                    VideoProgressList.Clear();
                    foreach (var item in remainingItems)
                    {
                        if (item.FilePath is string path)
                        {
                            if (File.Exists(path))
                            {
                                var videoprogressitem = new VideoProgress { FilePath = path, IsEpisode = item.IsEpisode };
                                var file = await StorageFile.GetFileFromPathAsync(path);
                                string fileExtension = file.FileType.ToLowerInvariant();
                                if (Extensions.VideoExtensions.List.Contains(fileExtension))
                                {
                                    Debug.WriteLine("REQ PATH SI " + path);
                                    videoprogressitem.FileName = Path.GetFileNameWithoutExtension(path);
                                }
                                //    item.Thumbnail = await FileThumbnailObtain.ExtractVidThumbnailBasic(path, 0.25);

                                var totalduration = item.TotalDuration;
                                var currentduration = item.CurrentDuration;
                                var percent97 = 0.97 * totalduration;
                                if (currentduration < totalduration && currentduration <= percent97)
                                {
                                    videoprogressitem.CurrentDuration = currentduration;
                                    videoprogressitem.TotalDuration = totalduration;
                                    VideoProgressList.Add(videoprogressitem);
                                    var fallbackUri = "ms-appx:///Assets/default.png";
                                    videoprogressitem.Thumbnail = new BitmapImage(new Uri(fallbackUri));
                                    var percentage = (videoprogressitem.CurrentDuration / videoprogressitem.TotalDuration);

                                    var task = Task.Run(async () =>
                                    {
                                        var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(path, percentage);
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


                    // You can now use lastSavedItem.CurrentDuration to resume playback
                }
                else
                {
                    grdRecents.Visibility = Visibility.Collapsed;
                    grdEmptyRecents.Visibility = Visibility.Visible;
                }
            }
        }

        private async void ContinuePlaying_InvokeList()
        {
            if (ContinuePlaying.videoProgressMain is VideoProgress vd && vd != null)
            {
                if (vd.FilePath is string path)
                {
                    if (File.Exists(path))
                    {
                        HighlightVideoPath = path;
                        var storagefile = await StorageFile.GetFileFromPathAsync(path);
                        var videoprops = await storagefile.Properties.GetVideoPropertiesAsync();

                        txtFileName.Text = videoprops.Title;
                        Debug.WriteLine(videoprops.Title);
                        if (txtFileName.Text == "")
                        {
                            txtFileName.Text = Path.GetFileNameWithoutExtension(path);
                        }
                        ToolTipService.SetToolTip(grdHighlightVideo, Path.GetFileNameWithoutExtension(path));
                        var percentage = (vd.CurrentDuration / vd.TotalDuration);
                        Debug.WriteLine(percentage + " is the current percentage");

                        var task = Task.Run(async () =>
                        {
                            var thumb = await FileThumbnailObtain.ExtractVidThumbnailBasic(path, percentage);
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
                                        ContinuePlaying.videoProgressMain.Thumbnail = bitmap;
                                        ContinuePlaying.videoProgressMain.ThumbnailPath = thumb;
                                        //       Delete the file immediately after the stream closes safely
                                        File.Delete(thumb);
                                    }

                                    CoverBackground.ImageSource = bitmap;
                                }
                                catch (Exception ex)
                                {
                                    Debug.WriteLine("An unexpected error occurred: " + ex.Message);
                                }
                            });
                        });
                        prgHighlight.Maximum = vd.TotalDuration;
                        prgHighlight.Value = vd.CurrentDuration;
                        prgHighlight.IsEnabled = false;
                    }
                }
            }
        }

        private async void btnResume_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                prgAwaitResume.Visibility = Visibility.Visible;
                prgAwaitResume.IsActive = true;
                App.NavigationFrame.Navigate(typeof(VideoPlayer), ContinuePlaying.videoProgressMain);
            }


        }
        private void AnimateScale(double Scale)
        {
            var sb = new Storyboard();
            var animX = new DoubleAnimation { To = Scale, Duration = TimeSpan.FromMilliseconds(200) };
            var animY = new DoubleAnimation { To = Scale, Duration = TimeSpan.FromMilliseconds(200) };

            Storyboard.SetTarget(animX, GridScale);
            Storyboard.SetTargetProperty(animX, "ScaleX");
            Storyboard.SetTarget(animY, GridScale);
            Storyboard.SetTargetProperty(animY, "ScaleY");

            sb.Children.Add(animX);
            sb.Children.Add(animY);
            sb.Begin();
        }
        private void grdHighlightVideo_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(1.05);
        }

        private void grdHighlightVideo_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            AnimateScale(1.0);
        }

        private void GridContinuePlaying_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is VideoProgress videoprogress)
            {
                if (chkSelect.IsChecked == false)
                {
                    ContinuePlaying.videoProgressMain = videoprogress;
                    Logger.Log("IS IT EPISODE: " + videoprogress.IsEpisode, "continueplA", Logger.LogLevelType.Information);
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(VideoPlayer), videoprogress);
                    }
                }
            }
        }

        private void GridContinuePlaying_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked == true) grdvRecents.SelectAll();
            else grdvRecents.SelectedItems.Clear();
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            grdvRecents.SelectionMode = chkSelect.IsChecked == true ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = chkSelect.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chkSelect_Unchecked(object sender, RoutedEventArgs e)
        {
            grdvRecents.SelectionMode = chkSelect.IsChecked == true ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = chkSelect.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllContinuePlaying_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked == true) grdvRecents.SelectAll();
            else grdvRecents.SelectedItems.Clear();
        }

        private async void btnRemoveFromContinueWatchingSelected_Click(object sender, RoutedEventArgs e)
        {
            var selecteditems = grdvRecents.SelectedItems.Cast<VideoProgress>().ToList();
            var settings = await SettingsLoader.LoadSettingsAsync();
            var videoprogress = settings.SavedVideoProgress;
            foreach (var item in selecteditems)
            {
                Debug.WriteLine(item.FilePath + " is going to be removed");
                VideoProgressList.Remove(item);
               
            }
            foreach (var item in selecteditems)
            {
               
                var exist = videoprogress.FirstOrDefault(p => p.FilePath == item.FilePath);
                if (exist != null)
                {
                    videoprogress.Remove(exist);
                }
            }
            await SettingsLoader.SaveSettingsAsync(settings);
        }


        private void StackPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var element = (UIElement)sender;
            // Get the underlying visual that the Toolkit is already using
            var visual = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(element);
            var size = element.ActualSize; // Requires WinUI 3
            visual.CenterPoint = new System.Numerics.Vector3(size.X / 2, size.Y / 2, 0f);
            // Change the Scale on the Visual layer instead of the UIElement layer
            visual.Scale = new System.Numerics.Vector3(1.06f, 1.06f, 1.0f);
        }

        private void StackPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var element = (UIElement)sender;
            var visual = Microsoft.UI.Xaml.Hosting.ElementCompositionPreview.GetElementVisual(element);

            visual.Scale = new System.Numerics.Vector3(1.0f, 1.0f, 1.0f);
        }


        private async void mnftStartFirstCW_Click(object sender, RoutedEventArgs e)
        {
            if (ContinuePlaying.videoProgressMain != null)
            {
                prgAwaitResume.Visibility = Visibility.Visible;
                prgAwaitResume.IsActive = true;
                if (File.Exists(ContinuePlaying.videoProgressMain.FilePath))
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(VideoPlayer), ContinuePlaying.videoProgressMain.FilePath);
                    }

            }
        }

        private async void mnftRemoveCW_Click(object sender, RoutedEventArgs e)
        {
            var settings = await SettingsLoader.LoadSettingsAsync();

            // Retrieve the item at the end of the list
            var lastSavedItem = settings.SavedVideoProgress.LastOrDefault();
            if (lastSavedItem != null)
                settings.SavedVideoProgress.Remove(lastSavedItem);
            await SettingsLoader.SaveSettingsAsync(settings);
            await LoadSettings();
        }

        private async void mnftAddToFavCW_Click(object sender, RoutedEventArgs e)
        {
            if (ContinuePlaying.videoProgressMain == null) return;

            var settings = await SettingsLoader.LoadSettingsAsync();
            var favourites = settings.Favourites;

            if (mnftAddToFavCW.Text == "Add to Favourites")
            {
                var exist = favourites.FirstOrDefault(p => p.FilePath == ContinuePlaying.videoProgressMain.FilePath);
                if (exist == null)
                {
                    favourites.Add(new FavouriteItems { FilePath = ContinuePlaying.videoProgressMain.FilePath });
                }
            }
            else
            {
                var exist = favourites.FirstOrDefault(p => p.FilePath == ContinuePlaying.videoProgressMain.FilePath);
                if (exist != null)
                {
                    favourites.Remove(exist);
                }
            }
            await SettingsLoader.SaveSettingsAsync(settings);
        }

        private void mnftFileInfoCW_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance is MainWindow wind)
            {
                if (ContinuePlaying.videoProgressMain != null && ContinuePlaying.videoProgressMain.FilePath != null)
                    wind.ShowFileInfo(ContinuePlaying.videoProgressMain.FilePath);
                FileInfo.RefreshValues -= FileInfo_RefreshValues;
                FileInfo.RefreshValues -= FileInfo_RefreshValues1;
                FileInfo.RefreshValues += FileInfo_RefreshValues;
            }
        }

        private async void FileInfo_RefreshValues()
        {
            if (FileInfo.JustUpdatedRenamePath == HighlightVideoPath)
            {
                var storagefile = await StorageFile.GetFileFromPathAsync(HighlightVideoPath);
                var videoprops = await storagefile.Properties.GetVideoPropertiesAsync();

                txtFileName.Text = videoprops.Title;
            }
            else
            {
                if (ContinuePlaying.videoProgressMain != null)
                {
                    ContinuePlaying.videoProgressMain.FilePath = FileInfo.JustUpdatedRenamePath;
                    ContinuePlaying.InvokeCall();
                }
            }

        }

        private async void mnftOpenFileLocCW_Click(object sender, RoutedEventArgs e)
        {
            if (ContinuePlaying.videoProgressMain == null) return;
            var path = ContinuePlaying.videoProgressMain.FilePath;

            if (File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
        }

        private async void mnftCopyFilePathCW_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft)
            {
                if (ContinuePlaying.videoProgressMain == null) return;
                var path = ContinuePlaying.videoProgressMain.FilePath;
                if (path == null) return;
                CopyToClipboard.CopyStringToClipboard(path);
                ttCopiedToClipboard.IsOpen = true;
                await Task.Delay(2000);
                ttCopiedToClipboard.IsOpen = false;
            }
        }

        private async void mnftAddToQueueCW_Click(object sender, RoutedEventArgs e)
        {
            if (ContinuePlaying.videoProgressMain == null) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites;
            var exist = favourites.FirstOrDefault(o => o.FilePath == ContinuePlaying.videoProgressMain.FilePath);
            bool isFav = false;
            if (exist != null)
            {
                isFav = true;
            }
            var newsong = new SongModel { IsAudioItem = false, VisibilityofAudioMeta = Visibility.Collapsed, Title = ContinuePlaying.videoProgressMain.FileName, FilePath = ContinuePlaying.videoProgressMain.FilePath, IsEpisode = ContinuePlaying.videoProgressMain.IsEpisode, Glyph = "\uE8B2", VisibilityofVideoInfo = Visibility.Visible, IsFavourite = isFav, MediaType = "Video" };
            QueueService.VusicQueue.Add(newsong);
            QueueService.VusicQueueNext.Add(newsong);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("US");
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var savedRecents = currentSettings.SavedVideoProgress;
            savedRecents.Clear();
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private void mnftFileInfoRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vd)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    FileInfo.RefreshValues -= FileInfo_RefreshValues;
                    FileInfo.RefreshValues -= FileInfo_RefreshValues1;
                    FileInfo.RefreshValues += FileInfo_RefreshValues1;
                    wind.ShowFileInfo(vd.FilePath);
                }
            }
        }

        private async void FileInfo_RefreshValues1()
        {
            VideoProgressList.Clear();
            //  await Task.Delay(1500);
            Debug.WriteLine("CKAUH");
            await LoadSettings();

        }

        private async void mnftRemoveRec_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is VideoProgress vdprg)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var existingpath = currentSettings.SavedVideoProgress.FirstOrDefault(p => p.FilePath == vdprg.FilePath);
                if (existingpath != null)
                {
                    currentSettings.SavedVideoProgress.Remove(existingpath);
                }
                VideoProgressList.Remove(vdprg);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
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
    .FirstOrDefault(x => x.Name == "mnftAddToFavCW");

            if (mnftAddtoFav == null) return;
            if (ContinuePlaying.videoProgressMain == null) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites;
            var exist = favourites.FirstOrDefault(o => o.FilePath == ContinuePlaying.videoProgressMain.FilePath);
            bool isFav = false;
            if (exist != null)
            {
                isFav = true;
            }
            if (isFav == true)
            {
                mnftAddtoFav.Text = "Remove from Favourites";
            }
            else
            {

                mnftAddtoFav.Text = "Add to Favourites";
            }
        }

        private async void MenuFlyout_Opened_1(object sender, object e)
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

        private async void btnDisableRecents_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsVideoHistoryDisabled = true;
            currentSettings.SavedVideoProgress.Clear();
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            grdHighlightVideo.Visibility = Visibility.Collapsed;
            stkEnabledEmptyRecents.Visibility = Visibility.Collapsed;
            stkDisabledEmptyRecents.Visibility = Visibility.Visible;
        }

        private async void btnEnableRecents_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsVideoHistoryDisabled = false;
            currentSettings.SavedVideoProgress.Clear();
                
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            stkEnabledEmptyRecents.Visibility = Visibility.Visible;
            stkDisabledEmptyRecents.Visibility = Visibility.Collapsed;
        }
    }
}

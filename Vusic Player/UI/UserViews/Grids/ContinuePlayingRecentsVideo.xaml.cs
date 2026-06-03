using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
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
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;

namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class ContinuePlayingRecentsVideo : UserControl
    {
        string HighlightVideoPath = "";

        public ContinuePlayingRecentsVideo()
        {
            InitializeComponent();
            ContinuePlaying.InvokeList += ContinuePlaying_InvokeList;
            LoadSettings();
        }
        ObservableCollection<VideoProgress> VideoProgressList = new();
        private async void LoadSettings()
        {
            var settings = await SettingsLoader.LoadSettingsAsync();

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
                        item.FileName = Path.GetFileNameWithoutExtension(path);
                        item.Thumbnail = await FileThumbnailObtain.GetVideoFrameAsync(path, 0.25);
                    }
                    VideoProgressList.Add(item);
                    grdvRecents.ItemsSource = VideoProgressList;
                }
                // You can now use lastSavedItem.CurrentDuration to resume playback
            }
            else
            {
                grdRecents.Visibility = Visibility.Collapsed;
                grdEmptyRecents.Visibility = Visibility.Visible;
            }
        }

        private async void ContinuePlaying_InvokeList()
        {
            if (ContinuePlaying.videoProgressMain is VideoProgress vd && vd != null)
            {
                if (vd.FilePath is string path)
                {
                    HighlightVideoPath = path;
                    txtFileName.Text = Path.GetFileNameWithoutExtension(path);

                    CoverBackground.ImageSource = await FileThumbnailObtain.GetVideoFrameAsync(path, 0.25);
                }
                prgHighlight.Maximum = vd.TotalDuration;
                prgHighlight.Value = vd.CurrentDuration;
                prgHighlight.IsEnabled = false;

            }
        }

        private async void btnResume_Click(object sender, RoutedEventArgs e)
        {

            if (ContinuePlaying.videoProgressMain != null)
            {
                prgAwaitResume.Visibility = Visibility.Visible;
                prgAwaitResume.IsActive = true;
                await Task.Delay(200);
          //      PlayerService.VideoInvoke();
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

        private void btnRemoveFromContinueWatchingSelected_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnRemoveFromContinueWatchingSelected_Click_1(object sender, RoutedEventArgs e)
        {

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
            LoadSettings();
        }

        private void mnftAddToFavCW_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftFileInfoCW_Click(object sender, RoutedEventArgs e)
        {

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

        private void mnftAddToQueueCW_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}

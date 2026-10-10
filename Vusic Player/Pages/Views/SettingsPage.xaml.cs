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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Vusic_Player.Pages.Views
{


    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }
        private async void LoadSettings()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            chkSaveHistory.IsChecked = !currentSettings.IsVideoHistoryDisabled;

            // Home Page Panels (Panel1VisibilityHome - Panel4VisibilityHome)
            chkHomeContinueRecent.IsChecked = currentSettings.Panel1VisibilityHome;
            chkHomeRecentMusic.IsChecked = currentSettings.Panel2VisibilityHome;
            chkHomeMusicMix.IsChecked = currentSettings.Panel3VisibilityHome;
            chkHomeFavouritesRecommendations.IsChecked = currentSettings.Panel4VisibilityHome;

            // Music Panels (Panel1VisibilityMusic - Panel4VisibilityMusic)
            chkMusicRecent.IsChecked = currentSettings.Panel1VisibilityMusic;
            chkMusicPlaylists.IsChecked = currentSettings.Panel2VisibilityMusic;
            chkMusicInstantPlaylists.IsChecked = currentSettings.Panel3VisibilityMusic;
            chkMusicGenreRecommendations.IsChecked = currentSettings.Panel4VisibilityMusic;

            // Video Panels (Panel1VisibilityVideo - Panel4VisibilityVideo)
            chkVideoContinueRecent.IsChecked = currentSettings.Panel1VisibilityVideo;
            chkVideoRecommended.IsChecked = currentSettings.Panel2VisibilityVideo;
            chkVideoPlaylists.IsChecked = currentSettings.Panel3VisibilityVideo;
            chkVideoShows.IsChecked = currentSettings.Panel4VisibilityVideo;
        }
        private void btnViewLog_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(LoggerPage));
            }
        }

        private void nvgNavigationMain_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer == null)
                return;
            grdAppSettings.Visibility = Visibility.Collapsed;
            grdHomeOptions.Visibility = Visibility.Collapsed;
            frmAboutOptions.Visibility = Visibility.Collapsed;
            grdMusicOptions.Visibility = Visibility.Collapsed;
            grdVideoOptions.Visibility = Visibility.Collapsed;
            lstHiddenVideos.Visibility = Visibility.Collapsed;
            hypBlockedVideos.Content = "View hidden videos";
            if (args.SelectedItemContainer == nvgitHomePage)
            {
                grdHomeOptions.Visibility = Visibility.Visible;
            }

            else if (args.SelectedItemContainer == nvgitMusicOptions)
            {
                grdMusicOptions.Visibility = Visibility.Visible;
            }

            else if (args.SelectedItemContainer == nvgitVideoOptions)
            {
                grdVideoOptions.Visibility = Visibility.Visible;
            }

            else if (args.SelectedItemContainer == nvgitAppSettings)
            {
                grdAppSettings.Visibility = Visibility.Visible;
            }
            else if (args.SelectedItemContainer == nvgitAboutHelp)
            {
                frmAboutOptions.Visibility = Visibility.Visible;
            }

        }

        private async void btnDeleteAllPlaylists_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var playlists = currentSettings.SavedPlaylists;
                playlists.Clear();
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "SettingsPage.DeleteAllPlaylists", Logger.LogLevelType.Error);
                txtInfo.Text = "An unexpected error occured. See log page for more info.";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/error.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
            finally
            {
                txtInfo.Text = "Successfully deleted all playlists!";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/success.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
        }


        private async void btnDeleteAllShows_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var shows = currentSettings.Shows;
                shows.Clear();
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "SettingsPage.DeleteAllShows", Logger.LogLevelType.Error);
                txtInfo.Text = "An unexpected error occured. See log page for more info.";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/error.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
            finally
            {
                txtInfo.Text = "Successfully deleted all shows!";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/success.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
        }

        private async void btnDeleteAllVideoHistory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var videoprogress = currentSettings.SavedVideoProgress;
                videoprogress.Clear();
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "SettingsPage.DeleteAllVideos", Logger.LogLevelType.Error);
                txtInfo.Text = "An unexpected error occured. See log page for more info.";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/error.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
            finally
            {
                txtInfo.Text = "Successfully deleted video history!";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/success.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }

        }

        private async void chkSaveHistory_Checked(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (chkSaveHistory.IsChecked == false)
            {
                currentSettings.IsVideoHistoryDisabled = true;
            }
            else
            {
                currentSettings.IsVideoHistoryDisabled = false;

            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        ObservableCollection<HiddenMediaItem> HiddenVideos = new ObservableCollection<HiddenMediaItem>();
        private async void btnRemoveVideoFromHiddenList_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is HiddenMediaItem hidden)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var hiddenmedia = currentSettings.HiddenMedia;

                var exist = hiddenmedia.FirstOrDefault(p => p.FilePath == hidden.FilePath);
                if (exist != null)
                {
                    hiddenmedia.Remove(exist);
                }
                var exist2 = HiddenVideos.FirstOrDefault(p => p.FilePath == hidden.FilePath);
                if (exist2 != null)
                {
                    HiddenVideos.Remove(exist2);
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }

        }

        public Visibility GetEmptyVisibility(int count)
        {

            return count == 0 ? Visibility.Visible : Visibility.Collapsed;


        }
        private async void hypBlockedVideos_Click(object sender, RoutedEventArgs e)
        {
            HiddenVideos.Clear();
            if (hypBlockedVideos.Content is string str && str == "View hidden videos")
            {
                grdHiddenVideos.Visibility = Visibility.Visible;
                hypBlockedVideos.Content = "Hide hidden video list";

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var hiddenmedia = currentSettings.HiddenMedia;

                foreach (var hiddenvid in hiddenmedia)
                {
                    HiddenVideos.Add(new HiddenMediaItem { FilePath = hiddenvid.FilePath });
                }

            }
            else
            {
                grdHiddenVideos.Visibility = Visibility.Collapsed;
                hypBlockedVideos.Content = "View hidden videos";
            }
        }

        private async void chkHidePanel1_Checked(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if(sender is CheckBox chkbox && chkbox.Content is string str)
            {
                if(str == "Continue Playing and Recent Videos")
                {
                    if(chkbox.IsChecked == true)
                    {
                        currentSettings.Panel1VisibilityHome = true;
                    }
                    else
                    {
                        currentSettings.Panel1VisibilityHome = false;
                    }
                }
                else if (str == "Recent Music")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel2VisibilityHome = true;
                    }
                    else
                    {
                        currentSettings.Panel2VisibilityHome = false;
                    }
                }
                else if (str == "Music Mix")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel3VisibilityHome = true;
                    }
                    else
                    {
                        currentSettings.Panel3VisibilityHome = false;
                    }
                }
                else if (str == "Favourites Recommendations")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel4VisibilityHome = true;
                    }
                    else
                    {
                        currentSettings.Panel4VisibilityHome = false;
                    }
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }

        private async void chkHidePanel2_Checked(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (sender is CheckBox chkbox && chkbox.Content is string str)
            {
                if (str == "Recent Music")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel1VisibilityMusic = true;
                    }
                    else
                    {
                        currentSettings.Panel1VisibilityMusic = false;
                    }
                }
                else if (str == "Playlists")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel2VisibilityMusic = true;
                    }
                    else
                    {
                        currentSettings.Panel2VisibilityMusic = false;
                    }
                }
                else if (str == "Instant Playlists")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel3VisibilityMusic = true;
                    }
                    else
                    {
                        currentSettings.Panel3VisibilityMusic = false;
                    }
                }
                else if (str == "Genre Recommendations")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel4VisibilityMusic = true;
                    }
                    else
                    {
                        currentSettings.Panel4VisibilityMusic = false;
                    }
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }

        private async void chkHidePanel3_Checked(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (sender is CheckBox chkbox && chkbox.Content is string str)
            {
                if (str == "Continue Playing and Recent Videos")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel1VisibilityVideo = true;
                    }
                    else
                    {
                        currentSettings.Panel1VisibilityVideo = false;
                    }
                }
                else if (str == "Recommended Videos")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel2VisibilityVideo = true;
                    }
                    else
                    {
                        currentSettings.Panel2VisibilityVideo = false;
                    }
                }
                else if (str == "Playlists")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel3VisibilityVideo = true;
                    }
                    else
                    {
                        currentSettings.Panel3VisibilityVideo = false;
                    }
                }
                else if (str == "Shows")
                {
                    if (chkbox.IsChecked == true)
                    {
                        currentSettings.Panel4VisibilityVideo = true;
                    }
                    else
                    {
                        currentSettings.Panel4VisibilityVideo = false;
                    }
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }

    }
}

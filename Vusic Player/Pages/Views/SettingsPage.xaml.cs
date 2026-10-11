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
using Vusic_Player.Configuration.Helper.UI.Customization;
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

            _isSyncing = true;
            try
            {
                // --- Skip Backward ---
                // --- Skip Backward ---
                double skipBack = currentSettings.SkipBack;
                string skipBackIconName;

                switch (skipBack)
                {
                    case 5:
                    case 10:
                    case 15:
                    case 20:
                    case 30:
                        cmbSkipBackward.SelectedIndex = skipBack switch
                        {
                            5 => 0,
                            10 => 1,
                            15 => 2,
                            20 => 3,
                            _ => 4
                        };
                        numCustomSkipBack.Visibility = Visibility.Collapsed;
                        skipBackIconName = $"skip{skipBack}back.png";
                        break;

                    default:
                        // Custom duration or unmapped value
                        cmbSkipBackward.SelectedIndex = 5;
                        numCustomSkipBack.Visibility = Visibility.Visible;
                        numCustomSkipBack.Value = skipBack;
                        skipBackIconName = "skipbackcustom.png";
                        break;
                }



                // --- Skip Forward ---
                double skipAhead = currentSettings.SkipAhead;
                string skipAheadIconName;

                switch (skipAhead)
                {
                    case 5:
                    case 10:
                    case 15:
                    case 20:
                    case 30:
                        cmbSkipForward.SelectedIndex = skipAhead switch
                        {
                            5 => 0,
                            10 => 1,
                            15 => 2,
                            20 => 3,
                            _ => 4
                        };
                        numCustomSkipForward.Visibility = Visibility.Collapsed;
                        skipAheadIconName = $"skip{skipAhead}ahead.png";
                        break;

                    default:
                        // Custom duration or unmapped value
                        cmbSkipForward.SelectedIndex = 5;
                        numCustomSkipForward.Visibility = Visibility.Visible;
                        numCustomSkipForward.Value = skipAhead;
                        skipAheadIconName = "skipaheadcustom.png";
                        break;
                }

            }
            finally
            {
                _isSyncing = false;
            }
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
            grdGeneral.Visibility = Visibility.Collapsed;
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
            else if (args.SelectedItemContainer == nvgitPlayback)
            {
                grdGeneral.Visibility = Visibility.Visible;
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
            if (sender is CheckBox chkbox && chkbox.Content is string str)
            {
                if (str == "Continue Playing and Recent Videos")
                {
                    if (chkbox.IsChecked == true)
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
        public IconHolder IconsInstance => IconHolder.Instance;

        private async void cmbSkipBackward_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSyncing) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (currentSettings == null) return;

            numCustomSkipBack.Visibility = Visibility.Collapsed;
            double skip;
            string skipBackIcon;

            switch (cmbSkipBackward.SelectedIndex)
            {
                case 0:
                    skip = 5;
                    skipBackIcon = "skip5back.png";
                    break;

                case 1:
                    skip = 10;
                    skipBackIcon = "skip10back.png";
                    break;

                case 2:
                    skip = 15;
                    skipBackIcon = "skip15back.png";
                    break;

                case 3:
                    skip = 20;
                    skipBackIcon = "skip20back.png";
                    break;

                case 4:
                    skip = 30;
                    skipBackIcon = "skip30back.png";
                    break;

                case 5:
                    numCustomSkipBack.Visibility = Visibility.Visible;
                    skip = !double.IsNaN(numCustomSkipBack.Value) ? numCustomSkipBack.Value : currentSettings.SkipBack;
                    skipBackIcon = "skipbackcustom.png";
                    break;

                default:
                    skip = 10;
                    skipBackIcon = "skip10back.png";
                    break;
            }

            IconsInstance.SkipBack = $"ms-appx:///Assets/Themes/{IconsInstance.Theme}/{skipBackIcon}";
            currentSettings.SkipBack = skip;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private async void cmbSkipForward_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isSyncing) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (currentSettings == null) return;

            numCustomSkipForward.Visibility = Visibility.Collapsed;
            double skipAhead;
            string skipAheadIcon;

            switch (cmbSkipForward.SelectedIndex)
            {
                case 0:
                    skipAhead = 5;
                    skipAheadIcon = "skip5ahead.png";
                    break;

                case 1:
                    skipAhead = 10;
                    skipAheadIcon = "skip10ahead.png";
                    break;

                case 2:
                    skipAhead = 15;
                    skipAheadIcon = "skip15ahead.png";
                    break;

                case 3:
                    skipAhead = 20;
                    skipAheadIcon = "skip20ahead.png";
                    break;

                case 4:
                    skipAhead = 30;
                    skipAheadIcon = "skip30ahead.png";
                    break;

                case 5:
                    numCustomSkipForward.Visibility = Visibility.Visible;
                    skipAhead = !double.IsNaN(numCustomSkipForward.Value) ? numCustomSkipForward.Value : currentSettings.SkipAhead;
                    skipAheadIcon = "skipaheadcustom.png";
                    break;

                default:
                    skipAhead = 10;
                    skipAheadIcon = "skip10ahead.png";
                    break;
            }

            IconsInstance.SkipAhead = $"ms-appx:///Assets/Themes/{IconsInstance.Theme}/{skipAheadIcon}";
            currentSettings.SkipAhead = skipAhead;
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        private bool _isSyncing = false;

        private async void numCustomSkipBack_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (_isSyncing || double.IsNaN(sender.Value)) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (currentSettings == null) return;

            double skip = sender.Value;
            _isSyncing = true;

            try
            {
                string iconName;
                switch (skip)
                {
                    case 5:
                        cmbSkipBackward.SelectedIndex = 0;
                        numCustomSkipBack.Visibility = Visibility.Collapsed;
                        iconName = "skip5back.png";
                        break;
                    case 10:
                        cmbSkipBackward.SelectedIndex = 1;
                        numCustomSkipBack.Visibility = Visibility.Collapsed;
                        iconName = "skip10back.png";
                        break;
                    case 15:
                        cmbSkipBackward.SelectedIndex = 2;
                        numCustomSkipBack.Visibility = Visibility.Collapsed;
                        iconName = "skip15back.png";
                        break;
                    case 20:
                        cmbSkipBackward.SelectedIndex = 3;
                        numCustomSkipBack.Visibility = Visibility.Collapsed;
                        iconName = "skip20back.png";
                        break;
                    case 30:
                        cmbSkipBackward.SelectedIndex = 4;
                        numCustomSkipBack.Visibility = Visibility.Collapsed;
                        iconName = "skip30back.png";
                        break;
                    default:
                        cmbSkipBackward.SelectedIndex = 5;
                        numCustomSkipBack.Visibility = Visibility.Visible;
                        iconName = "skipbackcustom.png";
                        break;
                }

                IconsInstance.SkipBack = $"ms-appx:///Assets/Themes/{IconsInstance.Theme}/{iconName}";
                currentSettings.SkipBack = skip;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            finally
            {
                _isSyncing = false;
            }
        }

        private async void numCustomSkipAhead_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args)
        {
            if (_isSyncing || double.IsNaN(sender.Value)) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (currentSettings == null) return;

            double skipAhead = sender.Value;
            _isSyncing = true;

            try
            {
                string iconName;
                switch (skipAhead)
                {
                    case 5:
                        cmbSkipForward.SelectedIndex = 0;
                        numCustomSkipForward.Visibility = Visibility.Collapsed;
                        iconName = "skip5ahead.png";
                        break;
                    case 10:
                        cmbSkipForward.SelectedIndex = 1;
                        numCustomSkipForward.Visibility = Visibility.Collapsed;
                        iconName = "skip10ahead.png";
                        break;
                    case 15:
                        cmbSkipForward.SelectedIndex = 2;
                        numCustomSkipForward.Visibility = Visibility.Collapsed;
                        iconName = "skip15ahead.png";
                        break;
                    case 20:
                        cmbSkipForward.SelectedIndex = 3;
                        numCustomSkipForward.Visibility = Visibility.Collapsed;
                        iconName = "skip20ahead.png";
                        break;
                    case 30:
                        cmbSkipForward.SelectedIndex = 4;
                        numCustomSkipForward.Visibility = Visibility.Collapsed;
                        iconName = "skip30ahead.png";
                        break;
                    default:
                        cmbSkipForward.SelectedIndex = 5;
                        numCustomSkipForward.Visibility = Visibility.Visible;
                        iconName = "skipaheadcustom.png";
                        break;
                }

                IconsInstance.SkipAhead = $"ms-appx:///Assets/Themes/{IconsInstance.Theme}/{iconName}";
                currentSettings.SkipAhead = skipAhead;
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            finally
            {
                _isSyncing = false;
            }
        }
    }
}

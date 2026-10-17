using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;


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
            chkShowSplash.IsChecked = currentSettings.IsSplashEnabled;
            if (currentSettings.StartupPage == "Home")
            {
                cmbStartupPage.SelectedIndex = 0;
            }
            else if (currentSettings.StartupPage == "Music")
            {
                cmbStartupPage.SelectedIndex = 1;
            }
            else if (currentSettings.StartupPage == "Video")
            {
                cmbStartupPage.SelectedIndex = 2;
            }
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
                LoadFonts();

            }
            else if (args.SelectedItemContainer == nvgitVideoOptions)
            {
                grdVideoOptions.Visibility = Visibility.Visible;
            }

            else if (args.SelectedItemContainer == nvgitAppSettings)
            {
                grdAppSettings.Visibility = Visibility.Visible;
                LoadFolders();
            }
            else if (args.SelectedItemContainer == nvgitAboutHelp)
            {
                frmAboutOptions.Visibility = Visibility.Visible;
                CheckForUpdates();
            }

        }
        List<FontStyleModel> systemFonts = new List<FontStyleModel>();
        private void LoadFonts()
        {
            var systemFontstemp = Microsoft.Graphics.Canvas.Text.CanvasTextFormat.GetSystemFontFamilies()
                              .OrderBy(f => f)
                              .ToList();

            systemFonts.Clear(); // Ensure list is clean
            foreach (var font in systemFontstemp)
            {
                systemFonts.Add(new Configuration.ClassModels.FontStyleModel
                {
                    FontFam = new FontFamily(font),
                    FontName = font
                });
            }

            cmbFontFamilies.ItemsSource = systemFonts;

            if (Application.Current.Resources.TryGetValue("GlobalFontHolder", out object resource)
                && resource is FontFamily currentFont)
            {
                string fontName = currentFont.Source;
                var def = systemFonts.FirstOrDefault(p => p.FontName.Equals(fontName, StringComparison.OrdinalIgnoreCase))
                       ?? systemFonts.FirstOrDefault(p => p.FontName.Equals("Segoe UI Variable Text", StringComparison.OrdinalIgnoreCase))
                       ?? systemFonts.FirstOrDefault();

                cmbFontFamilies.SelectedItem = def;
            }
        }

        private void asbSearchFormats_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                string query = sender.Text?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(query))
                {
                    sender.ItemsSource = null;
                    return;
                }

                // Return font names matching the search query
                sender.ItemsSource = systemFonts
                    .Where(f => f.FontName.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Select(f => f.FontName)
                    .ToList();
            }
        }

        private void asbSearchFormats_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            if (args.SelectedItem is string chosenFontName)
            {
                // Select matching item directly from the loaded systemFonts list
                var matchedModel = systemFonts.FirstOrDefault(f =>
                    string.Equals(f.FontName, chosenFontName, StringComparison.OrdinalIgnoreCase));

                if (matchedModel != null)
                {
                    ApplySelectedFormat(sender, matchedModel);
                }
            }
        }

        private void asbSearchFormats_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string targetName = args.ChosenSuggestion as string;

            if (string.IsNullOrWhiteSpace(targetName))
            {
                targetName = args.QueryText?.Trim();
            }

            if (string.IsNullOrWhiteSpace(targetName)) return;

            // Search existing font list for exact match first, then partial match
            var matchedModel = systemFonts.FirstOrDefault(f =>
                                   string.Equals(f.FontName, targetName, StringComparison.OrdinalIgnoreCase))
                            ?? systemFonts.FirstOrDefault(f =>
                                   f.FontName.Contains(targetName, StringComparison.OrdinalIgnoreCase));

            if (matchedModel != null)
            {
                ApplySelectedFormat(sender, matchedModel);
            }
        }

        private void ApplySelectedFormat(AutoSuggestBox sender, FontStyleModel format)
        {
            // 1. Clear search text
            sender.Text = string.Empty;

            // 2. Select the existing reference in the ComboBox
            cmbFontFamilies.SelectedItem = format;

            // 3. Close the Flyout if hosted inside one
            DependencyObject current = sender;
            while (current != null && !(current is FlyoutPresenter))
            {
                current = VisualTreeHelper.GetParent(current);
            }

            if (current is FlyoutPresenter fp && fp.Parent is Popup popup)
            {
                popup.IsOpen = false;
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

        private async void btnDeleteAllArtists_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var artistModels = currentSettings.ArtistsList;
                artistModels.Clear();
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "SettingsPage.DeleteAllArtists", Logger.LogLevelType.Error);
                txtInfo.Text = "An unexpected error occured. See log page for more info.";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/error.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
            finally
            {
                txtInfo.Text = "Successfully deleted all artist profile pictures!";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/success.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }

        }

        private async void btnDeleteAllAlbums_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var artistModels = currentSettings.AlbumsList;
                artistModels.Clear();
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "SettingsPage.DeleteAllAlbums", Logger.LogLevelType.Error);
                txtInfo.Text = "An unexpected error occured. See log page for more info.";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/error.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
            finally
            {
                txtInfo.Text = "Successfully deleted all album covers!";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/success.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }

        }

        private async void btnDeleteAllGenres_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var artistModels = currentSettings.GenresList;
                artistModels.Clear();
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "SettingsPage.DeleteAllGenres", Logger.LogLevelType.Error);
                txtInfo.Text = "An unexpected error occured. See log page for more info.";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/error.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
            finally
            {
                txtInfo.Text = "Successfully deleted all genre covers!";
                ttInfo.IsOpen = true;
                imgInfo.Source = new BitmapImage(new Uri("ms-appx:///Assets/success.png")); ;
                await Task.Delay(2000);
                ttInfo.IsOpen = false;
            }
        }
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string VersionEndpoint = "https://raw.githubusercontent.com/OceanCyanTech/Vusic-Player/refs/heads/master/Version/version.json";

        public async Task<bool> IsUpdateAvailableAsync()
        {
            try
            {
                // Avoid caching so the app always gets fresh data
                using var request = new HttpRequestMessage(HttpMethod.Get, VersionEndpoint);
                request.Headers.CacheControl = new System.Net.Http.Headers.CacheControlHeaderValue { NoCache = true };

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode) return false;

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                string remoteVersionStr = doc.RootElement.GetProperty("latestVersion").GetString() ?? "";
                Version remoteVersion = Version.Parse(remoteVersionStr);
                Debug.WriteLine(remoteVersionStr);
                // Get current running app package version
                var packageVersion = Package.Current.Id.Version;
                Version currentVersion = new Version(packageVersion.Major, packageVersion.Minor, packageVersion.Build);

                // True if remote is strictly newer
                return remoteVersion > currentVersion;
            }
            catch (Exception ex)
            {
                // Network drop, offline mode, or parse failure - fail silently
                System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
                return false;
            }
        }
        private async void btnCheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            imgUpdate.Visibility = Visibility.Collapsed;
            prgCheckingForUpdates.Visibility = Visibility.Visible;
            btnOpenStore.Visibility = Visibility.Collapsed;
            hypWhatsNew.Visibility = Visibility.Collapsed;
            CheckForUpdates();
        }
        private async void CheckForUpdates()
        {
            if (CheckInternet.IsInternetAvailable())
            {
                if (await IsUpdateAvailableAsync())
                {
                    txtUpdateInfo.Text = "A new version of Vusic Player is available!";

                    imgUpdate.Source = new BitmapImage(new Uri(IconsInstance.InfoIcon));
                    imgUpdate.Visibility = Visibility.Visible;
                    prgCheckingForUpdates.Visibility = Visibility.Collapsed;
                    btnOpenStore.Visibility = Visibility.Visible;
                    hypWhatsNew.Visibility = Visibility.Visible;
                }
                else
                {
                    txtUpdateInfo.Text = "You are on the latest version of Vusic Player!";

                    imgUpdate.Source = new BitmapImage(new Uri(IconsInstance.Success));
                    imgUpdate.Visibility = Visibility.Visible;
                    prgCheckingForUpdates.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                txtUpdateInfo.Text = "Cannot check for updates. Make sure you're internet connection is stable.";
                imgUpdate.Source = new BitmapImage(new Uri(IconsInstance.Error));
                imgUpdate.Visibility = Visibility.Visible;
                prgCheckingForUpdates.Visibility = Visibility.Collapsed;
            }

        }
        private async void btnOpenStore_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(
    new Uri("ms-windows-store://pdp/?ProductId=YOUR_12_CHAR_ID")
);
        }



        private void btnRefreshFonts_Click(object sender, RoutedEventArgs e)
        {
            LoadFonts();
        }

        private async void cmbFontFamilies_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            if (cmbFontFamilies.SelectedItem is FontStyleModel fontst && fontst.FontName is string str)
            {
                Application.Current.Resources["GlobalFontHolder"] = str;
                if (currentSettings.FontFamily != str)
                {
                    stkRestartApp.Visibility = Visibility.Visible;
                }
                currentSettings.FontFamily = str;
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private async void btnResetFont_Click(object sender, RoutedEventArgs e)
        {
            var defaultfont = systemFonts.FirstOrDefault(p => p.FontName == "Segoe UI Variable Display");
            if (defaultfont != null)
            {
                cmbFontFamilies.SelectedItem = defaultfont;
            }
        }

        private void btnRestartVusicPlayer_Click(object sender, RoutedEventArgs e)
        {
            AppRestartFailureReason result = AppInstance.Restart(string.Empty);

            // If restart succeeds, the current process terminates automatically.
            // If it fails, inspect the failure reason:
            switch (result)
            {
                case AppRestartFailureReason.RestartPending:
                    // A restart request is already processing
                    break;
                case AppRestartFailureReason.InvalidUser:
                    // The user token is invalid
                    break;
                case AppRestartFailureReason.Other:
                    // General failure, handle fallback
                    break;
            }
        }

        private async void btnRefreshDatabase_Click(object sender, RoutedEventArgs e)
        {
            // Prevent re-entrancy
            btnRefreshDatabase.IsEnabled = false;

            try
            {
                ttInfo.IsOpen = true;
                txtInfo.Text = "Clearing media database...";
                imgInfo.Source = new BitmapImage(new Uri(IconsInstance.Refresh));

                // 1. Clear database and reset state
                await DatabaseService.ClearDatabaseAsync();
                await FilesInDatabase.ResetLibraryAsync();

                txtInfo.Text = "Media Database cleared successfully.";
                await Task.Delay(1000); // Shorter visual confirmation

                // 2. Reload data
                txtInfo.Text = "Re-scanning media library...";
                await FilesInDatabase.InitializeDatabaseFunctionsAsync();


                // 3. Await the background service, or track its task explicitly
                await FilesInDatabase.RunBackgroundServiceAsync();
                await FilesInDatabase.LoadAllFiles();
                txtInfo.Text = "Media Database successfully re-scanned.";
                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                txtInfo.Text = $"Database refresh failed: {ex.Message}";
                await Task.Delay(3000);
            }
            finally
            {
                ttInfo.IsOpen = false;
                btnRefreshDatabase.IsEnabled = true;
            }
        }
        ObservableCollection<FoldersListOpened> foldersListOpened = new();

        private async void LoadFolders()
        {
            UserDataPaths paths = UserDataPaths.GetDefault();
            grdViewFolders.ItemsSource = foldersListOpened;
            // Assigning the system paths to the Tag property

            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            var folders = currentSettings.SavedFoldersOpened;
            var folderMappings = new (bool IsEnabled, string Path)[]
    {
    (currentSettings.IsDocumentEnabled, paths.Documents),
    (currentSettings.IsDownloadsEnabled, paths.Downloads),
    (currentSettings.IsPicturesEnabled, paths.Pictures),
    (currentSettings.IsVideosEnabled, paths.Videos),
    (currentSettings.IsMusicEnabled, paths.Music),
    };

            foreach (var (isEnabled, path) in folderMappings)
            {
                if (isEnabled)
                {
                    var exist = foldersListOpened.FirstOrDefault(p => p.FolderPath == path);
                    if (exist == null)
                    {
                        foldersListOpened.Add(new FoldersListOpened { FolderPath = path, IsHighLevelFolder = true, FolderName = Path.GetFileName(path), isChecked = true });
                    }
                }
            }
            foreach (var item in currentSettings.SavedFoldersOpened)
            {
                if (item.Show == true)
                {
                    var exist = foldersListOpened.FirstOrDefault(p => p.FolderPath == item.FolderPath);
                    if (exist == null)
                    {
                        item.FolderName = Path.GetFileName(item.FolderPath);
                        foldersListOpened.Add(item);
                    }

                }
                else
                {
                    Debug.WriteLine("item show = false " + item.FolderPath);
                    var exist = foldersListOpened.FirstOrDefault(p => p.FolderPath == item.FolderPath);
                    if (exist != null)
                    {
                        if (exist.Show == false)
                        {

                            foldersListOpened.Remove(item);
                        }
                    }
                }
            }
        }

        private async void btnGenericFolder_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton tgl && tgl.DataContext is FoldersListOpened folder)
            {
                tgl.IsEnabled = false; // Prevent rapid re-toggling during I/O

                try
                {
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    UserDataPaths paths = UserDataPaths.GetDefault();
                    bool isChecked = tgl.IsChecked ?? false;

                    // 1. Update in-memory state
                    folder.isChecked = isChecked;

                    // 2. Persist to settings
                    if (folder.IsHighLevelFolder)
                    {
                        switch (folder.FolderPath)
                        {
                            case var p when p == paths.Documents: currentSettings.IsDocumentEnabled = isChecked; break;
                            case var p when p == paths.Downloads: currentSettings.IsDownloadsEnabled = isChecked; break;
                            case var p when p == paths.Pictures: currentSettings.IsPicturesEnabled = isChecked; break;
                            case var p when p == paths.Videos: currentSettings.IsVideosEnabled = isChecked; break;
                            case var p when p == paths.Music: currentSettings.IsMusicEnabled = isChecked; break;
                        }
                    }
                    else
                    {
                        var saved = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.FolderPath);
                        if (saved != null)
                        {
                            saved.isChecked = isChecked;
                        }
                    }

                    await SettingsLoader.SaveSettingsAsync(currentSettings);

                    // 3. Dynamically purge or index without restarting
                    if (!isChecked)
                    {
                        await FilesInDatabase.DisableFolderAsync(folder.FolderPath);
                    }
                    else
                    {
                        await FilesInDatabase.EnableFolderAsync(folder.FolderPath);
                    }
                }
                finally
                {
                    tgl.IsEnabled = true;
                }
            }
        }

        private void mnftOpenFolder_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {

                if (tgl.FolderPath != null)
                {

                    if (Directory.Exists(tgl.FolderPath))
                    {

                        Process.Start("explorer.exe", $"\"{tgl.FolderPath}\"");
                    }
                }
            }
        }

        private void mnftCopyPathFolder_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {
                if (tgl.FolderPath is string str)
                {
                    CopyToClipboard.CopyStringToClipboard(str);
                }
            }
        }

        private async void mnftRemoveFolder_Click(object sender, RoutedEventArgs e)
        {
            //Remove Folder from the List
            if (sender is MenuFlyoutItem MNFT && MNFT.DataContext is FoldersListOpened tgl)
            {
                foldersListOpened.Remove(tgl);
                UserDataPaths paths = UserDataPaths.GetDefault();

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                if (tgl.IsHighLevelFolder)
                {
                    bool isChecked = false;

                    switch (tgl.FolderPath)
                    {
                        case var p when p == paths.Documents:
                            currentSettings.IsDocumentEnabled = isChecked;
                            break;
                        case var p when p == paths.Downloads:
                            currentSettings.IsDownloadsEnabled = isChecked;
                            break;
                        case var p when p == paths.Pictures:
                            currentSettings.IsPicturesEnabled = isChecked;
                            break;
                        case var p when p == paths.Videos:
                            currentSettings.IsVideosEnabled = isChecked;
                            break;
                        case var p when p == paths.Music:
                            currentSettings.IsMusicEnabled = isChecked;
                            break;
                    }
                    await SettingsLoader.SaveSettingsAsync(currentSettings);

                }
                else
                {
                    if (tgl.FolderPath is string str)
                    {
                        var folder = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == str);
                        if (folder != null)
                        {
                            Debug.WriteLine(str);
                            currentSettings.SavedFoldersOpened.Remove(folder);
                        }
                        await SettingsLoader.SaveSettingsAsync(currentSettings);
                    }
                }
            }
        }


        private async void btnAddFolders_Click(object sender, RoutedEventArgs e)
        {
            //Add folders to list
            var obser = new ObservableCollection<SongModel>();

            if (App.MainWindowInstance == null) return;
            var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Choose Folder", Windows.Storage.Pickers.PickerLocationId.MusicLibrary);
            if (folder != null)
            {
                // Detach old handlers if any, and attach a specific one for THIS button

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                Debug.WriteLine("folder add");
                var check = currentSettings.SavedFoldersOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                var savedfolders = currentSettings.SavedFoldersOpened;
                if (check == null)
                {
                    Debug.WriteLine("folder add NULL CHECK");

                    savedfolders.Add(new FoldersListOpened { FolderPath = folder.Path, isChecked = true, FolderName = Path.GetFileName(folder.Path) });
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    Debug.WriteLine("folder add NULL CHECK test");
                    var alreadyexist = foldersListOpened.FirstOrDefault(p => p.FolderPath == folder.Path);
                    if (alreadyexist != null) return;

                    foldersListOpened.Add(new FoldersListOpened { FolderPath = folder.Path, FolderName = Path.GetFileName(folder.Path), isChecked = true });

                }
            }
        }

        private async void cmbStartupPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            switch (cmbStartupPage.SelectedIndex)
            {
                case 0:
                    currentSettings.StartupPage = "Home";
                    break;

                case 1:
                    currentSettings.StartupPage = "Music";
                    break;

                case 2:
                    currentSettings.StartupPage = "Video";
                    break;
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private async void chkShowSplash_Checked(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if(chkShowSplash.IsChecked == false)
            {
                currentSettings.IsSplashEnabled = false;
            }
            else
            {
                currentSettings.IsSplashEnabled = true;

            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);

        }


        private async void btnAppLock_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Set App Lock", "Close", "", "", OceanDialogWindow.ContentType.AppLock, OceanContentDialogDefault.Primary, XamlRoot, 700, 800, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", ""
, new ObservableCollection<SongModel>(), "");
            MediaPlaybackController.Instance.PrimaryButtonEnable = false;
        }
    }
}

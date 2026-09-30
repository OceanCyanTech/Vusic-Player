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
            if (currentSettings.IsVideoHistoryDisabled)
            {
                chkSaveHistory.IsChecked = false;
            }
            else
            {
                chkSaveHistory.IsChecked = true;
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
            frmAboutOptions.Visibility = Visibility.Collapsed;
            grdMusicOptions.Visibility = Visibility.Collapsed;
            grdVideoOptions.Visibility = Visibility.Collapsed;
            lstHiddenVideos.Visibility = Visibility.Collapsed;
            hypBlockedVideos.Content = "View hidden videos";
            if (args.SelectedItemContainer == nvgitHomePage)
            {

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
    }
}

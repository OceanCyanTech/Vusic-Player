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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            InitializeComponent();
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
            else if(args.SelectedItemContainer == nvgitAboutHelp)
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
    }
}

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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

            if (args.SelectedItemContainer == nvgitHomePage)
            {

            }

            else if (args.SelectedItemContainer == nvgitMusicOptions)
            {
            }

            else if (args.SelectedItemContainer == nvgitVideoOptions)
            {
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
    }
}

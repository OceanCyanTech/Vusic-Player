using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages.Views.Onboarding;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AppLockVerify : Page
    {
        public AppLockVerify()
        {
            InitializeComponent();
            this.Loaded += AppLockVerify_Loaded;
        }

        private void AppLockVerify_Loaded(object sender, RoutedEventArgs e)
        {
            CheckAuthentication();
        }

        private async void CheckAuthentication()
        {
            if (PinService.IsWindowsAuthEnabled())
            {
                AppAuthVer.Visibility = Visibility.Collapsed;
                Debug.WriteLine("YES_WINDOWS AUTH");
                if (App.MainWindowInstance == null) return;
                IntPtr hwnd = WindowNative.GetWindowHandle(App.MainWindowInstance);

                // 2. Display the modal Windows security dialog
                bool verified = await AppLockAuth.VerifyCurrentUserAsync(
              hwnd,
              "Verify your identity to unlock Vusic Player.");

                if (verified)
                {
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();

                    if (currentSettings.IsSplashEnabled == false)
                    {
                        Frame.Navigate(typeof(OptimizeApp));
                    }
                    else
                    {
                        Frame.Navigate(typeof(SplashScreen));
                    }
                }
            }

        }

        private void Grid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {

        }

        private async void AppAuthVer_AuthenticationSuccess()
        {
            Debug.WriteLine("PARVARTI");
            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            if (currentSettings.IsSplashEnabled == false)
            {
                Frame.Navigate(typeof(OptimizeApp));
            }
            else
            {
                Frame.Navigate(typeof(SplashScreen));
            }

        }
    }
}

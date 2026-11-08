// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Pages.Views.Onboarding;
using Vusic_Player.UI.Experience.Pages;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Experience
{
    public sealed partial class FirstLaunchExperience : Window
    {
        private AppWindow _appWindow;
        private IntPtr _hwnd;

        public FirstLaunchExperience()
        {
            InitializeComponent();
            IntPtr hWnd = WindowNative.GetWindowHandle(this);

            // 2. Get the WindowId and AppWindow
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            // 3. Set presenter to Overlapped and Maximize
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.Maximize();
            }
            frmMain.Navigate(typeof(OceanCyanLogoIntro));
            this.ExtendsContentIntoTitleBar = true;
            this.Title = "Welcome to Vusic Player!";

            _hwnd = WindowNative.GetWindowHandle(this);
            string iconPath = Path.Combine(
                Package.Current.InstalledLocation.Path,
                "Assets",
                "appicon.ico"
            );
            _appWindow = AppWindow.GetFromWindowId(windowId);
            _appWindow.SetIcon(iconPath);
        }

        private record WizardStep(Type PageType, string Header);

        private readonly List<WizardStep> _steps = new()
        {
            new(typeof(WhatIsVusicPlayerPage), "Welcome to Vusic Player"),            // Index 0
            new(typeof(KeyFeaturesPage), "Vusic Player is magical!"),                 // Index 1
            new(typeof(ThemeSelection), "Select a Theme"),                            // Index 2
            new(typeof(FolderSelection), "Folder Permissions"),                       // Index 3
            new(typeof(DefaultFormats), "Default File Formats"),                      // Index 4 (Inserted before Desktop Shortcut)
            new(typeof(DesktopShortcut), "Create Desktop Shortcut"),                  // Index 5
            new(typeof(LicenseAgreement), "Open Source License"),                     // Index 6 (Requires Accept)
            new(typeof(PrivacyAgreement), "Your Privacy is OceanCyan's Priority"),    // Index 7
            new(typeof(AppLockDuringExperience), "App Lock")                          // Index 8 (Final Step)
        };

        private int _currentIndex = 0;

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                NavigateToStep(_currentIndex);
            }
        }

        private void btnGoForward_Click(object sender, RoutedEventArgs e)
        {
            if (_currentIndex < _steps.Count - 1)
            {
                _currentIndex++;
                NavigateToStep(_currentIndex);
            }
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            // Accepting the license advances directly to the next step (PrivacyAgreement)
            if (_currentIndex < _steps.Count - 1)
            {
                _currentIndex++;
                NavigateToStep(_currentIndex);
            }
        }

        private void NavigateToStep(int index)
        {
            var step = _steps[index];

            frmMain.Navigate(step.PageType);
            txtHeader.Text = step.Header;

            bool isLicenseStep = step.PageType == typeof(LicenseAgreement);
            bool isLastStep = index == _steps.Count - 1;

            // Back button: disabled only on the very first page
            btnGoBack.IsEnabled = index > 0;
            btnGoBack.Visibility = Visibility.Visible;

            // Forward button:
            // - Hidden on the final step
            // - Disabled on the License step (user must click btnAccept to proceed)
            // - Enabled on all other intermediary steps
            btnGoForward.Visibility = isLastStep ? Visibility.Collapsed : Visibility.Visible;
            btnGoForward.IsEnabled = !isLicenseStep && !isLastStep;

            // Accept button: Only visible on the License Agreement page
            btnAccept.Visibility = isLicenseStep ? Visibility.Visible : Visibility.Collapsed;
        }

        private void frmMain_Navigated(object sender, NavigationEventArgs e)
        {
            if (e.SourcePageType == typeof(WhatIsVusicPlayerPage))
            {
                btnGoBack.Visibility = Visibility.Visible;
                btnGoForward.Visibility = Visibility.Visible;
                _currentIndex = 0;
                btnGoBack.IsEnabled = false;
                rootgrd.Margin = new Thickness(40, 20, 0, 10);
                txtHeader.Text = "Welcome to Vusic Player";
            }
            else if (e.SourcePageType == typeof(OptimizeApp))
            {
                // Terminal configuration screen: disable and hide navigation controls
                btnGoBack.Visibility = Visibility.Collapsed;
                btnGoForward.Visibility = Visibility.Collapsed;
                btnAccept.Visibility = Visibility.Collapsed;
                txtHeader.Text = string.Empty;
            }
        }
    }
}
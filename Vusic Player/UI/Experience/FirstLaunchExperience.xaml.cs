// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

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
using Vusic_Player.UI.Experience.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Experience
{
    public sealed partial class FirstLaunchExperience : Window
    {
        int currentindex = 0;
        public FirstLaunchExperience()
        {
            InitializeComponent();
            frmMain.Navigate(typeof(OceanCyanLogoIntro));
            this.ExtendsContentIntoTitleBar = true;
            this.Title = "Welcome to Vusic Player!";
        }

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGoForward_Click(object sender, RoutedEventArgs e)
        {
            currentindex += 1;

            if(currentindex == 1)
            {
                frmMain.Navigate(typeof(WelcomePage));
            }
            else if (currentindex == 2)
            {
                frmMain.Navigate(typeof(WhatIsVusicPlayerPage));
                txtHeader.Text = "Welcome to Vusic Player";
            }
            else if (currentindex == 3)
            {
                frmMain.Navigate(typeof(KeyFeaturesPage));
                txtHeader.Text = "Vusic Player is magical!";
            }
            else if (currentindex == 4)
            {
                txtHeader.Text = "Select a Theme";

                frmMain.Navigate(typeof(ThemeSelection));
            }
            else if (currentindex == 5)
            {
                txtHeader.Text = "Folder Permissions";

                frmMain.Navigate(typeof(FolderSelection));
            }
            else if (currentindex == 6)
            {
                txtHeader.Text = "Create Desktop Shortcut";

                frmMain.Navigate(typeof(DesktopShortcut));
            }
            else if (currentindex == 7)
            {
                txtHeader.Text = "Open Source License";
                btnGoForward.IsEnabled = false;
                btnAccept.Visibility = Visibility.Visible;
                frmMain.Navigate(typeof(LicenseAgreement));
            }
       
        }

        private void btnAccept_Click(object sender, RoutedEventArgs e)
        {
            currentindex += 1;  
            txtHeader.Text = "Your Privacy is OceanCyan's Priority";

            frmMain.Navigate(typeof(PrivacyAgreement));
            btnGoForward.IsEnabled = true;
            btnAccept.Visibility = Visibility.Collapsed;
        }
    }
}

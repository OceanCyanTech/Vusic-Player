// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.7.0 (Build 131126) . All Rights Reserved.
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
using System.Xml;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Experience.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ThemeSelection : Page
    {
        public ThemeSelection()
        {
            InitializeComponent();
        }
        private void tgl_Unchecked(object sender, RoutedEventArgs e)
        {
            if (isChecking) return;

            if (sender is ToggleButton tgl)
            {
                tgl.IsChecked = true;
            }
        }
        private void tglOcean_Checked(object sender, RoutedEventArgs e)
        {
            if (isChecking) return;
            isChecking = true;

            tglNeon.IsChecked = false;
            tglSpace.IsChecked = false; SaveTheme();
            isChecking = false;
        }
        bool isChecking = false;
        private void tglNeon_Checked(object sender, RoutedEventArgs e)
        {
            if (isChecking) return;
            isChecking = true;
            tglOcean.IsChecked = false;
            tglSpace.IsChecked = false;
            SaveTheme(); isChecking = false;
        }
        private async void SaveTheme()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (tglSpace.IsChecked == true)
            {
                currentSettings.Theme = "Space";
                currentSettings.GlowColorHex = "#CECECE";
                currentSettings.FontFamily = "ms-appx:///Assets/Themes/Space/themefont.ttf#Rajdhani Medium";
            }
            else if (tglOcean.IsChecked == true)
            {
                currentSettings.Theme = "Ocean";
                currentSettings.GlowColorHex = "##00FFFF";
                currentSettings.FontFamily = "Segoe UI Variable Text";

            }
            else if (tglNeon.IsChecked == true)
            {
                currentSettings.Theme = "NeonGlow";
                currentSettings.FontFamily = "ms-appx:///Assets/Themes/NeonGlow/themefont.ttf#Beon";
                currentSettings.GlowColorHex = "#00FF2B";
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private void tglSpace_Checked(object sender, RoutedEventArgs e)
        {
            if (isChecking) return;
            isChecking = true;

            tglNeon.IsChecked = false;
            tglOcean.IsChecked = false;
            SaveTheme();
            isChecking = false;
        }
    }
}

// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 241126) . All Rights Reserved.
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
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;




namespace Vusic_Player.UI.Experience.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DefaultFormats : Page
    {
        public DefaultFormats()
        {
            InitializeComponent();
        }

        private async void btnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            string pfn = Package.Current.Id.FamilyName;

            // Replace "App" with the Id attribute defined in <Application Id="..." ...> in Package.appxmanifest
            string aumid = $"{pfn}!VusicPlayerOceanCyan";

            string uriString = $"ms-settings:defaultapps?registeredAUMID={Uri.EscapeDataString(aumid)}";
            await Launcher.LaunchUriAsync(new Uri(uriString));
        }
    }
}

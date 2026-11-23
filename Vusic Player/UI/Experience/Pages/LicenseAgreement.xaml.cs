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
using Windows.Foundation;
using Windows.Foundation.Collections;




namespace Vusic_Player.UI.Experience.Pages
{
    public sealed partial class LicenseAgreement : Page
    {
        public LicenseAgreement()
        {
            InitializeComponent();
            LoadLicense();
        }
        private async void LoadLicense()
        {
            var uri = new Uri("ms-appx:///Assets/GNULicense.license");
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            string textToSet = await Windows.Storage.FileIO.ReadTextAsync(file);

            rcheditboxLicense.IsReadOnly = false;
            rcheditboxLicense.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, textToSet);
            rcheditboxLicense.IsReadOnly = true;
        }
    }
}

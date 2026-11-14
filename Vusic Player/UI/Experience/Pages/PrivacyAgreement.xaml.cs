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
using Windows.Foundation;
using Windows.Foundation.Collections;


namespace Vusic_Player.UI.Experience.Pages
{
    public sealed partial class PrivacyAgreement : Page
    {
        public PrivacyAgreement()
        {
            InitializeComponent();
            LoadPrivacy();
        }
        private async void LoadPrivacy()
        {
            var uri = new Uri("ms-appx:///Assets/PrivacyStatement.txt");
            var file = await Windows.Storage.StorageFile.GetFileFromApplicationUriAsync(uri);
            string textToSet = await Windows.Storage.FileIO.ReadTextAsync(file);

            rcheditboxPrivacy.IsReadOnly = false;
            rcheditboxPrivacy.Document.SetText(Microsoft.UI.Text.TextSetOptions.None, textToSet);
            rcheditboxPrivacy.IsReadOnly = true;
        }
    }
}

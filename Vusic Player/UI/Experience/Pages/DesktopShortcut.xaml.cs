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
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Experience.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DesktopShortcut : Page
    {
        public DesktopShortcut()
        {
            InitializeComponent();
        }
        string ShortcutName => "Vusic Player";
        string DesktopDirectory => Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        string ShortcutPath => Path.Combine(DesktopDirectory, $"{ShortcutName}.lnk");
        private void CreateDesktopShortcut()
        {
            try
            {
                string packageFamily = Package.Current.Id.FamilyName;
                string appID = "VusicPlayerOceanCyan";
                string aumid = $"{packageFamily}!{appID}";

                string iconPath = Path.Combine(Package.Current.InstalledLocation.Path, "Assets", "appicon.ico");

                Type? shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;

                dynamic? shell = Activator.CreateInstance(shellType);
                if (shell == null) return;
                dynamic shortcut = shell.CreateShortcut(ShortcutPath);

                // Packaged apps are launched via explorer.exe shell:AppsFolder\[AUMID]
                shortcut.TargetPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "explorer.exe");
                shortcut.Arguments = $@"shell:AppsFolder\{aumid}";
                shortcut.Description = $"Launch {ShortcutName}";
                if (File.Exists(iconPath))
                {
                    shortcut.IconLocation = $"{iconPath},0";
                }

                shortcut.Save();
            }
            catch
            {

            }
        }

        private void chkDesktopShortcut_Checked(object sender, RoutedEventArgs e)
        {
            if(chkDesktopShortcut.IsChecked == true)
            {
                CreateDesktopShortcut();
            }
            else
            {
                if (File.Exists(ShortcutPath))
                {
                    File.Delete(ShortcutPath);
                }
            }
        }

        private void chkDesktopShortcut_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkDesktopShortcut.IsChecked == true)
            {
                CreateDesktopShortcut();
            }
            else
            {
                if (File.Exists(ShortcutPath))
                {
                    File.Delete(ShortcutPath);
                }
            }
        }
    }
}

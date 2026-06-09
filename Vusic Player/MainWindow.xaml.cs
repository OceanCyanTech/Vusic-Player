using FlyleafLib;
using FlyleafLib.MediaPlayer;
using Microsoft.UI.Input;
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
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;
using Windows.Foundation;
using Windows.Foundation.Collections;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private InputPreTranslateKeyboardSource? _keyboardSource;

        public MainWindow()
        {
            InitializeComponent();
            EngineService.StartEngine();
            frmMain.Navigate(typeof(MainPage));
            AppVersion.LoadBuildCounter();
            LoadVersion();
          
            //Player pl = new Player();
            //pl.Open(@"C:\Users\bnara\Downloads\Heartstopper Season 4\E08 Apart.mp4");
            //mainengine.Player = pl;
            this.ExtendsContentIntoTitleBar = true;
            rootGrid.Loaded += RootGrid_Loaded;
           
        }
        public async void ShowFileInfo(string filepath)
        {
           // FileInfo.LoadFileInfo(filepath, rootgrid.XamlRoot);
        }
        private void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            //var island = rootGrid.XamlRoot?.ContentIsland;

            //if (island != null)
            //{
            //    _keyboardSource = InputPreTranslateKeyboardSource.GetForIsland(island);
            //    _keyboardSource.PreTranslateMessage += OnKeyboardPreTranslateMessage;
            //}
        }

        public async void LoadVersion()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var counter = currentSettings.VersionCounter;
            if (counter.Count == 0)
            {
                counter.Add(0);
            }
            int counter2 = counter.FirstOrDefault() + 1;
            counter.Clear();
            counter.Add(counter2);
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            txtPreviewBuild.Text = $"Vusic Player Version {AppVersion.VersionString + Environment.NewLine} {AppVersion.VersionType} {DateTime.Now.ToString("MMddyy")}.{counter2}";

        }

    }
}

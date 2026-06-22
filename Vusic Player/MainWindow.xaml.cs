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
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using FileInfo = Vusic_Player.Configuration.Helper.FileInfo;
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
        private static MainWindow? instance;
        public static MainWindow? HideWindow()
        {
            if (instance != null)
            {
                instance.AppWindow.Hide();
            }
            return instance;
        }
        public static MainWindow ShowWindow()
        {
            if (instance == null)
            {
                instance = new MainWindow();

                //       instance.Closed += (_, __) => instance = null; // Reset when closed
                instance.Activate();
            }
            else
            {

                instance.Activate(); // Bring existing window to front
            }
            Debug.WriteLine("Launched again");
            App.MainWindowInstance = instance;
            //    instance.CheckForFileArguments();
            return instance;
        }
        public void CheckForFileArguments()
        {
            try
            {
                // Get how the current instance was activated 
                AppActivationArguments activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();

                if (activatedArgs != null)
                {
                    // Case 1: App was activated via a registered File Type Association
                    if (activatedArgs.Kind == ExtendedActivationKind.File)
                    {
                        if (activatedArgs.Data is FileActivatedEventArgs fileArgs)
                        {
                            foreach (var item in fileArgs.Files)
                            {
                                string filePath = item.Path;
                                //  ProcessOpenedFile(filePath);
                                string extension = System.IO.Path.GetExtension(filePath).ToLower();

                                string[] videoExtensions = Extensions.VideoExtensions.List;
                                string[] audioExtensions = Extensions.AudioExtensions.List;
                                if (videoExtensions.Contains(extension))
                                {

                                }
                                else if (audioExtensions.Contains(extension))
                                {
                                    //   MainWindow.ShowWindow();
                                    PlayerService.OpenPath(filePath);
                                    return;
                                }
                            }
                        }
                    }
                    // Case 2: App was activated via Command Line arguments / Standard launch
                    else if (activatedArgs.Kind == ExtendedActivationKind.Launch)
                    {
                        // Environment.GetCommandLineArgs() works beautifully across standard execution
                        string[] args = Environment.GetCommandLineArgs();

                        // Index 0 is always the application executable path, arguments start at Index 1
                        if (args.Length > 1)
                        {
                            for (int i = 1; i < args.Length; i++)
                            {
                                string filePath = args[i];

                                // Ensure it's an actual file path before processing
                                if (File.Exists(filePath))
                                {
                                    string extension = System.IO.Path.GetExtension(filePath).ToLower();

                                    string[] videoExtensions = Extensions.VideoExtensions.List;
                                    string[] audioExtensions = Extensions.AudioExtensions.List;
                                    if (videoExtensions.Contains(extension))
                                    {

                                    }
                                    else if (audioExtensions.Contains(extension))
                                    {
                                        //     MainWindow.ShowWindow();
                                        PlayerService.OpenPath(filePath);
                                        return;
                                    }
                                    //       ProcessOpenedFile(filePath);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to parse arguments: {ex.Message}");
            }
        }
        public async void ShowFileInfo(string filepath)
        {
            FileInfo.LoadFileInfo(filepath, rootGrid.XamlRoot);
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

            //      txtPreviewBuild.Text = $"Vusic Player Version {AppVersion.VersionString + Environment.NewLine} {AppVersion.VersionType} 150626";
            txtPreviewBuild.Text = $"Vusic Player Version {AppVersion.VersionString + Environment.NewLine} {AppVersion.VersionType} {DateTime.Now.ToString("MMddyy")}.{counter2}";

        }

    }
}

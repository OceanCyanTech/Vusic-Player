using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Pages;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player
{

    public partial class App : Application
    {
        private Window? _window;
        public static Window? MainWindowInstance;
        public static Window? OceanDialogInstance;
        public static Window? VideoDialogInstance;
        //public AlbumContext? SelectedAlbum { get; set; }
        public static Frame? NavigationFrame { get; set; }
        public static Frame? MasterFrame { get; set; }
        public static Frame? VideoPlayerFrame { get; set; }
        public AlbumContext? SelectedAlbum { get; set; }

        public App()
        {
            InitializeComponent();
            this.UnhandledException += (sender, e) =>
            {
                Debug.WriteLine("UNHANDLED EXCEPTION; "+ e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                string logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "winui_crash.txt");
                File.WriteAllText(logPath, args.ExceptionObject.ToString());
            };
        }


        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedArgs.Kind == ExtendedActivationKind.File)
            {
                var fileArgs = (FileActivatedEventArgs)activatedArgs.Data;
                var file = fileArgs.Files.FirstOrDefault();

                if (file != null)
                {
                    string filePath = file.Path;

                    string extension = System.IO.Path.GetExtension(filePath).ToLower();

                    string[] videoExtensions = Extensions.VideoExtensions.List;
                    string[] audioExtensions = Extensions.AudioExtensions.List;
                    if (videoExtensions.Contains(extension))
                    {
                        MainWindow.ShowWindow();
                        if (App.NavigationFrame != null)
                        {
                            if (PlayerService.InVideoPage == false)
                            {
                                App.NavigationFrame.Navigate(typeof(VideoPlayer), filePath);
                            }

                        }
                        return;
                    }
                    else if (audioExtensions.Contains(extension))
                    {
                        MainWindow.ShowWindow();
                        PlayerService.OpenPath(filePath);
                        return;
                    }
                }
            }
            MainWindow.ShowWindow();

        }
    }
}

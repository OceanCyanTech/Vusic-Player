using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI.Experience;
using Windows.ApplicationModel.Activation;
using AppInstance = Microsoft.Windows.AppLifecycle.AppInstance;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player
{

    public partial class App : Application
    {
        public static Window? MainWindowInstance;
        public static Window? OceanDialogInstance;
        public static Window? VideoDialogInstance;
        public static Window? SubtitleEditorDialogInstance;
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
                Debug.WriteLine("UNHANDLED EXCEPTION; " + e.Exception);
            };
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                string logPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "winui_crash.txt");
                File.WriteAllText(logPath, args.ExceptionObject.ToString());
            };
        }


        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            //     if (currentSettings.isFirstTimeLaunch == false)
            {
                var theme = currentSettings.Theme;
                ThemeManager.Instance.GlobalTheme = theme;
                DatabaseService.InitializeDatabase();
                MainWindow.ShowWindow();
                await FilesInDatabase.LoadAllFiles();
                await FilesInDatabase.InitializeDatabaseFunctionsAsync();
                _ = FilesInDatabase.RunBackgroundServiceAsync();
            }
            //else
            //{    EngineService.StartEngine();
            //    FirstLaunchExperience wind = new FirstLaunchExperience();
            //    wind.Activate();
            //}
         
        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;

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
        }

     
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            MainWindow.ShowWindow();
        }
    }
}

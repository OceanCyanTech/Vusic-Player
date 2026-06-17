using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.UI.Dialogs;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    public sealed partial class VideoLibrary : Page
    {
        public VideoLibrary()
        {
            InitializeComponent();
        }
        private void btnOpenVideo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            //OceanContentDialog.Show("Create New Video Playlist", "Create", "", "Cancel", OceanDialogWindow.ContentType.PlaylistCreation, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "Playlist", "", "", "", "", new PlaylistItem(), false, false);
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
        }
        private void OceanContentDialog_PrimaryRequested()
        {
         //   PlaylistCreation.CallPlaylistCreation();
            OceanContentDialog.HideDlg();
    //        MainWindow.ShowWindow();

        }
        private void hypViewEntireLib_Click(object sender, RoutedEventArgs e)
        {
        //    LibraryStore.IsMusicLibrary = false;
            this.Frame.Navigate(typeof(EntireMusicLibrary), "Videos");
        }

        private void Expander_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            if (!this.IsLoaded) return;

            if (sender != expRecents) expRecents.IsExpanded = false;

            if (sender != expRecommended) expRecommended.IsExpanded = false;

            if (sender != expVideoPlaylists) expVideoPlaylists.IsExpanded = false;

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Show Model", "Create", "", "Cancel", OceanDialogWindow.ContentType.ShowModel, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "", "", "", "", "", new PlaylistItem(), false, false);
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested1;
        }

        private void OceanContentDialog_PrimaryRequested1()
        {
            Debug.WriteLine("Yes create");
            PlaylistCreation.CallShowCreation();
            OceanContentDialog.HideDlg();
           MainWindow.ShowWindow();
        }
    }

}

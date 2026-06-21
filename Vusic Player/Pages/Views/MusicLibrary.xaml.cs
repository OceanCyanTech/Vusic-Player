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
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.UI.Navig;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{

    public sealed partial class MusicLibrary : Page
    {
        public MusicLibrary()
        {
            InitializeComponent();
            userplaylists.OpenPlaylistClick += Userplaylists_OpenPlaylistClick;
            userplaylists.GridViewItemClick += Userplaylists_GridViewItemClick;
            NavigationToPlaylist.NavigCalled += NavigationToPlaylist_NavigCalled;
        }

        private void Userplaylists_GridViewItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is PlaylistItem playlist)
            {
                if (_isopening) return;
                try
                {

                    _isopening = true;
                    this.Frame.Navigate(typeof(PlaylistView), playlist);

                }
                finally
                {
                    _isopening = false;

                }
            }
        }
        private bool _isopening = false;
        private void Userplaylists_OpenPlaylistClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem playlist)
            {
                if (_isopening) return;
                try
                {

                    _isopening = true;
                    this.Frame.Navigate(typeof(PlaylistView), playlist);

                }
                finally
                {
                    _isopening = false;
                }
            }
        }

        private void NavigationToPlaylist_NavigCalled()
        {
            this.Frame.Navigate(typeof(PlaylistView), NavigationToPlaylist.playlisttosend);
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;

            base.OnNavigatedFrom(e);
        }
        private void btnNewPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Playlist", "Create", "", "Cancel", OceanDialogWindow.ContentType.PlaylistCreation, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "Playlist");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
        }

        private void OceanContentDialog_PrimaryRequested()
        {
            PlaylistCreation.CallPlaylistCreation();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();

        }

        private void btnOpenMusic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void hypViewEntireLib_Click(object sender, RoutedEventArgs e)
        {
            LibraryStore.IsMusicLibrary = true;

            this.Frame.Navigate(typeof(EntireMusicLibrary));
        }

        private void expRecent_Expanding(Expander sender, ExpanderExpandingEventArgs args)
        {
            if (!this.IsLoaded) return;

            if (sender != expGenres) expGenres.IsExpanded = false;

            if (sender != expPlaylists) expPlaylists.IsExpanded = false;

            if (sender != expRecent) expRecent.IsExpanded = false;

            if (sender != expRecommend) expRecommend.IsExpanded = false;
        }

        private void TextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            expPlaylists.IsExpanded = true;
        }

        private void TextBlock_Tapped_1(object sender, TappedRoutedEventArgs e)
        {
            expRecommend.IsExpanded = true;
        }

        private void TextBlock_Tapped_2(object sender, TappedRoutedEventArgs e)
        {
            expGenres.IsExpanded = true;
        }
    }

}

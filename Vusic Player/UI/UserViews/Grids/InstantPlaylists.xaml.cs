using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class InstantPlaylists : UserControl
    {
        ObservableCollection<PlaylistItem> instantPlaylists = new ObservableCollection<PlaylistItem>();
        public InstantPlaylists()
        {
            InitializeComponent();
        }
        private async void CreateInstantPlaylists()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            //First playlist

            var mostplayedsongs = currentSettings.RecentMusic.OrderBy(p => p.PlayCount).Take(10);
            var tempobservablepaths = new HashSet<string>();
            foreach(var song in mostplayedsongs)
            {
                if (File.Exists(song.SongPath))
                {
                    tempobservablepaths.Add(song.SongPath);
                }
            }
            var playlist1 = new PlaylistItem { PlaylistName = "Instant Playlist 1", PlaylistId = Guid.NewGuid().ToString(), PlaylistGenre = "Most Played", DateCreation = DateTime.Now, SongsPaths = tempobservablepaths, Thumbnail = new Uri("ms-appx:///Assets/musicnoteicon.png") };

            instantPlaylists.Add(playlist1);
            var mostplayedartist = mostplayedsongs.FirstOrDefault(p=>p.)
            }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void chckSelectAll_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void grdViewPlaylists_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void mnftOpenPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftPlayAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftAddtoQueue_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftShufflePlay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftEditPlaylist_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

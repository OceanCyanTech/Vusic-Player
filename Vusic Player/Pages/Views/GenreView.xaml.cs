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
using Vusic_Player.Configuration.Helper.FileSystem;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GenreView : Page
    {
        public GenreView()
        {
            InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is GenreModel genreModel)
            {
                string genre = genreModel.GenreName;
                txtGenreTitle.Text = genre;
                var songs = FilesInDatabase.rawSongs;
                var genrebased = songs.Where(p => p.Genre == genre);
                lstViewMediaGenreSongs.ItemsSource = FoundSongs;

                var observablesongs = new ObservableCollection<SongModel>();
                foreach (var song in genrebased)
                {

                    FoundSongs.Add(new SongModel
                    {
                        Title = song.Title,
                        Artist = song.Artist,
                        AlbumName = song.AlbumName,
                        FilePath = song.FilePath,
                        SongDuration = song.SongDuration,
                        Genre = song.Genre,
                    });
                }
        //        FoundSongs = new ObservableCollection<SongModel>(observablesongs);
                txtNoSongs.Visibility = (FoundSongs.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
                lstViewMediaGenreSongs.Visibility = (FoundSongs.Count == 0) ? Visibility.Collapsed : Visibility.Visible;
                TotalDuration();
                txtSongCount.Text = $"• {FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";
            }
            base.OnNavigatedTo(e);
        }
        private void TotalDuration()
        {
            TimeSpan total = TimeSpan.FromTicks(
          FoundSongs.Sum(s => s.SongDuration?.Ticks ?? 0)
      );

            txtDuration.Text = total.TotalHours >= 1
                ? $"{(int)total.TotalHours}:{total.Minutes:D2}:{total.Seconds:D2}"
                : total.ToString(@"m\:ss");
            txtDuration.Text = "• " + txtDuration.Text;
        }
        ObservableCollection<SongModel> FoundSongs = new();
        private void btnRenameGenre_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnShuffle_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void btnAddSongs_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFindGenreProfileOnline_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnFindGenreProfileLocal_Click(object sender, RoutedEventArgs e)
        {

        }

        private void txtRename_GotFocus(object sender, RoutedEventArgs e)
        {

        }
    }
}

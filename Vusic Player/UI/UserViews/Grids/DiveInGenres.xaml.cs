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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Pages.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class DiveInGenres : UserControl
    {
        public DiveInGenres()
        {
            InitializeComponent();
            LoadGenres();
        }
        ObservableCollection<GenreModel> genresRecommended = new();
        private async void LoadGenres()
        {
            var rawsongs = FilesInDatabase.rawSongs;
            if (rawsongs == null || rawsongs.Count == 0) return;

            genresRecommended.Clear();
            Random rng = new Random();

            // 1. Group valid songs by genre (ignoring empty or unknown tags)
            var genreGroups = rawsongs
                .Where(s => !string.IsNullOrWhiteSpace(s.Genre)
                         && !s.Genre.Equals("Unknown Genre", StringComparison.OrdinalIgnoreCase)
                         && !s.Genre.Equals("Unknown", StringComparison.OrdinalIgnoreCase)
                         && File.Exists(s.FilePath))
                .GroupBy(s => s.Genre.Trim(), StringComparer.OrdinalIgnoreCase)
                // Shuffle the genres and pick up to 6 recommended genres
                .OrderBy(_ => rng.Next())
                .Take(6)
                .ToList();

            foreach (var group in genreGroups)
            {
                string genreName = group.Key;
                var tracksInGenre = group.ToList();

                // 2. Map the tracks to SongModel instances
                var songModels = tracksInGenre.Select(t => new SongModel
                {
                    Title = !string.IsNullOrWhiteSpace(t.Title) ? t.Title : Path.GetFileNameWithoutExtension(t.FilePath),
                    Artist = !string.IsNullOrWhiteSpace(t.Artist) ? t.Artist : "Unknown Artist",
                    AlbumName = !string.IsNullOrWhiteSpace(t.AlbumName) ? t.AlbumName : "Unknown Album",
                    FilePath = t.FilePath,
                    SongDuration = t.SongDuration // Or TimeSpan if stored on AudioTrackLite
                }).ToList();
                string coverPath = "ms-appx:///Assets/musicnoteicon.png";
                int trackCount = songModels.Count;

                genresRecommended.Add(new GenreModel
                {
                    GenreName = genreName,
                    GenreCover = coverPath,
                    GenreCount = $"{trackCount} {(trackCount == 1 ? "song" : "songs")}",
                    GenreTag = genreName.ToLowerInvariant(),
                    Songs = songModels
                });
            }
        }
        private void grdViewGenres_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is GenreModel genre)
            {
                if (App.NavigationFrame != null)
                {

                    App.NavigationFrame.Navigate(typeof(GenreView), genre);
                }
            }
        }

        private void mnftOpenGenre_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is GenreModel genre)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(GenreView), genre);
                }
            }
        }

        private async void mnftPlayGenre_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                Debug.WriteLine(genre.Songs.Count + " is hthe cout");
                foreach (var path in genre.Songs)
                {
                    Debug.WriteLine("SHD " + path);
                    var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, false, false);
            }

        }

        private async void mnftPlayGenreShuffled_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in genre.Songs)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, true, false);
            }
        }

        private async void mnftPlayGenreLoop_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in genre.Songs)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, false, true);
            }

        }

        private async void mnftPlayGenreLoopShuffle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is GenreModel genre)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in genre.Songs)
                {
                    var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, true, true);
            }
        }

    }
}

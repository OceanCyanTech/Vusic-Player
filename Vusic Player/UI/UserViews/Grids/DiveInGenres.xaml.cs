// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.7.0 (Build 131126) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Pages.Views;
using Windows.Storage;


namespace Vusic_Player.UI.UserViews.Grids
{
    /*  OCEANCYAN TECH HEADER
 VUSIC PLAYER - VERSION v1.0.7.0
© OCEANCYAN TECH 2026

USERCONTROL - DIVEINGENRES
BUILD 131126
 */
    public sealed partial class DiveInGenres : UserControl
    {
        #region Fields
        ObservableCollection<GenreModel> genresRecommended = new();
        #endregion

        #region MenuFlyoutContext
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
                foreach (var path in genre.Songs)
                {
                    if (File.Exists(path.FilePath))
                    {
                        Debug.WriteLine("SHD " + path);
                        var storagefile = await StorageFile.GetFileFromPathAsync(path.FilePath);
                        var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                        string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path.FilePath) : musicproperties.Title;
                        string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                        string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                        observabletemp.Add(new SongModel { FilePath = path.FilePath, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                    }
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
                    if (!File.Exists(path.FilePath)) continue;
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
                    if (!File.Exists(path.FilePath)) continue;

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
                    if (!File.Exists(path.FilePath)) continue;

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
        #endregion
        public DiveInGenres()
        {
            InitializeComponent();
            LoadGenres();
        }
        private async void LoadGenres()
        {
            stkNoGenres.Visibility = genresRecommended.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            grdViewGenres.Visibility = genresRecommended.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

            var rawsongs = FilesInDatabase.rawSongs;
            if (rawsongs == null || rawsongs.Count == 0) return;

            genresRecommended.Clear();
            Random rng = new Random();

            var genreGroups = rawsongs
                .Where(s => !string.IsNullOrWhiteSpace(s.Genre)
                         && !s.Genre.Equals("Unknown Genre", StringComparison.OrdinalIgnoreCase)
                         && !s.Genre.Equals("Unknown", StringComparison.OrdinalIgnoreCase)
                         && File.Exists(s.FilePath))
                .GroupBy(s => s.Genre.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderBy(_ => rng.Next())
                .Take(6)
                .ToList();

            foreach (var group in genreGroups)
            {
                string genreName = group.Key;
                var tracksInGenre = group.ToList();

                var songModels = tracksInGenre.Select(t => new SongModel
                {
                    Title = !string.IsNullOrWhiteSpace(t.Title) ? t.Title : Path.GetFileNameWithoutExtension(t.FilePath),
                    Artist = !string.IsNullOrWhiteSpace(t.Artist) ? t.Artist : "Unknown Artist",
                    AlbumName = !string.IsNullOrWhiteSpace(t.AlbumName) ? t.AlbumName : "Unknown Album",
                    FilePath = t.FilePath,
                    SongDuration = t.SongDuration 
                }).ToList();
                string coverPath = IconHolder.Instance.MusicNoteIcon;
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
            stkNoGenres.Visibility = genresRecommended.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            grdViewGenres.Visibility = genresRecommended.Count == 0 ? Visibility.Collapsed : Visibility.Visible;

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



    }
}

using CommunityToolkit.WinUI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Storage;

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
            CreateInstantPlaylists();
        }
        private async void CreateInstantPlaylists()
        {
            instantPlaylists.Clear();
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            //First playlist
            Random rng = new Random();


            var mostplayedsongs = currentSettings.RecentMusic
                .OrderByDescending(p => p.PlayCount)
                .Take(10)
                .OrderBy(_ => rng.Next())
                .ToList();
            var tempobservablepaths = new HashSet<string>();
            var tempmedia = new List<AudioTrackLite>();
            var rawsongs = FilesInDatabase.rawSongs;
            foreach (var song in mostplayedsongs)
            {
                var exist = rawsongs.FirstOrDefault(p => p.FilePath == song.SongPath);
                if (exist != null)
                {
                    tempmedia.Add(exist);
                }
                if (File.Exists(song.SongPath))
                {
                    tempobservablepaths.Add(song.SongPath);
                }
            }

            if (tempobservablepaths.Count != 0)
            {
                var playlist1 = new PlaylistItem { PlaylistName = "Listen Again", PlaylistId = Guid.NewGuid().ToString(), PlaylistGenre = "Most Played", DateCreation = DateTime.Now, SongsPaths = new HashSet<string>(tempobservablepaths), Thumbnail = new Uri(IconHolder.Instance.MusicNoteIcon) };
                instantPlaylists.Add(playlist1);
            }

            // --- PLAYLIST 2: Artist Spotlight ---
            var topArtist = tempmedia
                .Where(p => !string.IsNullOrWhiteSpace(p.Artist) && !p.Artist.Equals("Unknown Artist", StringComparison.OrdinalIgnoreCase))
                .GroupBy(p => p.Artist, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            if (!string.IsNullOrEmpty(topArtist))
            {
                var artistSongs = rawsongs
                    .Where(p => string.Equals(p.Artist, topArtist, StringComparison.OrdinalIgnoreCase) && File.Exists(p.FilePath))
                    .OrderBy(_ => rng.Next())
                    .Take(20)
                    .Select(p => p.FilePath)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (artistSongs.Count > 0)
                {
                    string[] titles = { $"{topArtist} Mix", "Artist Spotlight", "Your Favorite Artist", $"Best of {topArtist}" };
                    instantPlaylists.Add(new PlaylistItem
                    {
                        PlaylistName = titles[rng.Next(titles.Length)],
                        PlaylistId = Guid.NewGuid().ToString(),
                        PlaylistGenre = "Artist Mix",
                        DateCreation = DateTime.Now,
                        SongsPaths = artistSongs,
                        Thumbnail = new Uri(IconHolder.Instance.MusicNoteIcon)
                    });
                }
            }

            // --- PLAYLIST 3: Top Album Showcase ---
            var topAlbumGroup = tempmedia
                .Where(p => !string.IsNullOrWhiteSpace(p.AlbumName) && !p.AlbumName.Equals("Unknown Album", StringComparison.OrdinalIgnoreCase))
                .GroupBy(p => p.AlbumName, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault();

            if (topAlbumGroup != null)
            {
                string albumName = topAlbumGroup.Key;

                // Fetch tracks from that album (order by TrackNumber if available, or random)
                var albumSongs = rawsongs
                    .Where(p => string.Equals(p.AlbumName, albumName, StringComparison.OrdinalIgnoreCase) && File.Exists(p.FilePath))
                    .Select(p => p.FilePath)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (albumSongs.Count > 0)
                {
                    string[] albumTitles = { $"{albumName} in Full", "On Repeat: Album", "Featured Album", $"{albumName} Experience" };
                    instantPlaylists.Add(new PlaylistItem
                    {
                        PlaylistName = albumTitles[rng.Next(albumTitles.Length)],
                        PlaylistId = Guid.NewGuid().ToString(),
                        PlaylistGenre = "Album Mix",
                        DateCreation = DateTime.Now,
                        SongsPaths = albumSongs,
                        Thumbnail = new Uri(IconHolder.Instance.MusicNoteIcon)
                    });
                }
            }

            // --- PLAYLIST 4: Top Genre Mix ---
            var topGenre = tempmedia
                .Where(p => !string.IsNullOrWhiteSpace(p.Genre) && !p.Genre.Equals("Unknown Genre", StringComparison.OrdinalIgnoreCase))
                .GroupBy(p => p.Genre, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key;

            if (!string.IsNullOrEmpty(topGenre))
            {
                var genreSongs = rawsongs
                    .Where(p => string.Equals(p.Genre, topGenre, StringComparison.OrdinalIgnoreCase) && File.Exists(p.FilePath))
                    .OrderBy(_ => rng.Next())
                    .Take(25)
                    .Select(p => p.FilePath)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                if (genreSongs.Count > 0)
                {
                    string[] genreTitles = { $"{topGenre} Essentials", $"{topGenre} Vibes", $"Deep Dive: {topGenre}", $"{topGenre} Radio" };
                    instantPlaylists.Add(new PlaylistItem
                    {
                        PlaylistName = genreTitles[rng.Next(genreTitles.Length)],
                        PlaylistId = Guid.NewGuid().ToString(),
                        PlaylistGenre = topGenre,
                        DateCreation = DateTime.Now,
                        SongsPaths = genreSongs,
                        Thumbnail = new Uri(IconHolder.Instance.MusicNoteIcon)
                    });
                }
            }
            var playedPaths = new HashSet<string>(
    currentSettings.RecentMusic
        .Where(r => r.PlayCount > 0 && !string.IsNullOrEmpty(r.SongPath))
        .Select(r => r.SongPath),
    StringComparer.OrdinalIgnoreCase);

            // 2. Select tracks from rawsongs that have 0 plays / aren't in RecentMusic
            var unplayedTracks = rawsongs
                .Where(p => !string.IsNullOrEmpty(p.FilePath)
                         && !playedPaths.Contains(p.FilePath)
                         && File.Exists(p.FilePath))
                .OrderBy(_ => rng.Next())
                .Take(25)
                .Select(p => p.FilePath)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Only create the playlist if there are enough unplayed songs to make it worthwhile
            if (unplayedTracks.Count >= 5)
            {
                string[] unplayedTitles = { "Deep Cuts", "Never Played", "Hidden Gems", "Unheard Tracks", "Rediscover Your Library" };

                instantPlaylists.Add(new PlaylistItem
                {
                    PlaylistName = unplayedTitles[rng.Next(unplayedTitles.Length)],
                    PlaylistId = Guid.NewGuid().ToString(),
                    PlaylistGenre = "Discovery",
                    DateCreation = DateTime.Now,
                    SongsPaths = unplayedTracks,
                    Thumbnail = new Uri(IconHolder.Instance.MusicNoteIcon)
                });
            }
        }

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = chkSelect.IsChecked ?? false;

            grdViewPlaylists.SelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAll_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAll.IsChecked == true)
                grdViewPlaylists.SelectAll();
            else
                grdViewPlaylists.DeselectAll();
        }

        private void grdViewPlaylists_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (chkSelect.IsChecked == false)
            {
                if (e.ClickedItem is PlaylistItem playlist)
                {
                    if (App.NavigationFrame != null)
                        App.NavigationFrame.Navigate(typeof(PlaylistView), playlist);
                }
            }
        }

        private void mnftOpenPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem playlist)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(PlaylistView), playlist);
                }
            }
        }

        private async void mnftPlayAll_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is PlaylistItem playlist)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in playlist.SongsPaths)
                {
                    if (!File.Exists(path)) continue;

                    var storagefile = await StorageFile.GetFileFromPathAsync(path);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, false, false);
            }
        }

        private async void mnftShufflePlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem hyp && hyp.DataContext is PlaylistItem playlist)
            {
                var observabletemp = new ObservableCollection<SongModel>();
                foreach (var path in playlist.SongsPaths)
                {
                    if (!File.Exists(path)) continue;

                    var storagefile = await StorageFile.GetFileFromPathAsync(path);
                    var musicproperties = await storagefile.Properties.GetMusicPropertiesAsync();
                    string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(path) : musicproperties.Title;
                    string AlbumName = string.IsNullOrWhiteSpace(musicproperties.Album) ? "Unknown Album" : musicproperties.Album;
                    string Artist = string.IsNullOrWhiteSpace(musicproperties.Artist) ? "Unknown Artist" : musicproperties.Artist;

                    observabletemp.Add(new SongModel { FilePath = path, Title = title, AlbumName = AlbumName, Artist = Artist, SongDuration = musicproperties.Duration, Year = (int)musicproperties.Year });
                }
                QueueService.PlayMedia(observabletemp, true, false);
            }
        }


        private void mnftAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem playlist)
            {
                foreach (var item in playlist.SongsPaths)
                {
                    var exist = FilesInDatabase.rawSongs.FirstOrDefault(p => p.FilePath == item);
                    string Title = Path.GetFileNameWithoutExtension(item);
                    string Artist = "Unknown Artist";
                    string Album = "Unknown Album";
                    TimeSpan? Duration = TimeSpan.Zero;
                    if (exist != null)
                    {
                        Title = exist.Title;
                        Artist = exist.Artist;
                        Album = exist.AlbumName;
                        Duration = exist.SongDuration;
                    }
                    QueueService.VusicQueue.Add(new SongModel { Title = Title, FilePath = item, Artist = Artist, AlbumName = Album, SongDuration = Duration });
                    QueueService.VusicQueueNext.Add(new SongModel { Title = Title, FilePath = item, Artist = Artist, AlbumName = Album, SongDuration = Duration });
                }
            }

        }




        private void mnftDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem playlist)
            {
                if (App.MainWindowInstance == null) return;
                OceanContentDialog.Show("Confirm Delete", "Delete", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "deleteicon", "", "", new ObservableCollection<SongModel>(), "", $"Are you sure you want to delete the playlist '{playlist.PlaylistName}'? This cannot be undone.", "warning");
                //OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1;
                OceanContentDialog.PrimaryRequested += (async () =>
                {
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                    instantPlaylists.Remove(playlist);

                });
            }
        }



        PlaylistCreationValues Instance => PlaylistCreationValues.Instance;
        private async void mnftSavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is PlaylistItem playlist)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                currentSettings.SavedPlaylists.Add(playlist);
                Instance.PlaylistsMaster.Add(playlist);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                ttPlaylistSaved.IsOpen = true;
                await Task.Delay(2000);
                ttPlaylistSaved.IsOpen = false;
            }

        }

        private void btnRefreshPlaylists_Click(object sender, RoutedEventArgs e)
        {
            CreateInstantPlaylists();
        }
    }
}

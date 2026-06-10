using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace Vusic_Player.Pages.Views
{
    public sealed partial class PlaylistView : Page
    {
        ObservableCollection<SongModel> SongCollection = new();
        public PlaylistView()
        {
            InitializeComponent();
            SongCollection.CollectionChanged += SongCollection_CollectionChanged;
            lstViewUnified.ListViewRemoved += LstViewUnified_ListViewRemoved;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        private async void LstViewUnified_ListViewRemoved(object? sender, RoutedEventArgs e)
        {
            await Task.Delay(300);
            UpdateUI();
        }

        bool _isLoadingData = false;
        private async void SongCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isLoadingData) return;
            if (e.Action == NotifyCollectionChangedAction.Remove ||
          e.Action == NotifyCollectionChangedAction.Add ||
          e.Action == NotifyCollectionChangedAction.Move)
            {
                // UpdateUI();

                Debug.WriteLine("Moved");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var playl = currentSettings.SavedPlaylists;
                if (currentPlaylist != null)
                {
                    Debug.WriteLine("Check1");
                    var defaulexi = playl.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
                    if (defaulexi != null)
                    {
                        Debug.WriteLine("Check2");

                        defaulexi.SongsPaths.Clear();

                        foreach (var item in SongCollection.ToList())
                        {
                            Debug.WriteLine("Item: " + item.FilePath);
                            if (item.FilePath != null)
                            {
                                Debug.WriteLine("Check3");

                                defaulexi.SongsPaths.Add(item.FilePath);
                            }
                        }
                        int count = SongCollection.Count;
                        defaulexi.PlaylistCount = $"{count} {(count == 1 ? "item" : "items")}";
                    }
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);

            }
        }

        private void imgPlaylistCover_Tapped(object sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

        }


        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SongCollection)
            {
                item.IsCompleted = false;
            }
            QueueService.PlayMedia(SongCollection, btnShuffle.IsChecked ?? false, btnLoop.IsChecked ?? false);
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var playlists = currentSettings.SavedPlaylists;

            foreach (var playlist in playlists)
            {
                if (playlist.PlaylistName == txtPlaylistName.Text)
                {
                    playlist.PlaylistNowPlaying = "Now Playing...";
                }
                else
                {
                    // Reset all others to an empty string
                    playlist.PlaylistNowPlaying = "";
                }
            }

            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }


        private void btnLoop_Checked(object sender, RoutedEventArgs e)
        {
            QueueService.IsLoopTrue = btnLoop.IsChecked ?? false;
        }

        private void btnDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlaylist == null) return;
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Confirm Delete", "Confirm", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 600, 300, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"Are you sure you want the playlist '{currentPlaylist.PlaylistName}' to be deleted? This cannot be undone.", "warning");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
            OceanContentDialog.CloseRequested -= OceanContentDialog_CloseRequested;
            OceanContentDialog.CloseRequested += OceanContentDialog_CloseRequested;

        }

        private void OceanContentDialog_CloseRequested()
        {
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.CloseRequested -= OceanContentDialog_CloseRequested;

        }

        private async void OceanContentDialog_PrimaryRequested()
        {
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;

            if (currentPlaylist == null) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var playlists = currentSettings.SavedPlaylists;
            foreach (var items in playlists)
            {
                Debug.WriteLine(" IDS: " + items.PlaylistId);
            }
            var exist = playlists.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
            if (exist != null)
            {
                Debug.WriteLine(exist.PlaylistName + " is to be deleted");
                OceanContentDialog.HideDlg();
                MainWindow.ShowWindow();

                playlists.Remove(exist);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                grdRoot.Visibility = Visibility.Collapsed;
                grdDeletedPlaylist.Visibility = Visibility.Visible;
                txtPlaylistDeleted.Text = $"The playlist '{currentPlaylist.PlaylistName}' has been deleted.";
            }
        }

        private void btnEditPlaylistInfo_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            PlaylistCreation.playlistItem = currentPlaylist;
            OceanContentDialog.Show("Edit Playlist", "Save", "", "Cancel", OceanDialogWindow.ContentType.PlaylistEdit, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "saveicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "", "", "", "", "", currentPlaylist, true);


            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested1; ;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested1; ;
        }

        private void OceanContentDialog_PrimaryRequested1()
        {
            Debug.WriteLine("tEST2");
            currentPlaylist = PlaylistCreation.playlistItem;
            if (currentPlaylist == null) return;
            //Vusic_Player.Helper.FileInfo.RefreshValues -= FileInfo_RefreshValues;
            //Vusic_Player.Helper.FileInfo.RefreshValues += FileInfo_RefreshValues;
            txtPlaylistName.Text = currentPlaylist.PlaylistName;
            genreList.Clear();
            //playlistID = playlist.PlaylistId;
            if (currentPlaylist.PlaylistGenre != null)
            {
                txtGenreCov.Text = "Genre";
                var parts = currentPlaylist.PlaylistGenre.Split(',');
                foreach (var part in parts)
                {
                    var cleanedGenre = part.Trim();
                    if (!string.IsNullOrWhiteSpace(cleanedGenre))
                    {
                        var exist = genreList.FirstOrDefault(p => p.GenreTag == cleanedGenre);
                        if (exist == null)
                        {
                            genreList.Add(new GenreModel { GenreTag = cleanedGenre });
                        }
                    }
                }
            }
            else
            {
                txtGenreCov.Text = "";
            }
            grdViewGenres.ItemsSource = genreList;
            //           txtGenreCov.Text = "Genre: " + playlist.PlaylistGenre;
            if (currentPlaylist.PlaylistGenre == "") { txtGenreCov.Text = ""; }

            txtItemCount.Text = currentPlaylist.PlaylistCount;
            imgPlaylistCover.Source = new BitmapImage(currentPlaylist.Thumbnail);
            LoadItemsOnly(currentPlaylist);
            UpdateUI();
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();

        }

        private void dlgDeleteConfirmPlaylist_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
        private async void UpdateUI()
        {
            if (_isLoadingData) return;
            try
            {
                if (currentPlaylist == null) return;
                Debug.WriteLine("Yesssssss");
                _isLoadingData = true;
                bool hasSongs = SongCollection.Count > 0;
                var hasSongsVisibility = hasSongs ? Visibility.Visible : Visibility.Collapsed;
                var emptyVisibility = hasSongs ? Visibility.Collapsed : Visibility.Visible;

                panelEmptyplaylists.Visibility = emptyVisibility;
                txtPlaylistContentHeader.Visibility = hasSongsVisibility;
                if (currentPlaylist.isPlaylistVideo)
                {
                    lstViewUnified.VideoPlaylistUI();
                    lstViewUnified.VisiblityofViewButton = Visibility.Visible;

                }
                ListPanel.Visibility = hasSongsVisibility;
                int count = SongCollection.Count;
                txtItemCount.Text = $"• {count} {(count == 1 ? "item" : "items")}";
                TimeSpan timespan = new();
                timespan = TimeSpan.Zero;
                foreach (var item in SongCollection.ToList())
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                    MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();
                    timespan += properties.Duration;
                    Debug.WriteLine(properties.Duration + "  " + item.Title);
                }
                string formatted = timespan.TotalHours >= 1 ? timespan.ToString(@"h\:mm\:ss") : timespan.ToString(@"m\:ss");
                txtTotalDuration.Text = formatted;
            }
            finally
            {
                _isLoadingData = false;
            }

        }
        TimeSpan ts = new TimeSpan();
        private async void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            if (currentPlaylist == null) return;

            IReadOnlyList<StorageFile> files;
            try
            {
                files = await FilePickers.MediaPicker.PickMultipleMediaFilesAsync(App.MainWindowInstance, "Add media");
                if (files != null)
                {
                    var settings = await SettingsLoader.LoadSettingsAsync();
                    var favourites = settings.Favourites;
                    var favSet = new HashSet<FavouriteItems>(favourites);

                    var playlists = settings.SavedPlaylists;

                    foreach (var file in files)
                    {
                        var existing = SongCollection.FirstOrDefault(p => p.FilePath == file.Path);
                        if (existing == null)
                        {

                            MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();

                            string title = !string.IsNullOrWhiteSpace(properties.Title) ? properties.Title : file.DisplayName;
                            string album = !string.IsNullOrWhiteSpace(properties.Album) ? properties.Album : "Unknown Album";
                            string artist = !string.IsNullOrWhiteSpace(properties.Artist) ? properties.Artist : "Unknown Artist";

                            bool isfav = favSet.Any(f => f.FilePath == file.Path);
                            ts += properties.Duration;
                            var colorbrush = new SolidColorBrush(Microsoft.UI.Colors.White);
                            var glyph = "\uEC4F";

                            if (PlayerService.CurrentPlayingPath == file.Path)
                            {
                                colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
                                if (PlayerService.Masterplayer!.IsPlaying)
                                    glyph = "\uE769";
                                else
                                {
                                    glyph = "\uE768";
                                }
                            }
                            string fileExtension = file.FileType.ToLowerInvariant();
                            Visibility visibility = Visibility.Visible;
                            Visibility visibilityofvidtext = Visibility.Collapsed;
                            if (Extensions.VideoExtensions.List.Contains(fileExtension))
                            {
                                Debug.WriteLine("Yes is video");
                                glyph = "\uE8B2";
                                visibility = Visibility.Collapsed;
                                visibilityofvidtext = Visibility.Visible;
                            }
                            double opac = isfav ? 1.0 : 0.0;
                            string text = isfav ? "Remove from Favourites" : "Add to Favourites";
                            SongCollection.Add(new SongModel
                            {
                                Title = title,
                                AlbumName = album,
                                Artist = artist,
                                SongDuration = properties.Duration,
                                FilePath = file.Path,
                                FavOpacity = opac,
                                FavString = text,
                                VisibilityofAudioMeta = visibility,
                                VisibilityofVideoInfo = visibilityofvidtext,
                                IsFavourite = favSet.Any(f => f.FilePath == file.Path),
                                Glyph = glyph,
                                TitleColor = colorbrush,

                            });
                            lstViewUnified.ItemsSource = SongCollection;


                        }
                    }
                    await SettingsLoader.SaveSettingsAsync(settings);
                    UpdateUI();

                }

            }
            finally
            {
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
        bool isVidPlaylist = false;
        private async void LoadItemsOnly(PlaylistItem playlist)
        {
            if (_isLoadingData) return;
            try
            {
                _isLoadingData = true;
                SongCollection.Clear();
                var settings = await SettingsLoader.LoadSettingsAsync();
                var favourites = settings.Favourites;
                var favSet = new HashSet<FavouriteItems>(favourites);
                ts = TimeSpan.Zero;



                foreach (string path in playlist.SongsPaths)
                {
                    StorageFile file = await StorageFile.GetFileFromPathAsync(path);
                    MusicProperties properties = await file.Properties.GetMusicPropertiesAsync();

                    string title = !string.IsNullOrWhiteSpace(properties.Title) ? properties.Title : file.DisplayName;
                    string album = !string.IsNullOrWhiteSpace(properties.Album) ? properties.Album : "Unknown Album";
                    string artist = !string.IsNullOrWhiteSpace(properties.Artist) ? properties.Artist : "Unknown Artist";

                    bool isfav = favSet.Any(f => f.FilePath == path);
                    ts += properties.Duration;
                    var colorbrush = new SolidColorBrush(Microsoft.UI.Colors.White);

                    var glyph = "\uEC4F";

                    string fileExtension = file.FileType.ToLowerInvariant();
                    Visibility visibility = Visibility.Visible;
                    Visibility visibilityofvidtext = Visibility.Collapsed;
                    if (Extensions.VideoExtensions.List.Contains(fileExtension))
                    {

                        glyph = "\uE8B2";
                        visibility = Visibility.Collapsed;
                        visibilityofvidtext = Visibility.Visible;
                    }
                    if (PlayerService.CurrentPlayingPath == file.Path)
                    {
                        colorbrush = new SolidColorBrush(Microsoft.UI.Colors.Cyan);
                        if (PlayerService.Masterplayer!.IsPlaying)

                            glyph = "\uE769";
                        else
                        {
                            glyph = "\uE768";
                        }
                    }

                    double opac = isfav ? 1.0 : 0.0;
                    string text = isfav ? "Remove from Favourites" : "Add to Favourites";
                    SongCollection.Add(new SongModel
                    {
                        Title = title,
                        AlbumName = album,
                        Artist = artist,
                        SongDuration = properties.Duration,
                        FilePath = file.Path,
                        FavOpacity = opac,
                        FavString = text,
                        VisibilityofAudioMeta = visibility,
                        VisibilityofVideoInfo = visibilityofvidtext,
                        IsFavourite = favSet.Any(f => f.FilePath == file.Path),
                        Glyph = glyph,
                        TitleColor = colorbrush,
                    });
                    lstViewUnified.ItemsSource = SongCollection;




                }
                bool hasVideo = SongCollection.Any(song =>
          !string.IsNullOrEmpty(song.FilePath) &&
          Extensions.VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant())
      );
                if (hasVideo)
                {
                    lstViewUnified.VideoPlaylistUI();
                }
                int count = SongCollection.Count;
                txtItemCount.Text = $"• {count} {(count == 1 ? "item" : "items")}";
                string formatted = ts.TotalHours >= 1 ? ts.ToString(@"h\:mm\:ss") : ts.ToString(@"m\:ss");
                txtTotalDuration.Text = formatted;
                bool hasSongs = SongCollection.Count > 0;
                var hasSongsVisibility = hasSongs ? Visibility.Visible : Visibility.Collapsed;
                var emptyVisibility = hasSongs ? Visibility.Collapsed : Visibility.Visible;

                panelEmptyplaylists.Visibility = emptyVisibility;
                txtPlaylistContentHeader.Visibility = hasSongsVisibility;
                ListPanel.Visibility = hasSongsVisibility;
                //       UpdateUI();
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "PlaylistLoad", Logger.LogLevelType.Error);
            }
            finally
            {


                _isLoadingData = false;
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
  //          Vusic_Player.Helper.FileInfo.RefreshValues -= FileInfo_RefreshValues;
      //      lstViewUnified.ListViewRemoved -= LstViewUnified_ListViewRemoved;

            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            base.OnNavigatedFrom(e);
        }
        PlaylistItem? currentPlaylist;
        ObservableCollection<GenreModel> genreList = new();
        string playlistID = "";
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Debug.WriteLine("MultipleNavigatd");
            if (e.Parameter is PlaylistItem playlist)
            {
                currentPlaylist = playlist;
            //   FileInfo.RefreshValues -= FileInfo_RefreshValues;
              // FileInfo.RefreshValues += FileInfo_RefreshValues;
                txtPlaylistName.Text = playlist.PlaylistName;
                //playlistID = playlist.PlaylistId;
                genreList.Clear();
                if (playlist.PlaylistGenre != null)
                {
                    txtGenreCov.Text = "Genre";
                    var parts = playlist.PlaylistGenre.Split(',');
                    foreach (var part in parts)
                    {
                        var cleanedGenre = part.Trim();
                        if (!string.IsNullOrWhiteSpace(cleanedGenre))
                        {
                            var exist = genreList.FirstOrDefault(p => p.GenreTag == cleanedGenre);
                            if (exist == null)
                            {
                                genreList.Add(new GenreModel { GenreTag = cleanedGenre });
                            }
                        }
                    }
                }
                else
                {
                    txtGenreCov.Text = "";
                }
                grdViewGenres.ItemsSource = genreList;
                //           txtGenreCov.Text = "Genre: " + playlist.PlaylistGenre;
                if (playlist.PlaylistGenre == "") { txtGenreCov.Text = ""; }
                txtDateCreation.Text = playlist.DateCreation.ToString("dd MMMM yyyy");
                txtItemCount.Text = playlist.PlaylistCount;
                imgPlaylistCover.Source = new BitmapImage(playlist.Thumbnail);
                LoadItemsOnly(playlist);
                UpdateUI();
            }
            base.OnNavigatedTo(e);
        }

        private void FileInfo_RefreshValues()
        {
            if (currentPlaylist == null) return;
            LoadItemsOnly(currentPlaylist);

        }

        private void btnGenre_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnShuffle_Checked(object sender, RoutedEventArgs e)
        {
            if (btnShuffle.IsChecked == true)
            {
                QueueService.IsShuffleTrue = true;
                QueueService.ShuffleNext();
            }
            else
            {
                QueueService.IsShuffleTrue = false;
                QueueService.RestoreNext();
            }
        }

        private void btnGoToHome_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(HomeView));
            }
        }
    }

}

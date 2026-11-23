// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 161126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.FilePickers;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Storage;
using RoutedEventArgs = Microsoft.UI.Xaml.RoutedEventArgs;

namespace Vusic_Player.Pages.Views
{
    public class CountToItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return "• "+ (count == 1 ? $"{count} item" : $"{count} items");
            }

            return "• 0 items";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class PlaylistDurationString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is ObservableCollection<SongModel> media)
            {
                Debug.WriteLine("duration called ");
                TimeSpan ts = new TimeSpan();
                ts = TimeSpan.Zero;
                foreach (var item in media)
                {
                    Debug.WriteLine("duration for: " + item.FilePath);
                    Debug.WriteLine("duration: " + item.FormattedDuration);

                    ts += item.SongDuration ?? TimeSpan.Zero;
                }
                return ts.TotalHours >= 1
                            ? $"{(int)ts.TotalHours}:{ts:mm\\:ss}"
                            : ts.ToString(@"mm\:ss");
            }

            return "00:00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class MissingItemCount : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return count == 0 ? false : true;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class MissingItemCountString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is int count)
            {
                return $"{count} {(count == 1 ? "media path is" : "media paths are")} missing from your PC.";
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }


    public sealed partial class PlaylistView : Page
    {
      public  ObservableCollection<SongModel> SongCollection = new();
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
        private readonly PlaylistDurationString _durationConverter = new();

        public string FormatTotalDuration(int count)
        {
            return (string)_durationConverter.Convert(SongCollection, typeof(string), null!, string.Empty);
        }
        bool _isLoadingData = false;
        private async void SongCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_isLoadingData) return;
            if (e.Action == NotifyCollectionChangedAction.Remove ||
          e.Action == NotifyCollectionChangedAction.Add ||
          e.Action == NotifyCollectionChangedAction.Move)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var playl = currentSettings.SavedPlaylists;
                if (currentPlaylist != null)
                {
                    var defaulexi = playl.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
                    var existingmasterplaylist = Instance.PlaylistsMaster.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
                    if (existingmasterplaylist != null)
                    {
                        foreach (var item in SongCollection.ToList())
                        {
                            if (item.FilePath != null)
                            {
                                existingmasterplaylist.SongsPaths.Add(item.FilePath);
                            }
                        }
                    }
                    if (defaulexi != null)
                    {
                        defaulexi.SongsPaths.Clear();

                        foreach (var item in SongCollection.ToList())
                        {
                            if (item.FilePath != null)
                            {
                                defaulexi.SongsPaths.Add(item.FilePath);
                            }
                        }
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
            List<string> Paths = new List<string>();
            foreach (var item in SongCollection)
            {
                Paths.Add(item.FilePath);
            }
            QueueService.AddToQueueAndPlay(Paths, btnShuffle.IsChecked ?? false, btnLoop.IsChecked ?? false);
        }


        private void btnLoop_Checked(object sender, RoutedEventArgs e)
        {
            QueueObjects.QueueInstance.IsLoop = btnLoop.IsChecked ?? false;
        }

        private async void btnDeletePlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlaylist == null) return;
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Confirm Delete", "Confirm", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 600, 300, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"Are you sure you want the playlist '{currentPlaylist.PlaylistName}' to be deleted? This cannot be undone.", "warning");
            OceanContentDialog.PrimaryRequested += (async () =>
            {
                if (currentPlaylist == null) return;

                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var playlists = currentSettings.SavedPlaylists;

                var exist = playlists.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
                if (exist != null)
                {

                    playlists.Remove(exist);

                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    grdRoot.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
                    grdDeletedPlaylist.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    txtPlaylistDeleted.Text = $"The playlist '{currentPlaylist.PlaylistName}' has been deleted.";

                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                }
                var existingplaylistinmasterlist = MasterSearchIndex.PlaylistsMaster.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
                if (existingplaylistinmasterlist != null)
                {
                    MasterSearchIndex.PlaylistsMaster.Remove(existingplaylistinmasterlist);
                }
            });
        }



        private async void btnEditPlaylistInfo_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlaylist == null) return;
            Instance.MediaPaths.Clear();
            Instance.MediaSongModels.Clear();
            ObservableCollection<SongModel> songmodels = new ObservableCollection<SongModel>();
            foreach (var song in currentPlaylist.SongsPaths)
            {
                if (File.Exists(song))
                {
                    Instance.MediaSongModels.Add(await MetadataFetcher.GetSongModel(song));
                    Instance.MediaPaths.Add(song);
                }
            }
            ttEditPlaylist.IsOpen = true;
            Instance.PlCover = new BitmapImage(currentPlaylist.Thumbnail);
            Instance.Thumbnail = currentPlaylist.Thumbnail ?? new Uri(IconHolder.Instance.PlaylistIcon);

            Instance.PlaylistName = currentPlaylist.PlaylistName;
            Instance.Genre = currentPlaylist.PlaylistGenre ?? "";
            Instance.IsAppInstanceOceanDialog = false;

            btnSavePlaylistEdited.Click += (async (object sender, RoutedEventArgs e) =>
            {
                if (Instance.PlaylistName == "")
                {
                    Instance.PlaylistName = currentPlaylist.PlaylistName;
                }
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                string baseName = Instance.PlaylistName.Trim();

                string finalName = baseName;

                if (Instance.PlaylistName.Trim() != currentPlaylist.PlaylistName.Trim())
                {

                    if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                    int counter = 1;
                    while (currentSettings.SavedPlaylists.Any(p =>
                        string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalName = $"{baseName} ({counter++})";
                    }
                }
                currentPlaylist.PlaylistName = finalName;
                currentPlaylist.PlaylistGenre = Instance.Genre;
                currentPlaylist.plthumb = Instance.PlCover;
                currentPlaylist.Thumbnail = Instance.Thumbnail;
                currentPlaylist.ThumbnailString = Instance.ThumbnailString;
                currentPlaylist.SongsPaths = Instance.MediaPaths;
                var existingplaylist = currentSettings.SavedPlaylists.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
                if (existingplaylist != null)
                {
                    existingplaylist.PlaylistName = finalName;
                    genreList.Clear();
                    if (Instance.Genre != null)
                    {
                        txtGenreCov.Text = "Genre";
                        var parts = Instance.Genre.Split(',');
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
                        if (genreList.Count == 0)
                        {
                            txtGenreCov.Text = "";
                        }
                    }
                    existingplaylist.PlaylistGenre = Instance.Genre;
                    existingplaylist.plthumb = Instance.PlCover;
                    existingplaylist.Thumbnail = Instance.Thumbnail;
                    existingplaylist.ThumbnailString = Instance.ThumbnailString;
                    existingplaylist.SongsPaths = Instance.MediaPaths;
                    stkLoading.Visibility = Microsoft.UI.Xaml.Visibility.Visible;
                    LoadItemsOnly(existingplaylist);
                    UpdateUI();

                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    ttEditPlaylist.IsOpen = false;
                    Instance.IsAppInstanceOceanDialog = true;

                }
            });

        }


        private async void UpdateUI()
        {
            if (_isLoadingData) return;
            try
            {
                if (currentPlaylist == null) return;
                _isLoadingData = true;
                if (currentPlaylist.isPlaylistVideo)
                {
                    lstViewUnified.VideoPlaylistUI();
                    lstViewUnified.VisiblityofViewButton = Microsoft.UI.Xaml.Visibility.Visible;

                }
                stkLoading.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;
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
                    foreach (var file in files)
                    {
                        var existing = SongCollection.FirstOrDefault(p => p.FilePath == file.Path);
                        if (existing == null)
                        {
                            SongCollection.Add(await MetadataFetcher.GetSongModel(file.Path));
                        }
                    }
                    UpdateUI();
                }
            }
            finally
            {
            }
        }


        ObservableCollection<SongModel> MissingFilePaths = new ObservableCollection<SongModel>();
        private async void LoadItemsOnly(PlaylistItem playlist)
        {
            if (_isLoadingData) return;
            try
            {
                _isLoadingData = true;
                SongCollection.Clear();
                var settings = await SettingsLoader.LoadSettingsAsync();
                var favourites = settings.Favourites;
                bool hasHiddenFiles = false;
                foreach (string path in playlist.SongsPaths)
                {
                    try
                    {
                        if (File.Exists(path))
                        {
                            SongCollection.Add(await MetadataFetcher.GetSongModel(path));
                        }
                        else
                        {
                            SongCollection.Add(new SongModel { IsFileMissing = true, FilePath = path });
                            MissingFilePaths.Add(new SongModel { IsFileMissing = true, FilePath = path });
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                if (hasHiddenFiles)
                {
                    infoBarHiddenFiles.Severity = InfoBarSeverity.Informational;
                    infoBarHiddenFiles.Message = "Hidden files cannot be shown";
                    infoBarHiddenFiles.IsOpen = true;
                }
                else
                {
                    infoBarHiddenFiles.IsOpen = false;
                }
                bool hasVideo = SongCollection.Any(song =>
          !string.IsNullOrEmpty(song.FilePath) && File.Exists(song.FilePath) &&
          Extensions.VideoExtensions.List.Contains(Path.GetExtension(song.FilePath).ToLowerInvariant())
      );
                if (hasVideo)
                {
                    lstViewUnified.VideoPlaylistUI();
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "PlaylistLoad", Logger.LogLevelType.Error);
            }
            finally
            {
                stkLoading.Visibility = Microsoft.UI.Xaml.Visibility.Collapsed;

                _isLoadingData = false;
            }
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
        }
        PlaylistItem? currentPlaylist;
        ObservableCollection<GenreModel> genreList = new();
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is PlaylistItem playlist)
            {
                currentPlaylist = playlist;
                txtPlaylistName.Text = playlist.PlaylistName;
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
                imgPlaylistCover.Source = new BitmapImage(playlist.Thumbnail);
                LoadItemsOnly(playlist);
                //    UpdateUI();
            }
            base.OnNavigatedTo(e);
        }


        private void btnGenre_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is GenreModel genre)
            {
                if (App.NavigationFrame != null)
                {
                    App.NavigationFrame.Navigate(typeof(GenreView), genre);
                }
            }
        }

        private void btnShuffle_Checked(object sender, RoutedEventArgs e)
        {
            if (btnShuffle.IsChecked == true)
            {
                QueueService.ToggleShuffle(true);
                QueueService.ShuffleQueue();
            }
            else
            {
                QueueService.ToggleShuffle(false);
                QueueService.RestoreQueueItems();
            }
        }

        private void btnGoToHome_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(HomeView));
            }
        }

        private async void mnftChangeCover_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            if (currentPlaylist == null) return;
            var image = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.MainWindowInstance, "Choose Cover");
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var playlists = currentSettings.SavedPlaylists;
            var exist = playlists.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
            if (image != null)
            {
                imgPlaylistCover.Source = new BitmapImage(new Uri(image.Path));
                if (exist != null)
                {
                    exist.ThumbnailString = image.Path;
                    exist.Thumbnail = new Uri(image.Path);
                    exist.plthumb = new BitmapImage(new Uri(image.Path));
                }
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }

        private void btnRenamePlaylist_Click(object sender, RoutedEventArgs e)
        {
            ttRename.IsOpen = true;
            txtRenamePlaylist.Text = txtPlaylistName.Text;
        }
        PlaylistCreationValues Instance => PlaylistCreationValues.Instance;

        private async void btnOfficialRename_Click(object sender, RoutedEventArgs e)
        {
            if (currentPlaylist == null) return;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var shows = currentSettings.SavedPlaylists;
            var exist = shows.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
            var existinmaster = Instance.PlaylistsMaster.FirstOrDefault(p => p.PlaylistId == currentPlaylist.PlaylistId);
            if (exist != null)
            {
                string baseName = txtRenamePlaylist.Text.Trim();

                if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                string finalName = baseName;
                int counter = 1;
                while (currentSettings.Shows.Any(p =>
                    string.Equals(p.Name, finalName, StringComparison.OrdinalIgnoreCase)))
                {
                    finalName = $"{baseName} ({counter++})";
                }
                exist.PlaylistName = finalName;
            }
            if (existinmaster != null)
            {
                string baseName = txtRenamePlaylist.Text.Trim();

                if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                string finalName = baseName;
                int counter = 1;
                while (currentSettings.Shows.Any(p =>
                    string.Equals(p.Name, finalName, StringComparison.OrdinalIgnoreCase)))
                {
                    finalName = $"{baseName} ({counter++})";
                }
                existinmaster.PlaylistName = finalName;
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            txtPlaylistName.Text = txtRenamePlaylist.Text;
            ttRename.IsOpen = false;
        }

        private void btnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ttEditPlaylist.IsOpen = false;
        }

        private async void btnAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in SongCollection)
            {
                QueueService.AddToQueue(item.FilePath);
            }
            ttAddedtoQueue.IsOpen = true;
            await Task.Delay(2000);
            ttAddedtoQueue.IsOpen = false;
        }

        private async void btnRelocate_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is SongModel media)
            {
                var tempsongmodel = media;

                if (App.MainWindowInstance == null) return;
                var mediapicker = await MediaPicker.PickSingle(App.MainWindowInstance, "Relocate");
                if (mediapicker != null)
                {
                    var allmedia = SongCollection.Where(p => p.FilePath == media.FilePath);
                    foreach (var item in allmedia.ToList())
                    {
                        item.FilePath = mediapicker.Path;
                        item.IsFileMissing = false;
                        var index = SongCollection.IndexOf(item);
                        SongCollection.Remove(item);
                        var newsongmodel = await MetadataFetcher.GetSongModel(mediapicker.Path);
                       SongCollection.Insert(index, newsongmodel);
                    }
                }

                MissingFilePaths.Remove(tempsongmodel);
                if(MissingFilePaths.Count == 0)
                {
                    ttMissingFilesView.IsOpen = false;
                }
            }
        }

        private void btnViewMissingFiles_Click(object sender, RoutedEventArgs e)
        {
            ttMissingFilesView.IsOpen = true;
        }

        private void MissingFileRemovedInvoked(string filepath)
        {
            var existingfilepath = MissingFilePaths.Where(p => p.FilePath == filepath);
            foreach (var item in existingfilepath.ToList())
            {
                MissingFilePaths.Remove(item);
            }
            if (MissingFilePaths.Count == 0)
            {
                ttMissingFilesView.IsOpen = false;
            }
        }
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is SongModel media)
            {
                var allmedia = SongCollection.Where(p => p.FilePath == media.FilePath);
                foreach (var item in allmedia)
                {
                    SongCollection.Remove(item);
                }
                MissingFilePaths.Remove(media);
                if (MissingFilePaths.Count == 0)
                {
                    ttMissingFilesView.IsOpen = false;
                }
            }
        }
    }

}

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
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Pages.Views;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;



namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class RecentMusicList : UserControl
    {
        ObservableCollection<RecentMusicModel> recentMusics = new();

        public RecentMusicList()
        {
            InitializeComponent();
            LoadRecentMusic();
        }
        private async void LoadRecentMusic()
        {
            recentMusics.CollectionChanged -= RecentMusics_CollectionChanged;
            recentMusics.CollectionChanged += RecentMusics_CollectionChanged;
            recentMusics.Clear();
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if(currentSettings.IsMusicHistoryDisabled == true)
            {
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
                grdRecents.Visibility = Visibility.Collapsed;
                grdDisabledRecents.Visibility = Visibility.Visible;
                return;
            }
            var recentmusic = currentSettings.RecentMusic.Take(5);


            foreach (var item in recentmusic)
            {

                recentMusics.Add(new RecentMusicModel
                {
                    FolderName = new DirectoryInfo(
                            Path.GetDirectoryName(item.SongPath) ?? string.Empty
                        ).Name,
                    SongName = item.SongName,
                    SongPath = item.SongPath,
                    Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(item.SongPath),
                    PlayCountDisplay = $"{item.PlayCount} {(item.PlayCount == 1 ? "time" : "times")}",
                    LastPlayed = item.LastPlayed

                });

            }
            grdvRecents.ItemsSource = recentMusics;
            if (recentMusics.Count == 0)
            {
                grdvRecents.Visibility = Visibility.Collapsed;
                grdEmptySuggestions.Visibility = Visibility.Visible;
            }
            else
            {
                grdvRecents.Visibility = Visibility.Visible;
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
            }
            UpdateUI();
        }
        private void UpdateUI()
        {
            if (recentMusics.Count == 0)
            {
                grdRecents.Visibility = Visibility.Collapsed;
                grdEmptySuggestions.Visibility = Visibility.Visible;
            }
            else
            {
                grdRecents.Visibility = Visibility.Visible;
                grdEmptySuggestions.Visibility = Visibility.Collapsed;
            }
        }
        private void RecentMusics_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateUI();
        }

        private void mnftRemoveFromRecentMusic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftGoToFileLocation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftPlayRecents_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftAddtoQueueRecentMusic_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mnftViewFileInfo_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {

        }

        private void grdHighlightVideo_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void grdHighlightVideo_PointerExited(object sender, PointerRoutedEventArgs e)
        {

        }

 

        private void chkSelect_Checked(object sender, RoutedEventArgs e)
        {
            bool isChecked = chkSelect.IsChecked ?? false;

            grdvRecents.GridSelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chkSelect_Unchecked(object sender, RoutedEventArgs e)
        {
            bool isChecked = chkSelect.IsChecked ?? false;

            grdvRecents.GridSelectionMode = isChecked ? ListViewSelectionMode.Multiple : ListViewSelectionMode.Single;
            selectMoreOptions.Visibility = isChecked ? Visibility.Visible : Visibility.Collapsed;
        }

        private void chckSelectAllContinuePlaying_Checked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked ?? false)
                grdvRecents.SelectAll();
            else
                grdvRecents.ClearSelection();
        }

        private void btnRemoveFromContinueWatchingSelected_Click(object sender, RoutedEventArgs e)
        {
            selectMoreOptions.Visibility = Visibility.Collapsed;

            grdvRecents.RemoveSelection();

        }

        private void chckSelectAllContinuePlaying_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chckSelectAllContinuePlaying.IsChecked ?? false)
                grdvRecents.SelectAll();
            else
                grdvRecents.ClearSelection();
        }


        private void btnAllRecents_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(EntireMusicLibrary), "EntireHistory");
            }
        }
        private async Task<ObservableCollection<SongModel>> AddToSongModelObservable()
        {
            var observablecollection = new ObservableCollection<SongModel>();
            foreach (var item in recentMusics)
            {
                if (File.Exists(item.SongPath))
                {
                    var currentfile = await StorageFile.GetFileFromPathAsync(item.SongPath);
                    var musicprops = await currentfile.Properties.GetMusicPropertiesAsync();
                    observablecollection.Add(new SongModel { Title = item.SongName, FilePath = item.SongPath, AlbumName = musicprops.Album, Artist = musicprops.Artist, SongDuration = musicprops.Duration });
                }
            }
            return observablecollection;
        }
        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = await AddToSongModelObservable();
            QueueService.PlayMedia(observablecollection, false, false);
        }

        private async void btnDisableRecents_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsMusicHistoryDisabled = true;
            currentSettings.RecentMusic.Clear();
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            grdEmptySuggestions.Visibility = Visibility.Collapsed;
            grdRecents.Visibility = Visibility.Collapsed;
            grdDisabledRecents.Visibility = Visibility.Visible;
        }

        private async void btnEnableHistory_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.IsMusicHistoryDisabled = false;
            currentSettings.RecentMusic.Clear();
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            grdEmptySuggestions.Visibility = Visibility.Visible;
            grdRecents.Visibility = Visibility.Collapsed;
            grdDisabledRecents.Visibility = Visibility.Collapsed;
        }

        private async void mnftShufflePlay_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = await AddToSongModelObservable();
            QueueService.PlayMedia(observablecollection, true, false);
        }

        private async void mnftLoopPlay_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = await AddToSongModelObservable();
            QueueService.PlayMedia(observablecollection, false, true);
        }

        private async void mnftLoopShufflePlay_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = await AddToSongModelObservable();
            QueueService.PlayMedia(observablecollection, true, true);
        }
    }
}

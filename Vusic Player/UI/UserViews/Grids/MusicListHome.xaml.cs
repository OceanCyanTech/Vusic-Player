using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Extensions;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.Storage;




namespace Vusic_Player.UI.UserViews.Grids
{
    public sealed partial class MusicListHome : UserControl
    {
        public MusicListHome()
        {
            InitializeComponent();
            InitializeMusicSuggestions();
        }
        private async void InitializeMusicSuggestions()
        {
            // 1. Fetch library tracks from SQLite / Memory cache
            var allTracks = FilesInDatabase.rawSongs;

            if (allTracks == null || allTracks.Count == 0)
            {
                ShowEmptyMusicSuggestions();
                return;
            }

            Random rng = new Random();

            // 2. Filter valid, existing audio files (exclude missing paths or videos)
            var candidateAudios = allTracks
                .Where(t => !string.IsNullOrEmpty(t.FilePath)
                         && AudioExtensions.List.Contains(Path.GetExtension(t.FilePath), StringComparer.OrdinalIgnoreCase)
                         && File.Exists(t.FilePath))
                .OrderBy(_ => rng.Next())
                .Take(6) // Pick 6 recommended music tracks
                .ToList();

            if (candidateAudios.Count == 0)
            {
                ShowEmptyMusicSuggestions();
                return;
            }

            FinalList.Clear();

            // 3. Populate immediate placeholders to render the GridView instantly
            foreach (var track in candidateAudios)
            {
                string title = !string.IsNullOrWhiteSpace(track.Title)
                    ? track.Title
                    : Path.GetFileNameWithoutExtension(track.FilePath);

                var recommendItem = new FavouritesRecommend
                {
                    FilePath = track.FilePath,
                    FileName = title,
                    Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(track.FilePath),
                };

                FinalList.Add(recommendItem);

                // 4. Asynchronously extract embedded artwork without blocking UI responsiveness


                // 5. Update UI visibility
                if (FinalList.Count > 0)
                {
                    grdEmptySuggestions.Visibility = Visibility.Collapsed;
                    grdMusicMix.Visibility = Visibility.Visible;
                }
                else
                {
                    ShowEmptyMusicSuggestions();
                }
            }
        }
        private void mnftGoToFileLocation_Click(object sender, RoutedEventArgs e)
        {

            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                OpenFileLocation.PathSelect(selectedSong.FilePath);
            }
        }
        private async void mnftAddtoQueue_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                QueueService.AddToQueue(selectedSong.FilePath);
            }
        }
        private async void mnftPlay_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                if (File.Exists(selectedSong.FilePath))
                {
                    try
                    {
                        QueueService.AddToQueueAndPlay(selectedSong.FilePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error Occured: " + ex.Message, "RecentMusicGridView.PlayRecents", Logger.LogLevelType.Error);
                    }
                }
                else
                {
                    if (App.MainWindowInstance == null) return;
                    OceanContentDialog.Show("Missing File", "Relocate", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{selectedSong.FilePath}' does not exist.");

                    OceanContentDialog.PrimaryRequested += async () =>
                    {
                        if (App.OceanDialogInstance == null) return;


                        var file = await FilePickers.MediaPicker.PickSingleAudio(App.OceanDialogInstance, "Select File To Relocate");
                        if (file != null)
                        {
                            selectedSong.FilePath = file.Path;
                            selectedSong.Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(file.Path);
                            try
                            {
                                QueueService.AddToQueueAndPlay(selectedSong.FilePath);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Error Occured: " + ex.Message, "RecentMusicGridView.PlayRecents", Logger.LogLevelType.Error);
                            }

                            OceanContentDialog.HideDlg();
                            MainWindow.ShowWindow();
                        }
                    };
                }

            }

        }
        private async void mnftShufflePlay_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = await AddToSongModelObservable();
            QueueService.AddToQueueAndPlay(observablecollection, true, false);
        }

        private async void mnftLoopPlay_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = await AddToSongModelObservable();
            QueueService.AddToQueueAndPlay(observablecollection, false, true);
        }

        private async void mnftLoopShufflePlay_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = await AddToSongModelObservable();
            QueueService.AddToQueueAndPlay(observablecollection, true, true);
        }
        private async Task<List<string>> AddToSongModelObservable()
        {
            var observablecollection = new List<string>();
            foreach (var item in FinalList)
            {
                if (File.Exists(item.FilePath))
                {
                    observablecollection.Add(item.FilePath);
                }
            }
            return observablecollection;
        }

        private async void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            var observablecollection = await AddToSongModelObservable();
            QueueService.AddToQueueAndPlay(observablecollection, false, false);
        }
        private void mnftViewFileInfo_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem mnft && mnft.DataContext is FavouritesRecommend selectedSong)
            {
                if (App.MainWindowInstance is MainWindow wind)
                {
                    wind.ShowFileInfo(selectedSong.FilePath);
                }
            }
        }

        private void ShowEmptyMusicSuggestions()
        {
            FinalList.Clear();
            grdEmptySuggestions.Visibility = Visibility.Visible;
            grdMusicMix.Visibility = Visibility.Collapsed;
        }
        ObservableCollection<FavouritesRecommend> FinalList = new ObservableCollection<FavouritesRecommend>();
      





        private async void grdvFavourites_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (grdvFavourites.SelectionMode == ListViewSelectionMode.Single)
            {
                var selectedSong = e.ClickedItem as FavouritesRecommend;
                if (selectedSong == null) return;
                if (File.Exists(selectedSong.FilePath))
                {
                    try
                    {
                        QueueService.AddToQueueAndPlay(selectedSong.FilePath);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Error Occured: " + ex.Message, "RecentMusicGridView.PlayRecents", Logger.LogLevelType.Error);
                    }
                }
                else
                {
                    if (App.MainWindowInstance == null) return;
                    OceanContentDialog.Show("Missing File", "Relocate", "", "Cancel", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", $"'{selectedSong.FilePath}' does not exist.");

                    OceanContentDialog.PrimaryRequested += async () =>
                    {
                        if (App.OceanDialogInstance == null) return;

                        var file = await FilePickers.MediaPicker.PickSingleAudio(App.OceanDialogInstance, "Select File To Relocate");
                        if (file != null)
                        {

                            var musicproperties = await file.Properties.GetMusicPropertiesAsync();
                            string title = string.IsNullOrWhiteSpace(musicproperties.Title) ? Path.GetFileNameWithoutExtension(file.Path) : musicproperties.Title;
                            selectedSong.FilePath = file.Path;
                            selectedSong.FileName = title;
                            selectedSong.Thumbnail = await FileThumbnailObtain.GetFileThumbnailAsync(file.Path);
                            try
                            {
                                QueueService.AddToQueueAndPlay(selectedSong.FilePath);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log("Error Occured: " + ex.Message, "MusicListHome.Play", Logger.LogLevelType.Error);
                            }
                            OceanContentDialog.HideDlg();
                            MainWindow.ShowWindow();

                        }
                    };
                }
            }

        }
    }
}

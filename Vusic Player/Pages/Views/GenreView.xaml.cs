using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Internet;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
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
            FileSystemWatch.FileModified -= FileSystemWatch_FileModified;
            FileSystemWatch.FileModified += FileSystemWatch_FileModified;
        }
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _debouncers = new();

        private void FileSystemWatch_FileModified(string filePath, string arg2, string arg3, string arg4, TimeSpan arg5, string genre)
        {
            // If an update is already pending for this file, cancel it and restart the timer
            if (_debouncers.TryRemove(filePath, out var existingCts))
            {
                existingCts.Cancel();
                existingCts.Dispose();
            }

            var cts = new CancellationTokenSource();
            _debouncers[filePath] = cts;

            Task.Run(async () =>
            {
                try
                {
                    // Wait 300ms for Windows Explorer to finish writing and close the file
                    await Task.Delay(300, cts.Token);

                    if (cts.Token.IsCancellationRequested) return;

                    // Retry reading tags in case File Explorer still holds a brief lock
                    for (int attempt = 0; attempt < 5; attempt++)
                    {
                        try
                        {
                            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            var abstraction = new SimpleStreamAbstraction(filePath, stream);
                            using var tagFile = TagLib.File.Create(abstraction);

                            string title = string.IsNullOrWhiteSpace(tagFile.Tag.Title)
                                ? Path.GetFileNameWithoutExtension(filePath)
                                : tagFile.Tag.Title;
                            var tag = tagFile.Tag;
                            string artist = string.IsNullOrWhiteSpace(string.Join(", ", tag.AlbumArtists))
                                  ? (string.IsNullOrWhiteSpace(tag.FirstPerformer) ? "Unknown Artist" : tag.FirstPerformer)
                                  : string.Join(", ", tag.AlbumArtists);

                            string album = string.IsNullOrWhiteSpace(tagFile.Tag.Album)
                                ? "Unknown Album"
                                : tagFile.Tag.Album;
                            string genre = string.IsNullOrEmpty(string.Join(", ", tag.Genres)) ? "Unknown Genre" : string.Join(", ", tag.Genres);
                            TimeSpan duration = tagFile.Properties.Duration;

                            // Update SQLite Database
                            await DatabaseService.UpdateSongMetadataAsync(filePath, title, artist, album, genre);

                            // Safely dispatch to UI Thread once
                            DispatcherQueue.TryEnqueue(() =>
                            {
                                UpdateSongInCollections(filePath, album, artist, title, genre, duration);
                            });

                            break; // Success! Exit retry loop
                        }
                        catch (Exception ex) when (ex is IOException || ex is COMException)
                        {
                            // File is still locked by Explorer; wait 200ms and try again
                            await Task.Delay(200, cts.Token);
                        }
                    }
                }
                catch (TaskCanceledException)
                {
                    // Normal - superseded by a newer event for the same file
                }
                finally
                {
                    _debouncers.TryRemove(filePath, out _);
                    cts.Dispose();
                }
            });
        }
        private async void UpdateSongInCollections(string filePath, string newAlbum, string newArtist, string newTitle, string genre, TimeSpan duration)
        {
            string currentFilter = txtGenreTitle?.Text?.Trim() ?? "";
            var collections = new ObservableCollection<SongModel>[] { FoundSongs };
            bool foundInAny = false;

            bool matchesArtist = string.IsNullOrEmpty(currentFilter) ||
                                 genre.Contains(currentFilter, StringComparison.OrdinalIgnoreCase);

            foreach (var collection in collections)
            {
                if (collection == null) continue;

                var song = collection.FirstOrDefault(s => s.FilePath == filePath);
                if (song != null)
                {
                    foundInAny = true;
                    if (!matchesArtist)
                    {
                        collection.Remove(song);
                        Debug.WriteLine($"[Watcher] Removed (Artist changed/unmatched): {filePath}");
                    }
                    else
                    {
                        song.AlbumName = newAlbum;
                        song.Artist = newArtist;
                        song.Title = newTitle;
                        song.SongDuration = duration;
                        Debug.WriteLine($"[Watcher] Updated: {filePath}");
                    }
                }
            }

            if (!foundInAny && matchesArtist)
            {
                FoundSongs?.Add(new SongModel
                {
                    FilePath = filePath,
                    AlbumName = newAlbum,
                    Artist = newArtist,
                    Title = newTitle,
                    SongDuration = duration,
                    Genre = genre
                });
                Debug.WriteLine($"[Watcher] Added new track: {filePath}");
            }
            if (FoundSongs != null)
            {
                txtSongCount.Text = $"• {FoundSongs.Count} {(FoundSongs.Count == 1 ? "song" : "songs")}";
                TotalDuration();
            }
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
        private bool _isRenaming = false;

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
        private async void btnRenameGenre_Click(object sender, RoutedEventArgs e)
        {
            if (_isRenaming) return; // Prevent concurrent re-entry
            _isRenaming = true;
            btnRenameGenre.IsEnabled = false;
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var genres = currentSettings.GenresList;
            var existgenre = genres.FirstOrDefault(p => p.GenreName == txtGenreTitle.Text);
            string newGenreName = txtRename.Text?.Trim() ?? "";
            var existgenreinlist = genres.FirstOrDefault(p => p.GenreName == newGenreName);
            if (existgenreinlist != null)
            {


                if (string.IsNullOrEmpty(newGenreName))
                {
                    _isRenaming = false;
                    btnRenameGenre.IsEnabled = true;
                    return;
                }

                var filePathsToProcess = FoundSongs
                .Where(s => !string.IsNullOrEmpty(s.FilePath))
                .Select(s => s.FilePath)
                .ToList();
                try
                {
                    List<string> filepathstemp = new List<string>();
                    await Task.Run(() =>
                    {
                        foreach (var filePath in filePathsToProcess)
                        {
                            // This triggers FileSystemWatcher automatically when saved!
                            AudioMetadata.ChangeGenre(filePath, newGenreName);
                        }
                    });

                    if (existgenre != null)
                    {
                        existgenre.GenreName = newGenreName;
                        await SettingsLoader.SaveSettingsAsync(currentSettings);
                    }



                    txtGenreTitle.Text = newGenreName;
                    flyoutRename.Hide();
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message, "ArtistPage.RenameArtist", Logger.LogLevelType.Error);
                }
                finally
                {
                    // 7. Always restore state and resume watchers
                    _isRenaming = false;
                    btnRenameGenre.IsEnabled = true;
                }
            }
        }

        private void btnPlayAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in FoundSongs)
            {
                item.IsCompleted = false;
            }
            QueueService.PlayMedia(FoundSongs, btnShuffle.IsChecked ?? false, false);
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

        private async void btnAddSongs_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;

            var files = await FilePickers.MediaPicker.PickMultipleAudioFilesAsync(App.MainWindowInstance, "Choose files");
            if (files == null || !files.Any()) return;

            string targetGenre = txtGenreTitle?.Text?.Trim() ?? "";

            foreach (var song in files)
            {
                var filepath = song.Path;
                if (AudioMetadata.ChangeGenre(song.Path, targetGenre) == false)
                {
                    Debug.WriteLine("ERROR OCCURED IN ADDING SONGS TO GENRE: " + targetGenre);

                }

            }
        }

        private void btnFindGenreProfileOnline_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            CheckInternet.SetImage += () =>
            {
                LoadImage(CheckInternet.UrlToDownload);
            };
            OceanContentDialog.Show("Find Genre Cover Online", "Set", "", "Cancel", OceanDialogWindow.ContentType.OnlineArtistPicture, OceanContentDialogDefault.Primary, XamlRoot, 800, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", "", "", txtGenreTitle.Text);

            OceanContentDialog.PrimaryRequested += () =>
            {
                MainWindow.ShowWindow();
                OceanContentDialog.HideDlg();
            };
        }
        public async void LoadImage(string filePath)
        {
            try
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(filePath);
                using (var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read))
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(stream);
                    imgGenreCover.Source = bitmap;
                }
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var genres = currentSettings.GenresList;
                var existinggenre = genres.FirstOrDefault(a => a.GenreName == txtGenreTitle.Text);

                if (existinggenre != null)
                {
                    existinggenre.GenreCover = file.Path;
                }
                else
                {
                    var newgenre = new GenreModel
                    {
                        GenreName = txtGenreTitle.Text,
                        GenreCover = file.Path
                        // Add other default properties here
                    };
                    genres.Add(newgenre);
                }

                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, "GenreImageLoad", Logger.LogLevelType.Error);
            }
        }
        private async void btnFindGenreProfileLocal_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var file = await FilePickers.MediaPicker.PickSingleImageFileAsync(App.MainWindowInstance, "Choose Cover for Genre");

            if (file != null)
            {
                imgGenreCover.Source = new BitmapImage(new Uri(file.Path));
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var genres = currentSettings.GenresList;
                var existingGenre = genres.FirstOrDefault(a => a.GenreName == txtGenreTitle.Text);

                if (existingGenre != null)
                {
                    existingGenre.GenreCover = file.Path;
                }
                else
                {
                    var newGenre = new GenreModel
                    {
                        GenreName = txtGenreTitle.Text,
                        GenreCover = file.Path
                    };
                    genres.Add(newGenre);
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }
        }

        private void txtRename_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb) tb.SelectAll();
        }

        private async void mnftRemoveIcon_Click(object sender, RoutedEventArgs e)
        {
            imgGenreCover.Source = new BitmapImage(new Uri("ms-appx:///Assets/defaultgenre.png"));
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var genres = currentSettings.GenresList;
            var existingGenre = genres.FirstOrDefault(a => a.GenreName == txtGenreTitle.Text);

            if (existingGenre != null)
            {
                // 1. Update the existing entry
                existingGenre.GenreCover = "";
            }
            else
            {
                // 2. Create a new entry if it doesn't exist
                var newGenre = new GenreModel
                {
                    GenreName = txtGenreTitle.Text,
                    GenreCover = ""
                    // Add other default properties here
                };
                genres.Add(newGenre);
            }

            // Don't forget to save the changes back to storage!
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
    }
}

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.FilePickers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class AlbumEditor : UserControl
    {
        public AlbumEditor()
        {
            InitializeComponent();
        }
        public ObservableCollection<SongModel> FoundSongs { get; set; } = new ObservableCollection<SongModel>();
        HashSet<string> uniqueArtists = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        TimeSpan ts;
        private ObservableCollection<ArtistShow> ArtistShows { get; set; } = new ObservableCollection<ArtistShow>();
        private void TotalDuration()
        {
            TimeSpan total = TimeSpan.FromTicks(
                FoundSongs.Sum(s => s.SongDuration?.Ticks ?? 0)
            );

            txtDuration.Text = total.TotalHours >= 1
                ? $"{(int)total.TotalHours}:{total.Minutes:D2}:{total.Seconds:D2}"
                : total.ToString(@"m\:ss");
        }


        private async Task SearchFiles()
        {
            listviewItems.HideSort();

            ts = TimeSpan.Zero;
            string targetAlbum = txtAlbumName.Text;
            FoundSongs.Clear();
            uniqueArtists.Clear();

            ttProgress.IsOpen = true;
            prgProgress.IsIndeterminate = true; // Slide back and forth while we gather files
            prgProgress.Value = 0;

            string[] searchPaths = {
        UserDataPaths.GetDefault().Music,
        UserDataPaths.GetDefault().Downloads,
        UserDataPaths.GetDefault().Documents,
        UserDataPaths.GetDefault().Videos
    };

            List<StorageFile> allFoundFiles = new List<StorageFile>();

            foreach (var path in searchPaths)
            {
                try
                {
                    StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                    var queryOptions = new QueryOptions(CommonFileQuery.OrderByMusicProperties, new[] { ".mp3", ".flac", ".m4a" });
                    queryOptions.ApplicationSearchFilter = $"System.Music.AlbumTitle:=\"{targetAlbum}\"";

                    var query = folder.CreateFileQueryWithOptions(queryOptions);
                    var files = await query.GetFilesAsync();
                    allFoundFiles.AddRange(files);
                }
                catch { /* Handle access denied */ }
            }

            if (allFoundFiles.Count > 0)
            {
                prgProgress.IsIndeterminate = false;
                prgProgress.Maximum = allFoundFiles.Count;

                int processedCount = 0;

                foreach (var file in allFoundFiles)
                {
                    var tagFile = TagLib.File.Create(file.Path);
                    var tag = tagFile.Tag;

                    var props = await file.Properties.GetMusicPropertiesAsync();
                    ts += props.Duration;
                    string artistName = !string.IsNullOrWhiteSpace(props.AlbumArtist)
                        ? props.AlbumArtist : props.Artist;
                    string title = !string.IsNullOrWhiteSpace(props.Title) ? props.Title : Path.GetFileNameWithoutExtension(file.Path);
                    if (!string.IsNullOrEmpty(artistName))
                        uniqueArtists.Add(artistName);
                    var artistna = props.Artist;
                    if (artistna == "")
                    {
                        artistna = "Unknown Artist";
                    }
                    FoundSongs.Add(new SongModel
                    {
                        Title = title,
                        Artist = artistna,
                        AlbumName = props.Album,
                        SongDuration = props.Duration,
                        FilePath = file.Path
                    });
                    string albumName = string.IsNullOrWhiteSpace(tag.Album)
                         ? "Unknown Album"
                         : tag.Album;
                    string artists = (tag.AlbumArtists != null && tag.AlbumArtists.Length > 0)
    ? string.Join(", ", tag.AlbumArtists)
    : "Unknown Artist";

                    processedCount++;
                    prgProgress.Value = processedCount;
                }
            }
            listviewItems.ItemsSource = FoundSongs;
            string formatted = ts.TotalHours >= 1
         ? ts.ToString(@"h\:mm\:ss")
         : ts.ToString(@"m\:ss");
            txtDuration.Text = "• " + formatted;
            var sortedArtists = uniqueArtists.OrderBy(a => a);
            txtArtistsInvolved.Text = "• " + string.Join(", ", sortedArtists);
            txtCount.Text = "• " + $"{FoundSongs.Count} {(FoundSongs.Count == 1 ? "item" : "items")}";
            await Task.Delay(500);
            ttProgress.IsOpen = false;
        }
        string OriginalAlbumname = "";
        public async Task LoadAlbum(string AlbumName)
        {
            try
            {
                // 1. Update text UI safely
                txtAlbumName.Text = AlbumName;
                OriginalAlbumname = AlbumName;

                // 2. Fetch songs directly from SQLite (background thread)
                var albumSongs = await DatabaseService.GetSongsByAlbumAsync(AlbumName);

                // 3. Update UI collection safely on the UI Thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    FoundSongs.Clear();
                    foreach (var song in albumSongs)
                    {
                        FoundSongs.Add(song);
                    }

                    // Recalculate total duration
                    TotalDuration();
                });

                // 4. Load Album Cover / Settings (background read)
                Uri fallbackUri = new Uri("ms-appx:///Assets/defaultalbum.png");
                var currentSettings = await SettingsLoader.LoadSettingsAsync();

                var existingAlbum = currentSettings.AlbumsList?
                    .FirstOrDefault(a => a.Name.Equals(AlbumName, StringComparison.OrdinalIgnoreCase));

                string thumbnailPath = existingAlbum?.Thumbnail ?? "";

                // 5. BitmapImage MUST be created/assigned on the UI thread
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (!string.IsNullOrEmpty(thumbnailPath))
                    {
                        try
                        {
                            imgAlbumCover.Source = new BitmapImage(new Uri(thumbnailPath));
                        }
                        catch (Exception ex)
                        {
                            Logger.Log($"Failed to load thumbnail, reverting to default: {ex.Message}", "AlbumEditor", Logger.LogLevelType.Error);
                            imgAlbumCover.Source = new BitmapImage(fallbackUri);
                        }
                    }
                    else
                    {
                        imgAlbumCover.Source = new BitmapImage(fallbackUri);
                    }
                });
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading album '{AlbumName}': {ex.Message}", "AlbumEditor", Logger.LogLevelType.Error);
            }
        }
        private async void btnChangeAlbumCover_Click(object sender, RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null) return;
            var file = await MediaPicker.PickSingleImageFileAsync(App.OceanDialogInstance, "Choose Album Cover");
            if (file != null)
            {
                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                var artists = currentSettings.AlbumsList;
                var existingArtist = artists.FirstOrDefault(a => a.Name == txtAlbumName.Text);

                if (existingArtist != null)
                {
                    existingArtist.Thumbnail = file.Path;
                }
                else
                {
                    var newArtist = new AlbumModel
                    {
                        Name = txtAlbumName.Text,
                        Thumbnail = file.Path
                    };
                    artists.Add(newArtist);
                }
                await SettingsLoader.SaveSettingsAsync(currentSettings);
                imgAlbumCover.Source = new BitmapImage(new Uri(file.Path));
            }

        }
        bool _isRenaming = false;
        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            RenameAlbum(txtAlbumName.Text);
        }
        string thumbnailprev = "";
        private async void RenameAlbum(string newName, bool isRenamedUndo = false)
        {
            if (newName == OriginalAlbumname.ToLower()) return;
            if (_isRenaming) return; // Prevent double clicks
            _isRenaming = true;

            // Disable the button or show a loading state if needed
            btnRename.IsEnabled = false;

            try
            {
                Debug.WriteLine("Rename initiated");
                string newAlbumName = newName;
                var songsToProcess = FoundSongs.ToList();
                foreach (SongModel item in FoundSongs.ToList())
                {
                    try
                    {
                        if (item.FilePath != null)
                        {
                            var filelocked = GetLockingProcess.GetLockingProcesses(item.FilePath);
                            if (filelocked.Count == 0)
                            {
                                var file = TagLib.File.Create(item.FilePath);
                                file.Tag.Album = newAlbumName;
                                file.Save();
                                file.Dispose(); // Important: Release TagLib handle
                                item.AlbumName = newAlbumName;
                                var currentSettings = await SettingsLoader.LoadSettingsAsync();
                                var albumslist = currentSettings.AlbumsList;
                                var existing = albumslist.FirstOrDefault(p => p.Name == OriginalAlbumname);
                                if (existing != null)
                                {
                                    if (newAlbumName == "")
                                    {
                                        if (existing.Thumbnail != null && existing.Thumbnail != "")
                                        {
                                            thumbnailprev = existing.Thumbnail;
                                        }
                                        albumslist.Remove(existing);
                                    }
                                    else
                                    {
                                        existing.Name = newAlbumName;
                                    }
                                }
                                else
                                {
                                    if (newAlbumName != "")
                                    {
                                        albumslist.Add(new AlbumModel { Name = newAlbumName, Thumbnail = thumbnailprev });
                                    }
                                }
                                await SettingsLoader.SaveSettingsAsync(currentSettings);
                            }
                            else
                            {
                                var processNames = string.Join(", ", filelocked.Select(p => p.ProcessName));
                                if (App.MainWindowInstance == null) return;


                                InfoBar.Visibility = Visibility.Visible;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message, "ArtistPage.RenameArtist", Logger.LogLevelType.Error);
                    }
                }

            }
            finally
            {
                if (newName != "")
                {
                    _isRenaming = false;
                    btnRename.IsEnabled = true;


                    OriginalAlbumname = newName;
                    txtAlbumName.Text = newName;
                    if (isRenamedUndo == false)
                    {
                        ttRename.Title = $"Renamed album from {OriginalAlbumname} to {newName}";
                        ttRename.IsOpen = true;

                        await Task.Delay(2000);
                        ttRename.IsOpen = false;
                    }
                }
                else
                {
                    _isRenaming = false;

                    rootGrid.Visibility = Visibility.Collapsed;
                    btnBack.Visibility = Visibility.Collapsed;
                    btnSetImage.Visibility = Visibility.Collapsed;
                    txtAlbumName.Text = "";
                    grdRemovedAlbumInfo.Visibility = Visibility.Visible;
                }
                prgLoad.Visibility = Visibility.Collapsed;

            }
        }
        private void HoverOverlay_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateOpacity(HoverOverlay, 0.7);
            AnimateOpacity(btnRemoveImage, 1);
        }

        private void HoverOverlay_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            AnimateOpacity(HoverOverlay, 0);
            AnimateOpacity(btnRemoveImage, 0);

        }
        private void AnimateOpacity(UIElement target, double toOpacity)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation animation = new DoubleAnimation
            {
                To = toOpacity,
                Duration = new Duration(TimeSpan.FromMilliseconds(200)), // 0.2 seconds
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, "Opacity");

            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

        private void btnRemoveImage_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            AnimateOpacity(HoverOverlay, 0.7);
            AnimateOpacity(btnRemoveImage, 1);
        }

        private void btnRemoveImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            AnimateOpacity(HoverOverlay, 0);
            AnimateOpacity(btnRemoveImage, 0);
        }

        private async void btnRemoveImage_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var artists = currentSettings.AlbumsList;
            var existingArtist = artists.FirstOrDefault(a => a.Name == txtAlbumName.Text);

            if (existingArtist != null)
            {
                artists.Remove(existingArtist);
                await SettingsLoader.SaveSettingsAsync(currentSettings);
            }

            imgAlbumCover.Source = new BitmapImage(new Uri("ms-appx:///Assets/defaultalbum.png"));
        }

        private void btnFindOnline_Click(object sender, RoutedEventArgs e)
        {
            rootGrid.Visibility = Visibility.Collapsed;
            SearchOnline.Visibility = Visibility.Visible;
            SearchOnline.UpdateArtistName(txtAlbumName.Text);
            btnBack.Visibility = Visibility.Visible;
            btnSetImage.Visibility = Visibility.Visible;
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            rootGrid.Visibility = Visibility.Visible;
            SearchOnline.Visibility = Visibility.Collapsed;
            btnBack.Visibility = Visibility.Collapsed;
            btnSetImage.Visibility = Visibility.Collapsed;
        }

        private void btnSetImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InfoBar.Visibility = Visibility.Collapsed;
        }

        private void btnRemoveAlbum_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnConfirmRemoveAlbum_Click(object sender, RoutedEventArgs e)
        {
            prgLoad.Visibility = Visibility.Visible;
            flyoutConfirm.Hide();
            RenameAlbum("");
        }

        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await SearchFiles();
        }

        private async void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            rootGrid.Visibility = Visibility.Visible;

            grdRemovedAlbumInfo.Visibility = Visibility.Collapsed;
            Debug.WriteLine(OriginalAlbumname);
            RenameAlbum(OriginalAlbumname, true);

        }
    }
}

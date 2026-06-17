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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.Converters;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.FilePickers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class NewPlaylistCreation : UserControl
    {
        public string PlaylistNameSuggested
        {
            get => (string)GetValue(PlaylistNameSuggestedProperty);
            set => SetValue(PlaylistNameSuggestedProperty, value);
        }

        public static readonly DependencyProperty PlaylistNameSuggestedProperty =
            DependencyProperty.Register(nameof(PlaylistNameSuggested), typeof(string), typeof(NewPlaylistCreation), new PropertyMetadata(string.Empty));
        //
        public static readonly DependencyProperty GenreVisibility = DependencyProperty.Register("Value", typeof(Visibility), typeof(NewPlaylistCreation), new PropertyMetadata(Visibility.Visible));
        public Visibility VisibilityOfGenreProperty
        {
            get => (Visibility)GetValue(GenreVisibility);
            set => SetValue(GenreVisibility, value);
        }
        public static readonly DependencyProperty SearchPlaceholder =
            DependencyProperty.Register("Value", typeof(string), typeof(NewPlaylistCreation), new PropertyMetadata("Find songs that you added..."));

        public string SearchPlaceHolderText
        {
            get => (string)GetValue(SearchPlaceholder);
            set => SetValue(SearchPlaceholder, value);
        }
        public static readonly DependencyProperty HeaderAdded =
           DependencyProperty.Register("Value", typeof(string), typeof(NewPlaylistCreation), new PropertyMetadata("Added Songs"));

        public string AddedMediaHeader
        {
            get => (string)GetValue(HeaderAdded);
            set => SetValue(HeaderAdded, value);
        }
        public static readonly DependencyProperty EmptyList =
           DependencyProperty.Register("Value", typeof(string), typeof(NewPlaylistCreation), new PropertyMetadata("No songs have been added yet!"));

        public string EmptyListDisplay
        {
            get => (string)GetValue(EmptyList);
            set => SetValue(EmptyList, value);
        }
        public static readonly DependencyProperty ButtonAddedSongs =
          DependencyProperty.Register("Value", typeof(string), typeof(NewPlaylistCreation), new PropertyMetadata("Add Songs"));
        public static readonly DependencyProperty IsVideoPlaylistProperty =
        DependencyProperty.Register(
            nameof(IsVideoPlaylist),
            typeof(bool),
            typeof(NewPlaylistCreation),
            new PropertyMetadata(false)); // Default value set to false

        public bool IsVideoPlaylist
        {
            get => (bool)GetValue(IsVideoPlaylistProperty);
            set => SetValue(IsVideoPlaylistProperty, value);
        }
        public string AddedSongsButton
        {
            get => (string)GetValue(ButtonAddedSongs);
            set => SetValue(ButtonAddedSongs, value);
        }

        public Visibility CheckIfListIsEmpty(object items)
        {
            if (items is System.Collections.IEnumerable list)
            {
                var enumerator = list.GetEnumerator();
                if (enumerator.MoveNext())
                {
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public ObservableCollection<SongModel> AllSongs { get; set; } = new();
        string playlistcoverpath = "";

        public NewPlaylistCreation()
        {
            InitializeComponent();
            PlaylistCreation.CreationCall -= PlaylistCreation_CreationCall;
            PlaylistCreation.CreationCall += PlaylistCreation_CreationCall;
            PlaylistCreation.ExistingItems -= PlaylistCreation_ExistingItems1;
            PlaylistCreation.ExistingItems += PlaylistCreation_ExistingItems1;
            this.Unloaded += NewPlaylistCreation_Unloaded;
        }

        private void PlaylistCreation_ExistingItems1()
        {
            AllSongs.Clear();
            foreach (var item in PlaylistCreation.existingitems)
            {
                AllSongs.Add(item);
            }
            
            lstViewPlaylistAddedSongs.StartBringIntoView();
            lstViewPlaylistAddedSongs.ItemsSource = AllSongs;
            UpdateUI();
        }

        private void NewPlaylistCreation_Unloaded(object sender, RoutedEventArgs e)
        {
            PlaylistCreation.CreationCall -= PlaylistCreation_CreationCall;
        }

        public void ClearStuff()
        {
            Uri defaultPath = new Uri("ms-appx:///Assets/playlistdefaultdark.png");
            imgPlaylistCov.Source = new BitmapImage(defaultPath);
            txtEditGenre.Text = "";
            AllSongs.Clear();
            UpdateUI();
        }
        private void PlaylistCreation_ExistingItems()
        {
            Debug.WriteLine("CALLED");
            AllSongs.Clear();
            foreach (var item in PlaylistCreation.existingitems)
            {
                AllSongs.Add(item);
            }
            txtEditPlaylistName.Text = PlaylistCreation.suggestedplaylistname;
            lstViewPlaylistAddedSongs.StartBringIntoView();
            lstViewPlaylistAddedSongs.ItemsSource = AllSongs;
            UpdateUI();
        }

        private void PlaylistCreation_CreationCall()
        {
            string playlistID = Guid.NewGuid().ToString("N");
            Uri defaultPath = new Uri("ms-appx:///Assets/playlistdefaultdark.png");


            if (playlistcoverpath != "")
            {
                defaultPath = new Uri(playlistcoverpath);
            }
            else
            {
                Uri darkIcon = new Uri("ms-appx:///Assets/playlistdefaultdark.png");

                // Set your initial default (e.g., based on current theme)
                defaultPath = darkIcon;
            }

            var newPlaylistItem = new Configuration.ClassModels.PlaylistItem
            {
                PlaylistName = txtEditPlaylistName.Text,
                PlaylistGenre = txtEditGenre.Text,

                PlaylistId = playlistID,
                PlaylistCount = $"{AllSongs.Count} {(AllSongs.Count == 1 ? "item" : "items")}",
                PlaylistNowPlaying = "",
                SongsPaths = AllSongs
            .Select(s => s.FilePath)
            .Where(path => path != null)
            .ToHashSet()!,
                Thumbnail = defaultPath,
                DateCreation = DateTime.Now.Date,
            };
            PlaylistCreation.playlistItem = newPlaylistItem;
            Debug.WriteLine("PlaylistID CALL " + playlistID);
            PlaylistCreation.CallPlaylistCreationAdd();
        }
        PlaylistItem? playlistItemtoEdit;
        public void PlaylistEdit(PlaylistItem playlist)
        {
            Debug.WriteLine("DYD");
            txtEditPlaylistName.Text = playlist.PlaylistName;
            txtEditGenre.Text = playlist.PlaylistGenre;
            this.Loaded -= NewPlaylistCreation_Loaded;
            this.Loaded += NewPlaylistCreation_Loaded;

        }
        public PlaylistItem GetEditedPlaylistItem()
        {
            Debug.WriteLine("tEST1");

            HashSet<string> songpaths = new();
            foreach (var item in AllSongs)
            {
                if (item.FilePath != null)
                {
                    songpaths.Add(item.FilePath);
                }
            }
            Uri defaultPath = new Uri("ms-appx:///Assets/playlistdefaultdark.png");


            if (playlistcoverpath != "")
            {
                defaultPath = new Uri(playlistcoverpath);
            }
            else
            {
                Uri darkIcon = new Uri("ms-appx:///Assets/playlistdefaultdark.png");

                // Set your initial default (e.g., based on current theme)
                defaultPath = darkIcon;
            }
            var pl = new PlaylistItem
            {
                PlaylistName = txtEditPlaylistName.Text,
                PlaylistGenre = txtEditGenre.Text,
                SongsPaths = songpaths,
                PlaylistCount = $"{AllSongs.Count} {(AllSongs.Count == 1 ? "item" : "items")}",
                Thumbnail = defaultPath
    ,
            };
            PlaylistCreation.playlistItem = pl;
            return pl;
        }
        private async void NewPlaylistCreation_Loaded(object sender, RoutedEventArgs e)
        {
            if (PlaylistCreation.playlistItem != null)
            {
                txtEditPlaylistName.Text = PlaylistCreation.playlistItem.PlaylistName;
                txtEditGenre.Text = PlaylistCreation.playlistItem.PlaylistGenre;
                imgPlaylistCov.Source = new BitmapImage(PlaylistCreation.playlistItem.Thumbnail);
                CoverOptions.Visibility = Visibility.Visible;
                playlistcoverpath = PlaylistCreation.playlistItem.Thumbnail!.ToString();
                AllSongs.Clear();
                foreach (var path in PlaylistCreation.playlistItem.SongsPaths)
                {
                    if (!AllSongs.Any(s => s.FilePath == path))
                    {
                        var file = await StorageFile.GetFileFromPathAsync(path);
                        var musicProps = await file.Properties.GetMusicPropertiesAsync();

                        string duration = FormatTimeSpanDuration.Format(musicProps.Duration);
                        var glyph = "\uEC4F";
                        string fileExtension = file.FileType.ToLowerInvariant();
                        if (Extensions.VideoExtensions.List.Contains(fileExtension))
                        {

                            glyph = "\uE8B2";
                        }
                        AllSongs.Add(new SongModel
                        {
                            Title = Path.GetFileNameWithoutExtension(file.Path),
                            SongDuration = musicProps.Duration,
                            FilePath = file.Path,
                            Glyph = glyph

                        });
                    }
                }
                lstViewPlaylistAddedSongs.ItemsSource = AllSongs;
                UpdateUI();
            }
        }

        private void btnActualReset_Click(object sender, RoutedEventArgs e)
        {
            txtEditPlaylistName.Text = "Playlist";
            txtEditGenre.Text = "";
            if (AllSongs != null)
            {
                AllSongs.Clear();
            }
            imgPlaylistCov.Source = new BitmapImage(new Uri("ms-appx:///Assets/playlistdefaultdark.png"));
            CoverOptions.Visibility = Visibility.Collapsed;
            btnAddPlaylistCover.IsEnabled = true;
            UpdateUI();
            if (btnReset.Flyout is Flyout f)
            {
                f.Hide();
            }
        }

        private void imgPlaylistCov_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

        }

        private async void btnAddPlaylistCover_Click(object sender, RoutedEventArgs e)
        {

            if (App.OceanDialogInstance == null)
            {

                return;
            }
            var file = await MediaPicker.PickSingleImageFileAsync(App.OceanDialogInstance, "Choose Image");

            if (file != null)
            {
                CoverOptions.Visibility = Visibility.Visible;
                ToolTipService.SetToolTip(imgPlaylistCov, Path.GetFileName(file.Path));
                imgPlaylistCov.Source = new BitmapImage(new Uri(file.Path));
                playlistcoverpath = file.Path;
            }
        }



        private void btnRemovePlaylistCover_Click(object sender, RoutedEventArgs e)
        {
            ToolTipService.SetToolTip(imgPlaylistCov, "");
            playlistcoverpath = "";
            CoverOptions.Visibility = Visibility.Collapsed;
            btnAddPlaylistCover.IsEnabled = true;
            imgPlaylistCov.Source = new BitmapImage(new Uri("ms-appx:///Assets/playlistdefaultdark.png"));
        }

        private void txtEditPlaylistName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.DispatcherQueue.TryEnqueue(() =>
                {
                    textBox.SelectAll();
                });
            }
        }

        private async void btnAddSongs_Click(object sender, RoutedEventArgs e)
        {
            if (App.OceanDialogInstance == null)
            {
                mssgBar.IsOpen = true;
                mssgBar.Title = "Error";
                mssgBar.Message = "An unexpected error occured. Check log details in Settings Page.";
                mssgBar.Severity = InfoBarSeverity.Error;
                Logger.Log("Error code 0x0012oc. Refer the github page for more details.", "PlaylistCreation", Logger.LogLevelType.Error);
                return;
            }

            var files =
                 await MediaPicker.PickMultipleMediaFilesAsync(App.OceanDialogInstance, "Select Media");


            if (files == null) return;

            foreach (var file in files)
            {
                if (AllSongs.Any(s => s.FilePath == file.Path)) continue;
                TimeSpan duration = (await file.Properties.GetMusicPropertiesAsync()).Duration;
                string fileExtension = file.FileType.ToLowerInvariant();
                var glyph = "\uEC4F";
                if (Extensions.VideoExtensions.List.Contains(fileExtension))
                {

                    glyph = "\uE8B2";
                    duration = (await file.Properties.GetVideoPropertiesAsync()).Duration;


                }


                AllSongs.Add(new SongModel
                {
                    Title = Path.GetFileNameWithoutExtension(file.Path),
                    SongDuration = duration,
                    FilePath = file.Path,
                    IsAudioItem = !IsVideoPlaylist,
                    Glyph = glyph
                });
            }
            lstViewPlaylistAddedSongs.StartBringIntoView();
            lstViewPlaylistAddedSongs.ItemsSource = AllSongs;
            UpdateUI();
        }

        private void asbSearchSongs_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            var query = sender.Text.ToLower();
            SearchSongs(query);
        }
        private void SearchSongs(string query)
        {
            if (query != "")
            {
                txtNullAddedSongs.Text = "No songs have been added yet!";
                txtNullAddedSongs.Visibility = Visibility.Collapsed;
                lstViewPlaylistAddedSongs.Visibility = Visibility.Visible;

                var suggestions = AllSongs.Where(p => p.Title != null && p.Title.ToLower().Contains(query));
                if (suggestions.Any())
                {
                    lstViewPlaylistAddedSongs.ItemsSource = suggestions;
                    DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
                    {
                        var options = new BringIntoViewOptions
                        {
                            VerticalAlignmentRatio = 0.0, // Force to the top
                            AnimationDesired = true
                        };

                        lstViewPlaylistAddedSongs?.StartBringIntoView(options);
                    });
                }
                else
                {
                    lstViewPlaylistAddedSongs.Visibility = Visibility.Collapsed;
                    txtNullAddedSongs.Text = "No results found!";
                    txtNullAddedSongs.Visibility = Visibility.Visible;
                }
            }
            else
            {
                lstViewPlaylistAddedSongs.Visibility = Visibility.Visible;

                lstViewPlaylistAddedSongs.ItemsSource = AllSongs;
            }
        }
        private void asbSearchSongs_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var query = sender.Text.Trim().ToLower();
            SearchSongs(query);
        }
        private void UpdateUI()
        {
            if (lstViewPlaylistAddedSongs.Items.Count == 0)
            {
                txtNullAddedSongs.Visibility = Visibility.Visible;
            }
            else
            {
                txtNullAddedSongs.Visibility = Visibility.Collapsed;
            }
        }
        private void mnftRemoveSongFromPlaylistCreation_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is SongModel song)
            {
                AllSongs.Remove(song);
            }

        }
    }
}

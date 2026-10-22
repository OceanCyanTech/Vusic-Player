using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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
using System.Threading;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Helper.UI.Creation;
using Vusic_Player.Configuration.Helper.UI.Customization;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.UI.UserViews;
using Windows.System;
using Windows.UI.Core;
using Page = Microsoft.UI.Xaml.Controls.Page;
using TextBox = Microsoft.UI.Xaml.Controls.TextBox;


namespace Vusic_Player.Pages
{

    public sealed partial class MainPage : Page
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        private DispatcherTimer? _placeholderTimer;
        private List<string>? _suggestions;
        private int _currentIndex = 0;
        private object? _originalHeader;
        public MainPage()
        {
            InitializeComponent();
            App.NavigationFrame = frmMain;
            App.VideoPlayerFrame = frmVid;
            App.MasterFrame = frmRoot;
            CheckforStartupPage();
            PlayerService.mainXamlRoot = XamlRoot;

            _originalHeader = nvgMain.Header;
            InitializeFav();
            InitializeWatermark();
            grdRoot.AddHandler(
              UIElement.KeyDownEvent,
              new KeyEventHandler(async (sender, keyArgs) =>
              {
                  var ctrlState = InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
                  bool isCtrlDown = ctrlState.HasFlag(CoreVirtualKeyStates.Down);
                  var altState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu);

                  var shiftState = InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift);
                  bool isShiftDown = shiftState.HasFlag(CoreVirtualKeyStates.Down);
                  bool isAltDown = altState.HasFlag(CoreVirtualKeyStates.Down);
                  if (isCtrlDown && keyArgs.Key == Windows.System.VirtualKey.T)
                  {
                      System.Diagnostics.Debug.WriteLine("Ctrl+T caught via AddHandler!");
                      PlayerService.CallJumpToTime();
                      keyArgs.Handled = true;
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.I)
                  {
                      if (App.MainWindowInstance is MainWindow wind)
                      {
                          if (PlayerService.CurrentPlayingPath != null && File.Exists(PlayerService.CurrentPlayingPath))
                          {
                              wind.ShowFileInfo(PlayerService.CurrentPlayingPath);
                          }
                      }
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.O)
                  {
                      OpenMedia();
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.P)
                  {
                      CreatePlaylist();
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.F)
                  {
                      if (App.NavigationFrame.Content is not VideoPlayer)
                      {
                          OpenFolder();
                      }
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.H)
                  {
                      if (Uri.TryCreate("https://oceancyantech.github.io/Vusic-Player/feedback.html", UriKind.Absolute, out Uri? uri))
                      {
                          await Launcher.LaunchUriAsync(uri);
                      }
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.R)
                  {
                      if (PlayerService.Masterplayer != null) { PlayerService.Masterplayer.Seek(0); }
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.L)
                  {
                      if (PlayerService.Masterplayer != null && PlayerService.CurrentPlayingPath != null)
                      {
                          OpenFileLocation.PathSelect(PlayerService.CurrentPlayingPath);
                      }
                  }
                  else if (isShiftDown && keyArgs.Key == VirtualKey.M)
                  {
                      if (PlayerService.Masterplayer == null) return;
                      if (PlayerService.CurrentPlayingPath == "") return;
                      PictureInPicture pictureinPicture = new PictureInPicture();
                      pictureinPicture.Activate();
                      MainWindow.HideWindow();
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.Number1)
                  {
                      nvgitHome.IsSelected = true;
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.Number2)
                  {
                      nvgitMusic.IsSelected = true;
                  }
                  else if (isAltDown && keyArgs.Key == VirtualKey.S)
                  {
                      panelControlAudioPlayer.OpenSpeed();
                  }
                  else if (isAltDown && keyArgs.Key == VirtualKey.Left)
                  {
                      if (App.NavigationFrame != null)
                      {
                          if (App.NavigationFrame.CanGoBack)
                          {
                              App.NavigationFrame.GoBack();
                          }
                      }
                  }
                  else if (isAltDown && keyArgs.Key == VirtualKey.P)
                  {
                      panelControlAudioPlayer.OpenPitch();
                  }
                  else if (isAltDown && keyArgs.Key == VirtualKey.R)
                  {
                      if (App.NavigationFrame.Content is not VideoPlayer)
                      {
                          panelControlAudioPlayer.OpenReverb();
                      }
                  }
                  else if (isAltDown && keyArgs.Key == VirtualKey.M)
                  {
                      if (App.NavigationFrame.Content is not VideoPlayer)
                      {
                          panelControlAudioPlayer.OpenMultiDeviceOutput();
                      }
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.Number3)
                  {
                      nvgitVideo.IsSelected = true;
                  }
                  else if (isCtrlDown && keyArgs.Key == VirtualKey.Number4)
                  {
                      nvgitQueue.IsSelected = true;
                  }
              }),
             handledEventsToo: false
          );
        }
        bool iscreating = false;
        private async void CreatePlaylist()
        {
            if (App.MainWindowInstance == null) return;
            OceanContentDialog.Show("Create New Playlist", "Create", "", "Cancel", OceanDialogWindow.ContentType.PlaylistCreation, OceanContentDialogDefault.Primary, XamlRoot, 600, 760, OceanContentDialogType.Elevated, App.MainWindowInstance, "addicon", "", "", new System.Collections.ObjectModel.ObservableCollection<SongModel>(), "Playlist", "", "", "", "", new PlaylistItem(), false, false);
            OceanContentDialog.PrimaryRequested += (async () =>
            {
                if (iscreating) return;
                try
                {
                    iscreating = true;
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    string baseName = PlaylistCreationValues.Instance.PlaylistName.Trim();

                    if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";

                    string finalName = baseName;
                    int counter = 1;
                    while (currentSettings.SavedPlaylists.Any(p =>
                        string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                    {
                        finalName = $"{baseName} ({counter++})";
                    }
                    var newplaylist = new PlaylistItem { PlaylistName = finalName, PlaylistGenre = PlaylistCreationValues.Instance.Genre, plthumb = PlaylistCreationValues.Instance.PlCover, PlaylistId = PlaylistCreationValues.Instance.PlaylistID, DateCreation = PlaylistCreationValues.Instance.CreationDate, SongsPaths = new HashSet<string>(PlaylistCreationValues.Instance.MediaPaths), Thumbnail = PlaylistCreationValues.Instance.Thumbnail };
                    Debug.WriteLine(newplaylist.PlaylistName);
                    PlaylistCreationValues.Instance.PlaylistsMaster.Add(newplaylist);

                    currentSettings.SavedPlaylists.Add(newplaylist);
                    await SettingsLoader.SaveSettingsAsync(currentSettings);
                    OceanContentDialog.HideDlg();
                    MainWindow.ShowWindow();
                }
                catch (Exception ex)
                {
                    Logger.Log("An unexpected error occured: " + ex.Message, "PlaylistCreate.MusicLibPage", Logger.LogLevelType.Error);
                }
                finally
                {
                    iscreating = false;
                }
            });

        }
        private async void OpenFolder()
        {
            try
            {
                if (App.MainWindowInstance == null) return;
                var folder = await FilePickers.FolderPickerFunct.PickFolder(App.MainWindowInstance, "Open Folder", Windows.Storage.Pickers.PickerLocationId.Downloads);
                if (folder != null)
                {
                    if (App.NavigationFrame != null)
                    {
                        App.NavigationFrame.Navigate(typeof(FolderView), new FolderModel { FolderName = Path.GetFileName(folder.Path), FolderPath = folder.Path });
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "HOMEVIEW.OPENFOLDER", Logger.LogLevelType.Error);
            }
        }
        private async void OpenMedia()
        {
            try
            {
                if (App.MainWindowInstance == null) return;
                var media = await MediaPicker.PickSingle(App.MainWindowInstance, "Open Media");
                if (media != null)
                {
                    bool isAudio = AudioExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                    bool isVideo = VideoExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                    if (isAudio)
                    {
                        if (File.Exists(media.Path))
                        {
                            ObservableCollection<SongModel> single = new();
                            string Title = Path.GetFileNameWithoutExtension(media.Path);

                            single.Add(new SongModel { FilePath = media.Path, Title = Title, AlbumName = AudioMetadata.Album(media.Path), Artist = AudioMetadata.Artist(media.Path), SongDuration = await AudioMetadata.GetTimeSpanDuration(media.Path) });
                            QueueService.PlayMedia(single, false, false);
                        }
                    }
                    else if (isVideo)
                    {
                        if (PlayerService.InVideoPage == false)
                        {
                            if (File.Exists(media.Path))
                                Frame.Navigate(typeof(VideoPlayer), media.Path);
                        }
                        else
                        {
                            PlayerService.OpenPath(media.Path);
                        }
                    }
                    else
                    {
                        ShowException("Error", "Cannot Open file of Invalid Format");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowException("Error", $"An unexpected error occured. See log page for more details.");
                Logger.Log(ex.Message, "MAINPAGE.OPENMEDIAKEYBOARDSHORTCUT", Logger.LogLevelType.Error);
            }

        }
        private void ShowException(string title, string Message)
        {
            ttEXCEPTION.Title = title;
            txtExceptionError.Text = Message;
            ttEXCEPTION.IsOpen = true;
        }

        private async void CheckforStartupPage()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            var startuppage = currentSettings.StartupPage;
            if (startuppage == "Home")
            {
                nvgitHome.IsSelected = true;
                frmMain.Navigate(typeof(HomeView));
            }
            else if (startuppage == "Music")
            {
                nvgitMusic.IsSelected = true;
                frmMain.Navigate(typeof(MusicLibrary));
            }
            else if (startuppage == "Video")
            {
                nvgitVideo.IsSelected = true;
                frmMain.Navigate(typeof(VideoLibrary));
            }
        }
        private bool IsTextInputFocused()
        {
            var focused = FocusManager.GetFocusedElement(this.Content.XamlRoot);
            return focused is TextBox
                || focused is RichEditBox
                || focused is AutoSuggestBox;
        }

        private void Spacebar_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (IsTextInputFocused())
            {
                return; // Let user type space into the text box
            }

            PlayerService.PlayPause();
            args.Handled = true;
        }

        private void ControlRoot_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                // Don't swallow space if user is typing in a search/text box!
                if (IsTextInputFocused())
                {
                    return;
                }

                PlayerService.PlayPause();
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Right)
            {
                if (IsTextInputFocused())
                {
                    return;
                }

                PlayerService.SeekAhead();
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Left)
            {
                if (IsTextInputFocused())
                {
                    return;
                }

                PlayerService.SeekBefore();
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.M)
            {
                if (mediacontroller.IsMuted == false)
                {
                    mediacontroller.IsMuted = true;
                    mediacontroller.OldVolume = mediacontroller.VolumeValue;
                    PlayerService.VolumeChange(0);

                }
                else
                {
                    mediacontroller.IsMuted = false;
                    PlayerService.VolumeChange(mediacontroller.OldVolume);
                }

            }
            else if (e.Key == Windows.System.VirtualKey.Up)
            {
                var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
                bool isCtrlDown = (ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;

                if (isCtrlDown)
                {
                    e.Handled = true; // Prevents UI focus or scrolling from capturing Up Arrow

                    var currentVolume = mediacontroller.VolumeValue;
                    var newVolume = Math.Min(currentVolume + 10, mediacontroller.MaximumVolume);

                    if (newVolume != currentVolume)
                    {
                        PlayerService.VolumeChange(newVolume);
                        Debug.WriteLine($"Volume Up +: {newVolume}");
                    }

                }
            }
            //else if (e.Key == Windows.System.VirtualKey.T)
            //{
            //    var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
            //    bool isCtrlDown = (ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;

            //    if (isCtrlDown)
            //    {
            //        e.Handled = true; // Prevents UI focus movement or scrolling down

            //        PlayerService.CallJumpToTime();
            //    }
            //}

            else if (e.Key == Windows.System.VirtualKey.Down)
            {
                var ctrlState = Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(Windows.System.VirtualKey.Control);
                bool isCtrlDown = (ctrlState & Windows.UI.Core.CoreVirtualKeyStates.Down) == Windows.UI.Core.CoreVirtualKeyStates.Down;

                if (isCtrlDown)
                {
                    e.Handled = true; // Prevents UI focus movement or scrolling down

                    var currentVolume = mediacontroller.VolumeValue;
                    var newVolume = Math.Max(currentVolume - 10, 0);

                    if (newVolume != currentVolume)
                    {
                        PlayerService.VolumeChange(newVolume);
                        Debug.WriteLine($"Volume Down -: {newVolume}");
                    }

                }
            }

        }
        private async void InitializeWatermark()
        {
            var settings = await SettingsLoader.LoadSettingsAsync();

            // 2. Hydrate the watermark singleton if settings exist
            if (settings?.Watermark != null)
            {
                WatermarkFontValues.Instance.LoadFromSettingsModel(settings.Watermark);
            }
        }
        private async void InitializeFav()
        {
            await globalFavourites.InitializeAsync();
        }
        GlobalFavourites globalFavourites => GlobalFavourites.Instance;
        private void UpdatePlaceholderText()
        {
            _suggestions = new List<string>
        {
            "Search for files across your library...",
            "Try 'Recent Music'...",
            "Find 'User Profile'...",
            "Looking for 'Advanced Tools'?"
        };

            // 2. Initialize the Timer
            _placeholderTimer = new DispatcherTimer();
            _placeholderTimer.Interval = TimeSpan.FromSeconds(3);
            _placeholderTimer.Tick += _placeholderTimer_Tick; ;

            // 3. Start the timer
            _placeholderTimer.Start();
        }

        private void _placeholderTimer_Tick(object? sender, object e)
        {
            if (_suggestions == null) return;
            _currentIndex = (_currentIndex + 1) % _suggestions.Count;

            // Update the UI
            //asbMaster.PlaceholderText = _suggestions[_currentIndex];
        }


        private void nvgMain_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (frmMain.CanGoBack)
            {
                frmMain.GoBack();
            }
        }
        private void sldMain_DragCompleted()
        {
            PlayerService.SldMain_DragCompleted(sldMain);
        }
        ShowCreationValues showInstance => ShowCreationValues.Instance;
        PlaylistCreationValues playlistInstance => PlaylistCreationValues.Instance;
        public IconHolder IconsInstance => IconHolder.Instance;
        public SeekService SeekServiceInstance => SeekService.Instance;

        private async void InitializeSettings()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var playlists = currentSettings.SavedPlaylists;
            MasterSearchIndex.PlaylistsMaster = new ObservableCollection<PlaylistItem>(playlists);
            var newObservableShows = new ObservableCollection<Show>();
            var newObservablePlaylists = new ObservableCollection<PlaylistItem>();
            string skipBackIcon = currentSettings.SkipBack is 5 or 10 or 15 or 20 or 30
                    ? $"skip{currentSettings.SkipBack}back.png"
                    : "skipbackcustom.png";
            mediacontroller.MaximumSearchResults = currentSettings.MaxSearchResults;
            mediacontroller.MaximumVolume = currentSettings.MaxVolume;
            IconsInstance.SkipBack = $"ms-appx:///Assets/Themes/{currentSettings.Theme}/{skipBackIcon}";
            SeekServiceInstance.SkipAhead = currentSettings.SkipAhead;
            SeekServiceInstance.SkipBack = currentSettings.SkipBack;
            // Skip Forward Icon
            string skipAheadIcon = currentSettings.SkipAhead is 5 or 10 or 15 or 20 or 30
                ? $"skip{currentSettings.SkipAhead}ahead.png"
                : "skipaheadcustom.png";

            IconsInstance.SkipAhead = $"ms-appx:///Assets/Themes/{currentSettings.Theme}/{skipAheadIcon}";
            foreach (var show in currentSettings.Shows)
            {
                var seasoncountstring = $"• {show.SeasonCount} {(show.SeasonCount == 1 ? "season" : "seasons")}";
                newObservableShows.Add(new Show { Poster = show.Poster ?? IconsInstance.DefaultShow, ShowID = show.ShowID, SeasonCountString = seasoncountstring, Name = show.Name, Description = show.Description, Crew = show.Crew, Creators = show.Creators, Tags = show.Tags, Directory = show.Directory, ReleaseDate = show.ReleaseDate });
            }
            foreach (var playlist in currentSettings.SavedPlaylists)
            {
                newObservablePlaylists.Add(new PlaylistItem { PlaylistName = playlist.PlaylistName, PlaylistId = playlist.PlaylistId, PlaylistNowPlaying = playlist.PlaylistNowPlaying, Thumbnail = playlist.Thumbnail, plthumb = new BitmapImage(new Uri(playlist.ThumbnailString)), DateCreation = playlist.DateCreation, PlaylistGenre = playlist.PlaylistGenre, SongsPaths = playlist.SongsPaths });

            }
            showInstance.ShowsMaster = new ObservableCollection<Show>(newObservableShows);
            playlistInstance.PlaylistsMaster = new ObservableCollection<PlaylistItem>(newObservablePlaylists);
            MasterSearchIndex.FoldersOpenedMaster = new ObservableCollection<FolderModel>(currentSettings.FoldersRecent);
            MasterSearchIndex.Pages.Add("Home");
            MasterSearchIndex.Pages.Add("Music Library");
            MasterSearchIndex.Pages.Add("Video Library");
            MasterSearchIndex.Pages.Add("Queue");
            MasterSearchIndex.Pages.Add("Entire Music Library");
            MasterSearchIndex.Pages.Add("Entire Video Library");
            MasterSearchIndex.Pages.Add("Log");
            MasterSearchIndex.Pages.Add("Settings");

            MasterSearchIndex.Settings.Add("Skip Duration"); //
            MasterSearchIndex.Settings.Add("Seek Duration");//
            MasterSearchIndex.Settings.Add("Maximum Volume");//
            MasterSearchIndex.Settings.Add("Customization");//
            MasterSearchIndex.Settings.Add("Theme");//
            MasterSearchIndex.Settings.Add("Font");//
            MasterSearchIndex.Settings.Add("Personalization");//
            MasterSearchIndex.Settings.Add("Skin");//
            MasterSearchIndex.Settings.Add("Home Page Panels");//
            MasterSearchIndex.Settings.Add("Music Library Page Panels");//
            MasterSearchIndex.Settings.Add("Video Library Page Panels");//
            MasterSearchIndex.Settings.Add("Playlist Settings");//
            MasterSearchIndex.Settings.Add("Audio Metadata Settings");//
            MasterSearchIndex.Settings.Add("Artist Settings");//
            MasterSearchIndex.Settings.Add("Album Settings");//
            MasterSearchIndex.Settings.Add("Genre Settings");//
            MasterSearchIndex.Settings.Add("Continue Playing Videos Settings");//
            MasterSearchIndex.Settings.Add("Shows Settings");//
            MasterSearchIndex.Settings.Add("Media Database");//
            MasterSearchIndex.Settings.Add("Folder Permissions");//
            MasterSearchIndex.Settings.Add("Startup Settings");//
            MasterSearchIndex.Settings.Add("App Lock");//
            MasterSearchIndex.Settings.Add("Splash Screen");//
            MasterSearchIndex.Settings.Add("Startup Page");//
            MasterSearchIndex.Settings.Add("Default App for Formats");//
            MasterSearchIndex.Settings.Add("Reset App");//
            MasterSearchIndex.Settings.Add("About App");//
            MasterSearchIndex.Settings.Add("App License");//
            MasterSearchIndex.Settings.Add("Updates");//
            MasterSearchIndex.Settings.Add("Help");//
            MasterSearchIndex.Settings.Add("Feedback");//
            MasterSearchIndex.Settings.Add("Suggestions");//
            MasterSearchIndex.Settings.Add("Privacy");//
            MasterSearchIndex.Settings.Add("Source Code");//
            MasterSearchIndex.Settings.Add("Documentation");//
            MasterSearchIndex.Settings.Add("Keyboard Shortcuts");
            MasterSearchIndex.Settings.Add("How to use Vusic Player?");
        }

        private async void FrmMain_Navigated(object sender, NavigationEventArgs e)
        {
            nvgMain.IsBackEnabled = frmMain.CanGoBack;
            nvgMain.AlwaysShowHeader = true;
            txtHeader.Text = "";
            nvgMain.IsPaneVisible = true;
            grdsplittr.Visibility = Visibility.Visible;
            blackBackground.Visibility = Visibility.Collapsed;
            ColumnMaster.MinWidth = 280;
            ColumnMaster.Width = new GridLength(300);
            MusicPlayerMaster.Visibility = Visibility.Visible;
            InitializeSettings();
            //    SetGridBackground();
        
             if (e.SourcePageType == typeof(EntireMusicLibrary))
            {
                txtHeader.Text = "Music Library";
            }
            else if (e.SourcePageType == typeof(EntireVideoLibrary))
            {
                txtHeader.Text = "Video Library";
            }
            else if (e.SourcePageType == typeof(FavouritesView))
            {
                txtHeader.Text = "Favourites";
            }
            else if (e.SourcePageType == typeof(FolderView))
            {
                txtHeader.Text = "";
                nvgMain.SelectedItem = null;
            }
            else if (e.SourcePageType == typeof(HomeView))
            {
                nvgMain.SelectedItem = nvgitHome;
                txtHeader.Text = "Home";
            }
            else if (e.SourcePageType == typeof(MusicLibrary))
            {
                nvgMain.SelectedItem = nvgitMusic;
                txtHeader.Text = "Music Library";
            }
            else if (e.SourcePageType == typeof(VideoLibrary))
            {
                nvgMain.SelectedItem = nvgitVideo;
                txtHeader.Text = "Video Library";
            }
            else if (e.SourcePageType == typeof(QueuePage))
            {
                nvgMain.SelectedItem = nvgitQueue;
                txtHeader.Text = "";

                MusicPlayerMaster.Visibility = Visibility.Collapsed;
                ColumnMaster.MinWidth = 0;
                ColumnMaster.Width = new GridLength(0);
                grdsplittr.Visibility = Visibility.Collapsed;
            }
            else if (e.SourcePageType == typeof(MusicPlayerFull))
            {
                MusicPlayerMaster.Visibility = Visibility.Collapsed;
                ColumnMaster.MinWidth = 0;
                ColumnMaster.Width = new GridLength(0);
                grdsplittr.Visibility = Visibility.Collapsed;
                txtHeader.Text = "";

            }
            else if (e.SourcePageType == typeof(VideoPlayer))
            {
                nvgMain.AlwaysShowHeader = false;
                txtHeader.Text = "";
                blackBackground.Visibility = Visibility.Visible;
                nvgMain.IsPaneVisible = false;
                grdsplittr.Visibility = Visibility.Collapsed;
                ColumnMaster.MinWidth = 0;
                MusicPlayerMaster.Visibility = Visibility.Collapsed;
                //    grdRoot.Background = null;
                ColumnMaster.Width = GridLength.Auto;
            }
            else if (e.SourcePageType == typeof(SettingsPage))
            {
                nvgMain.SelectedItem = nvgMain.SettingsItem;
                txtHeader.Text = "Settings";
            }
            else if (e.SourcePageType == typeof(LoggerPage))
                txtHeader.Text = "App Logs";

            else if (e.SourcePageType == typeof(SearchResults))
                txtHeader.Text = "";


            else
                txtHeader.Text = "";
        }

        private void nvgMain_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            MusicPlayerMaster.Visibility = Visibility.Visible;
            if (args.IsSettingsSelected)
            {
                frmMain.Navigate(typeof(SettingsPage));
                return;
            }

            if (args.SelectedItemContainer == null)
                return;

            Type? pageType = null;

            if (args.SelectedItemContainer == nvgitHome)
                pageType = typeof(HomeView);

            else if (args.SelectedItemContainer == nvgitMusic)
                pageType = typeof(MusicLibrary);

            else if (args.SelectedItemContainer == nvgitVideo)
                pageType = typeof(VideoLibrary);

            else if (args.SelectedItemContainer == nvgitQueue)
            {
                frmMain.Navigate(typeof(QueuePage), null, new DrillInNavigationTransitionInfo());
                MusicPlayerMaster.Visibility = Visibility.Collapsed;
            }

            if (pageType != null && frmMain.CurrentSourcePageType != pageType)
            {
                frmMain.Navigate(pageType, null, new DrillInNavigationTransitionInfo());
            }
        }
        ObservableCollection<MasterSearchModel> searchResultsMaster = [];
        private CancellationTokenSource? _cts;

        private async void asbMaster_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // ONLY run when user types
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                _cts?.Cancel();
                _cts = new CancellationTokenSource();
                var token = _cts.Token;

                var targetSearch = sender.Text.Trim();
                if (string.IsNullOrEmpty(targetSearch))
                {
                    sender.ItemsSource = null;
                    return;
                }

                try
                {
                    // Wait 150ms before executing search to smooth out rapid UI changes
                    await Task.Delay(150, token);

                    if (!token.IsCancellationRequested)
                    {
                        var filteredResults = GetFilteredResults(targetSearch);
                        sender.ItemsSource = filteredResults;
                    }
                }
                catch (TaskCanceledException)
                {
                    // Ignored as rapid typing cancelled the previous search task
                }
            }
        }
        private ObservableCollection<MasterSearchModel> GetFilteredResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return new ObservableCollection<MasterSearchModel>();

            var rawQuery = query.Trim();

            var returnableobservable = new ObservableCollection<MasterSearchModel>();

            var rawMedia = FilesInDatabase.rawSongs;
            var rawMediaTitleBoolCheck = rawMedia.All(p => p.Artist?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true);
            Debug.WriteLine("raw media count: " + rawMedia.Count);
            Debug.WriteLine("raw query: " + rawQuery);
            var rawMediaArtist = rawMedia.Where(p => p.Title?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true);
            foreach (var item in rawMediaArtist)
            {
                if (!File.Exists(item.FilePath)) return new ObservableCollection<MasterSearchModel>(); ;
                var subinfo = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "Song" : "Video";
                returnableobservable.Add(new MasterSearchModel { ImageThumbnail = IconsInstance.DefaultArtist, ResultMain = item.Title, SubInformation = subinfo });
            }

            //if (rawMediaTitleBoolCheck)
            //{
            //    Debug.WriteLine("TITLE CHECK");
            //    var rawMediaTitle = rawMedia.Where(p => p.Title?.Contains(rawQuery, StringComparison.OrdinalIgnoreCase) == true);
            //    foreach (var item in rawMediaTitle)
            //    {
            //        var subinfo = (VideoExtensions.List.Contains(Path.GetExtension(item.FilePath).ToLowerInvariant()) == false) ? "Song" : "Video";
            //        returnableobservable.Add(new MasterSearchModel { ImageThumbnail = IconHolder.Instance.DefaultArtist, ResultMain = item.Title, SubInformation = subinfo });
            //    }
            //}
            return returnableobservable;
        }


        private void asbMaster_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {

        }

        private void asbMaster_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            //if (args.SelectedItem is MasterSearchModel search)
            //{
            //    sender.Text = search.ResultMain;
            //}
        }

        private void sldVolume_ValueChanged(double obj)
        {

        }

        private void sldMain_DragStarted()
        {
            PlayerService.SldMain_DragStarted();
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (mediacontroller.ArtistDisplayName == "Unknown Artist") return;
            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(ArtistView), mediacontroller.ArtistDisplayName);
            }
        }

        private void HyperlinkButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (mediacontroller.ArtistDisplayName == "Unknown Album") return;

            if (App.NavigationFrame != null)
            {
                App.NavigationFrame.Navigate(typeof(AlbumView), mediacontroller.AlbumDisplayName);
            }
        }

        private void chckMultioutput_Checked(object sender, RoutedEventArgs e)
        {
            if (chckMultioutput.IsChecked == true)
            {
                mediacontroller.MultiDeviceOutputFeature = true;
                PlayerService.SetupMultiAudioOutput();
            }
            else
            {
                mediacontroller.MultiDeviceOutputFeature = false;
            }
        }

        private void btnMultiDeviceOptions_Click(object sender, RoutedEventArgs e)
        {
            panelControlAudioPlayer.OpenMultiDeviceController();
        }

        private void txtTotalDuration_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }
        private void PKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (IsTextInputFocused())
            {
                return; // Let user type space into the text box
            }
            QueueService.PlayPrevious();
        }
        private void NKey_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (IsTextInputFocused())
            {
                return; // Let user type space into the text box
            }
            QueueService.PlayNext();
        }
        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (IsTextInputFocused())
            {
                return; // Let user type space into the text box
            }

            if (mediacontroller.IsMuted == false)
            {
                mediacontroller.IsMuted = true;
                mediacontroller.OldVolume = mediacontroller.VolumeValue;
                PlayerService.VolumeChange(0);

            }
            else
            {
                mediacontroller.IsMuted = false;
                PlayerService.VolumeChange(mediacontroller.OldVolume);
            }
            args.Handled = true;
        }
    }
}

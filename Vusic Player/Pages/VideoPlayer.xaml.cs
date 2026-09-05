
using FlyleafLib;
using FlyleafLib.MediaFramework.MediaDevice;
using FlyleafLib.MediaPlayer;
using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using Vortice.XAudio2;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.SubtitlesProperties;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Vusic_Player.MediaProperties.AudioProperties;
using Vusic_Player.MediaProperties.VideoProperties;
using Vusic_Player.Pages.Views;
using Vusic_Player.UI;
using Vusic_Player.UI.Dialogs;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.UI.Dialogs.VideoOptions.Audio.AudioGeneral;
using Windows.ApplicationModel.DataTransfer;
using Windows.Devices.Spi;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using WinRT.Interop;
using Logger = Vusic_Player.Configuration.AppConfig.Logger;
using Orientation = Vusic_Player.MediaProperties.VideoProperties.Orientation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages
{

    public sealed partial class VideoPlayer : Page, INotifyPropertyChanged
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        bool isPinned = false;
        private SystemMediaTransportControls? _smtc;

        DateTime recordStartTime;
        DispatcherTimer? SubtitleTimer;
        DispatcherTimer RecordTimer;
        DispatcherTimer? SaveTimer;
        bool LoadingProgress = false;
        public GlowService Glow => GlowService.Instance;
        public Orientation AngleRotate => Orientation.Instance;


        private void PlayerService_PlayCalled()
        {
            if (PlayerService.Masterplayer != null)
            {
                if (PlayerService.Masterplayer.IsPlaying)
                {
                    UpdatePlaybackStatus(MediaPlaybackStatus.Playing);
                }
                else
                {
                    UpdatePlaybackStatus(MediaPlaybackStatus.Paused);

                }
            }
        }
        public void UpdatePlaybackStatus(MediaPlaybackStatus status)
        {
            if (_smtc != null)
            {
                _smtc.PlaybackStatus = status;
            }
        }
        private void PlayerService_PlayPauseChanged()
        {
            if (PlayerService.Masterplayer == null) return;
            if (PlayerService.Masterplayer.IsPlaying)
            {
                //    statsTimerRealTime.Start();
            }
            else
            {
                //    statsTimerRealTime.Stop();
            }
            if (_smtc == null) return;
            if (PlayerService.Masterplayer.IsPlaying)
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else
            {
                _smtc.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
        }
        private async void SystemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            if (PlayerService.Masterplayer == null) return;
            this.DispatcherQueue.TryEnqueue(() =>
            {
                switch (args.Button)
                {
                    case SystemMediaTransportControlsButton.Play:
                        PlayerService.Play();
                        break;
                    case SystemMediaTransportControlsButton.Pause:
                        PlayerService.Pause();
                        break;
                }
            });
        }
        private IntPtr _hwnd;

        public VideoPlayer()
        {
            InitializeComponent();
            InitiateInfoText();
            InitiateSeekText();
            UpdateSubtitleStyle();
            this.Loaded += VideoPlayer_Loaded;
            RecordTimer = new();
            RecordTimer.Interval = TimeSpan.FromMilliseconds(300);
            RecordTimer.Tick += RecordTimer_Tick;
            InitiateRecord();
            PlayerService.LoggedMessage -= PlayerService_LoggedMessage;
            PlayerService.LoggedMessage += PlayerService_LoggedMessage;

            PlayerService.PIPRestore -= PlayerService_PIPRestore;
            PlayerService.PIPRestore += PlayerService_PIPRestore;
            SubtitleTimer = new();
            SubtitleTimer.Interval = TimeSpan.FromMilliseconds(250);
            SubtitleTimer.Tick += SubtitleTimer_Tick;
            PlayerService.ErrorCalled -= PlayerService_ErrorCalled; ;
            PlayerService.ErrorCalled += PlayerService_ErrorCalled; ;

        }
        public interface ISystemMediaTransportControlsInterop

        {

            IntPtr GetForWindow(IntPtr appWindow, [System.Runtime.InteropServices.In] ref Guid riid);

        }

        public async void SetupSMTC()

        {

            // Get the controls for the current view

            _smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;

            _smtc.IsPlayEnabled = true;

            _smtc.IsPauseEnabled = true;

            _smtc.IsNextEnabled = true;

            _smtc.IsPreviousEnabled = true;



            var updater = _smtc.DisplayUpdater;

            _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;

            updater.Type = MediaPlaybackType.Video;
            updater.VideoProperties.Subtitle = "Current media (Vusic Player)";
            updater.VideoProperties.Title = mediacontroller.MediaDisplayName;

            //StorageFolder cacheFolder = ApplicationData.Current.LocalCacheFolder;
            //string thumbPath = vditem.ThumbnailPath ?? "";
            //if (PlayerService.Masterplayer == null) return;
            //PlayerService.Masterplayer.TakeSnapshotToFile(thumbPath, 0, 0, false, false);

            //await Task.Delay(200);
            //Debug.WriteLine("THE THUMBNAIL APS IXS " + thumbPath);
            //REMOVE:
            //StorageFile file = await StorageFile.GetFileFromPathAsync(PlayerService.CurrentPlayingPath);
            //// 2. Create the Stream Reference the SMTC expects
            //updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(file);



            updater.Update();

            // Hook up the event handler for button presses

            _smtc.ButtonPressed += SystemControls_ButtonPressed;

            PlayerService.PlayPauseChanged += PlayerService_PlayPauseChanged;

        }
        private void VideoPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            //lstViewVideoLogs.ItemsSource = LogEntries;
            //  SetupSMTC();
        }
        ObservableCollection<LogEntry> LogEntries = new ObservableCollection<LogEntry>();
        private void InitiateSeekText()
        {
            Configuration.Helper.UI.SeekInfoService.OnSeekRequest += (text, isForward) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    txtSeek.Text = text;
                    txtSeek.HorizontalAlignment = isForward ? HorizontalAlignment.Right : HorizontalAlignment.Left;
                    FadeInOutSeek.Begin();
                });
            };
        }
        private void PlayerService_ErrorCalled()
        {
            if (App.MainWindowInstance == null) return;

            Debug.WriteLine("ERROR CALLED");
            OceanContentDialog.Show("Error", "OK", "", "", OceanDialogWindow.ContentType.MessageShow, OceanContentDialogDefault.Primary, XamlRoot, 400, 400, OceanContentDialogType.Elevated, App.MainWindowInstance, "", "", "", new ObservableCollection<SongModel>(), "", mediacontroller.ErrorMessage, "error");
            OceanContentDialog.PrimaryRequested -= OceanContentDialog_PrimaryRequested;
            OceanContentDialog.PrimaryRequested += OceanContentDialog_PrimaryRequested;
        }
        private static void OceanContentDialog_PrimaryRequested()
        {
            OceanContentDialog.HideDlg();
            MainWindow.ShowWindow();
        }

        private void PlayerService_PIPRestore()
        {
            hostMedia.Player = PlayerService.Masterplayer;

        }

        private void InitiateRecord()
        {
            Screen.OnRecordRequest += () =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    if (PlayerService.Masterplayer == null) return;
                    stkRecording.Visibility = Visibility.Visible;
                    RecFrame.Visibility = Visibility.Visible;
                    RecFrame.Opacity = 1;
                    RecFrame.Width = PlayerService.Masterplayer.Video.Width;
                    RecFrame.Height = PlayerService.Masterplayer.Video.Height;
                    recordStartTime = DateTime.Now;
                    RecordTimer.Start();
                });
            };
            Screen.OnRecordStopRequest += () =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    StopRecording();
                });
            };
        }
        private void UpdateSubtitleStyle()
        {
            Configuration.Helper.SubtitlesProperties.Customize.OnSubtitleCustomizeRequest += () =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    SyncToActualSubtitle();
                });
            };
        }
        private void SyncToActualSubtitle()
        {
            if (txtSubtitle == null) return;


            txtSubtitle.FontFamily = Configuration.Helper.SubtitlesProperties.Customize.fontFamily;
            txtSubtitle.FontSize = Configuration.Helper.SubtitlesProperties.Customize.FontSize;
            txtSubtitle.FontWeight = Configuration.Helper.SubtitlesProperties.Customize.FontWeight;
            txtSubtitle.FontStyle = Configuration.Helper.SubtitlesProperties.Customize.FontStyle;
            txtSubtitle.FontStretch = Configuration.Helper.SubtitlesProperties.Customize.FontStretch;

            txtSubtitle.Foreground = Configuration.Helper.SubtitlesProperties.Customize.Foreground;
            txtSubtitle.TextDecorations = Configuration.Helper.SubtitlesProperties.Customize.TextDecorations;
            txtSubtitle.CharacterSpacing = Configuration.Helper.SubtitlesProperties.Customize.CharacterSpacing;
            txtSubtitle.TextAlignment = Configuration.Helper.SubtitlesProperties.Customize.TextAlignment;
            txtSubtitle.Margin = Configuration.Helper.SubtitlesProperties.Customize.thickness;
            txtSubtitle.Style = Configuration.Helper.SubtitlesProperties.Customize.style;
            txtSubtitle.VerticalAlignment = Configuration.Helper.SubtitlesProperties.Customize.verticalAlignment;
            txtSubtitle.HorizontalAlignment = Configuration.Helper.SubtitlesProperties.Customize.horizontalAlignment;
        }


        private void RecordTimer_Tick(object? sender, object e)
        {
            if (PlayerService.Masterplayer == null) return;
            if (PlayerService.Masterplayer.IsRecording)
            {
                TimeSpan elapsed = DateTime.Now - recordStartTime;
                txtRecordingTimeFrame.Text = elapsed.ToString(@"hh\:mm\:ss");
            }
            else
            {
                StopRecording();
            }
        }
        private void StopRecording()
        {
            RecordTimer?.Stop();
            stkRecording.Visibility = Visibility.Collapsed;
            RecFrame.Visibility = Visibility.Collapsed;
            Screen.StopRecord();
            videoControls.StopRecording();
        }


        private void InitiateInfoText()
        {
            GeneralInfoService.OnInfoRequest += (text) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    txtInformation.Text = text;
                    FadeInOutStoryboard.Begin();
                });
            };
        }

        private void SubtitleTimer_Tick(object? sender, object e)
        {
            if (PlayerService.Masterplayer == null) return;
            txtSubtitle.Text = PlayerService.Masterplayer.Subtitles.SubsText;
        }

        public static string[] List = {
                ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm",
                ".ts", ".m2ts", ".mts", ".m4v", ".3gp", ".3g2", ".ogv",
                ".mpeg", ".mpg", ".vob", ".rmvb", ".asf", ".m2p"
            };
        private void AnimateHeartFull(FontIcon targetIcon, bool isBecomingFavorite)
        {
            // 1. Get the Visual and Compositor
            var visual = ElementCompositionPreview.GetElementVisual(targetIcon);
            var compositor = visual.Compositor;

            // 2. Setup Scale Animation (The "Pop")
            var scaleAnim = compositor.CreateScalarKeyFrameAnimation();
            scaleAnim.InsertKeyFrame(0.0f, 1.0f);
            scaleAnim.InsertKeyFrame(0.5f, 1.4f); // Pulse size
            scaleAnim.InsertKeyFrame(1.0f, 1.0f);
            scaleAnim.Duration = TimeSpan.FromMilliseconds(400);

            visual.CenterPoint = new Vector3((float)targetIcon.ActualWidth / 2, (float)targetIcon.ActualHeight / 2, 0);
            visual.StartAnimation("Scale.X", scaleAnim);
            visual.StartAnimation("Scale.Y", scaleAnim);

            // 3. Setup Color Animation (The "Fill")
            // Note: We animate the 'Brush.Color' of the visual
            var colorAnim = compositor.CreateColorKeyFrameAnimation();
            colorAnim.Duration = TimeSpan.FromMilliseconds(400);

            if (isBecomingFavorite)
            {
                colorAnim.InsertKeyFrame(0.0f, Colors.Gray);
                colorAnim.InsertKeyFrame(1.0f, Colors.Red);
            }
            else
            {
                colorAnim.InsertKeyFrame(0.0f, Colors.Red);
                colorAnim.InsertKeyFrame(1.0f, Colors.Gray);
            }

            // Create a Brush if one doesn't exist on the visual layer
            var brush = compositor.CreateColorBrush();
            visual.Properties.InsertColor("Color", isBecomingFavorite ? Colors.Red : Colors.Gray);

            // Start the color transition
            // Note: For FontIcon, it's often easier to just swap the Glyph 
            // and let the Composition color animation handle the tint.
            targetIcon.Foreground = new SolidColorBrush(isBecomingFavorite ? Colors.Red : Colors.Gray);
        }

        private async void btnFavourite_Click(object sender, RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var Favourites = currentSettings.Favourites;
            var pathtocheck = PlayerService.CurrentPlayingPath;
            if (pathtocheck == null) return;
            var fillHeartIcon = HeartIcon;
            if (fillHeartIcon == null) return;
            var existing = Favourites.FirstOrDefault(p => p.FilePath == pathtocheck);
            if (existing == null)
            {
                fillHeartIcon.Glyph = "\uEB52";
                ToolTipService.SetToolTip(btnFavourite, "Remove from Favourites");
                AnimateHeartFull(fillHeartIcon, true);
                Favourites.Add(new FavouriteItems { FilePath = pathtocheck });
                // song.FavString = "Remove from Favourites";

            }
            else
            {

                fillHeartIcon.Glyph = "\uEB51";
                AnimateHeartFull(fillHeartIcon, false);
                Favourites.Remove(existing);
                ToolTipService.SetToolTip(btnFavourite, "Add to Favourites");



            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);


            // Favourite button click logic
        }

        private async void btnOpenVidSplash_Click(object sender, RoutedEventArgs e)
        {

            if (App.MainWindowInstance != null)
            {
                var file = await MediaPicker.PickSingleVideo(App.MainWindowInstance, "Open Video");
                if (file == null) return;
                PlayerService.OpenPath(file.Path);
                if (PlayerService.Masterplayer != null)
                {
                    SplashGrid.Visibility = Visibility.Collapsed;
                    MainGrid.Visibility = Visibility.Visible;
                    hostMedia.Player = PlayerService.Masterplayer;
                    PlayerService.Masterplayer.OpenCompleted += Masterplayer_OpenCompleted;

                }
                else
                {
                    txtSplash.Text = "An unexpected error occured! Check log page under App Settings for more details";
                    //   Logger.Log("[ERR-PLY-001] Media player instance is null. Unable to initialize playback.", "VideoPlayerPage.OpenVideo", Logger.LogLevelType.Error);
                }
            }

        }
        public void HidePointer()
        {
            int safetyThrottle = 0;
            while (ShowCursor(false) >= 0 && safetyThrottle < 10)
            {
                safetyThrottle++;
            }
        }
        public void ShowPointer()
        {
            int safetyThrottle = 0;


            while (ShowCursor(true) < 0 && safetyThrottle < 10)
            {
                safetyThrottle++;
            }
        }
        [DllImport("user32.dll", EntryPoint = "ShowCursor", CharSet = CharSet.Auto)]
        private static extern int ShowCursor(bool bShow);
        private void ShowInformation(string information)
        {
            txtInformation.VerticalAlignment = VerticalAlignment.Top;
            txtInformation.HorizontalAlignment = HorizontalAlignment.Center;
            txtInformation.Margin = new Thickness(20, 30, 0, 0);
            txtInformation.Text = information;
            FadeInOutStoryboard.Begin();

        }
        bool isEpisodeVideo = false;
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            SaveTimer?.Stop();
            base.OnNavigatedFrom(e);
        }
        private void InitializeShow()
        {
            btnNextEpisode.Visibility = Visibility.Visible;
            videoControls.ViewEpisodeVisibility = Visibility.Visible;

        }
        private void ShowPanel()
        {

            if (isPinned == false)
            {
                //ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
                FadeInOutStoryboardPanel.Begin();
            }
        }

        #region Context Menu Events

        private async void mnftOpenVideo_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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
                        if (App.NavigationFrame != null)
                        {
                            if (PlayerService.InVideoPage == true)
                            {
                                App.NavigationFrame.GoBack();
                                if (PlayerService.Masterplayer != null)
                                {
                                    if (PlayerService.Masterplayer.IsPlaying)
                                    {
                                        PlayerService.Pause();
                                    }
                                }
                                PlayerService.InVideoPage = false;
                            }


                            ObservableCollection<SongModel> single = new();
                            string Title = Path.GetFileNameWithoutExtension(media.Path);

                            single.Add(new SongModel { FilePath = media.Path, Title = Title, AlbumName = AudioMetadata.Album(media.Path), Artist = AudioMetadata.Artist(media.Path), SongDuration = await AudioMetadata.GetTimeSpanDuration(media.Path) });
                            QueueService.PlayMedia(single, false, false);
                        }
                    }
                }
                else if (isVideo)
                {
                    PlayerService.OpenPath(media.Path);
                }
                else
                {
                    //Handle other cases
                }
            }

        }

        private void mnftVolume_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ttVolume.IsOpen = true;
        }

        private void mnftPrevious_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            QueueService.PlayPrevious();
        }

        private void mnftSkipBack_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PlayerService.SeekBefore();
        }

        private void mnftPlay_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PlayerService.PlayPause();
        }

        private void mnftSkipForward_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PlayerService.SeekAhead();
        }

        private void mnftNext_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            QueueService.PlayNext();
        }

        private void mnftFullScreen_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            FullScreen.FullScreenToggle();
        }
        private async void CheckForFavourite()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var favourites = currentSettings.Favourites;
            var exist = favourites.FirstOrDefault(p => p.FilePath == PlayerService.CurrentPlayingPath);
            if (exist != null)
            {
                HeartIcon.Glyph = "\uEB52";

                AnimateHeartFull(HeartIcon, true);
                ToolTipService.SetToolTip(btnFavourite, "Remove from Favourites");

            }
        }
        private void mnftSubtitleOptions_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
            var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 2, 0, 14);

        }


        private void mnftVideoOptions_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
            var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 0, 0, 0);
        }

        private void mnftAudioOptions_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
            var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 1, 0, 10);
        }

        private void mnftHome_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                if (PlayerService.InVideoPage == true)
                {
                    App.NavigationFrame.GoBack();
                    if (PlayerService.Masterplayer != null)
                    {
                        if (PlayerService.Masterplayer.IsPlaying)
                        {
                            PlayerService.Pause();
                        }
                    }
                    PlayerService.InVideoPage = false;
                }

            }
        }

        #endregion

        public Configuration.Helper.SubtitlesProperties.Stream ViewModelSubtitles { get; } = new();
        bool ShowInformationOpened = true;
        private async void VideoIsEpisode(VideoProgress vdprg)
        {
            isEpisodeVideo = true;
            if (vdprg.FilePath == null) return;

            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var shows = currentSettings.Shows;
            var listofotherepisodes = EpisodeDirectory.GetEpisodeShowInfo(vdprg.FilePath);
            var observablesongcollection = new ObservableCollection<SongModel>();
            foreach (var item in listofotherepisodes)
            {

                observablesongcollection.Add(new SongModel { Title = Path.GetFileName(item.FilePath), VisibilityofVideoInfo = Visibility.Visible, VisibilityofAudioMeta = Visibility.Collapsed, Glyph = "\uE8B2", IsAudioItem = false, FilePath = item.FilePath });
            }
            foreach (var item in observablesongcollection)
            {
                QueueService.VusicQueue.Add(item);
            }
            foreach (var item in observablesongcollection)
            {
                QueueService.VusicQueueNext.Add(item);
            }
            var existingitem = QueueService.VusicQueueNext.FirstOrDefault(p => p.FilePath == vdprg.FilePath);
            if (existingitem != null)
            {
                int index = QueueService.VusicQueueNext.IndexOf(existingitem);
                if (index > 0)
                {
                    for (int i = 0; i < index; i++)
                    {
                        QueueService.VusicQueueNext.RemoveAt(0);
                    }
                }

                QueueService.VusicQueueNext.Remove(existingitem);
            }
            var seasonstosend = new ObservableCollection<PlaylistItem>();
            foreach (var show in shows)
            {
                Debug.WriteLine(show.Name + " Show Name");
                var directorypath = show.Directory;
                var rootPath = directorypath;

                if (directorypath != null)
                {
                    Debug.WriteLine("not null " + directorypath);

                    bool isInsideAndExists = ShowManager.IsFileInDirectory(directorypath, vdprg.FilePath)
                                && File.Exists(vdprg.FilePath);
                    if (isInsideAndExists)
                    {

                        if (Directory.Exists(directorypath))
                        {
                            // 1. Only get the top-level folders (e.g., "Season 1", "Season 2", "Season 3")
                            var primaryFolders = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly).ToList();
                            primaryFolders.Insert(0, rootPath);

                            string pattern = @"\b(season\s*|s)(\d+)\b";

                            foreach (string path2 in primaryFolders)
                            {
                                string folderName = Path.GetFileName(path2);
                                Match match = Regex.Match(folderName, pattern, RegexOptions.IgnoreCase);

                                if (path2 == rootPath) match = Regex.Match(new DirectoryInfo(rootPath).Name, pattern, RegexOptions.IgnoreCase);

                                if (match.Success)
                                {

                                    int seasonNum = Convert.ToInt32(match.Groups[2].Value);
                                    string seasonName = $"Season {seasonNum}";
                                    int episodeCount = 0;

                                    // This variable will track the actual deep folder where files are found!
                                    string actualContentPath = path2;

                                    foreach (var ext in Extensions.VideoExtensions.List)
                                    {
                                        string searchPattern = $"*{ext.ToLower()}";

                                        // Get the full path details of any matching video files inside
                                        var foundFiles = Directory.EnumerateFiles(path2, searchPattern, SearchOption.AllDirectories).ToList();

                                        if (foundFiles.Any())
                                        {
                                            episodeCount += foundFiles.Count;

                                            // Grab the directory name of the first video file found. 
                                            // This is guaranteed to be the real folder containing the episodes!
                                            actualContentPath = Path.GetDirectoryName(foundFiles.First())!;
                                        }
                                    }

                                    string episodeCountString = $"{episodeCount} {(episodeCount == 1 ? "episode" : "episodes")}";

                                    var existingSeason = seasonstosend.FirstOrDefault(p => p.PlaylistName == seasonName);
                                    if (existingSeason == null)
                                    {
                                        var newSeason = new PlaylistItem
                                        {
                                            PlaylistName = seasonName,
                                            PlaylistCount = episodeCountString,

                                            // SAVE THIS: Points exactly to "Season 3\Extra Subfolder" if files are deep
                                            PlaylistId = actualContentPath,

                                            SeasonNumber = seasonNum
                                        };
                                        seasonstosend.Add(newSeason);

                                    }
                                    else
                                    {
                                        existingSeason.PlaylistCount = episodeCountString;
                                        existingSeason.PlaylistId = actualContentPath; // Update path if found
                                    }
                                }
                            }
                            int activeSeasonNumber = 1;
                            string activeSeasonDir = rootPath;

                            // Walk up from the file's containing folder
                            DirectoryInfo dirWalker = new System.IO.FileInfo(vdprg.FilePath).Directory;

                            while (dirWalker != null)
                            {
                                Match dirMatch = Regex.Match(dirWalker.Name, pattern, RegexOptions.IgnoreCase);
                                if (dirMatch.Success)
                                {
                                    activeSeasonNumber = Convert.ToInt32(dirMatch.Groups[2].Value);
                                    activeSeasonDir = dirWalker.FullName;
                                    break;
                                }

                                // Stop if we hit or pass above the show's root directory
                                if (string.Equals(dirWalker.FullName.TrimEnd('\\', '/'),
                                                  rootPath.TrimEnd('\\', '/'),
                                                  StringComparison.OrdinalIgnoreCase))
                                {
                                    break;
                                }

                                dirWalker = dirWalker.Parent;
                            }

                            // If the folder names didn't contain "Season X" / "S01", try matching against the file name itself
                            if (activeSeasonNumber == 1 && dirWalker == null)
                            {
                                Match fileMatch = Regex.Match(Path.GetFileName(vdprg.FilePath), pattern, RegexOptions.IgnoreCase);
                                if (fileMatch.Success)
                                {
                                    activeSeasonNumber = Convert.ToInt32(fileMatch.Groups[2].Value);
                                }
                            }

                            // Assign once outside the loop
                            ShowManager.mainShowPlayable = new ShowData
                            {
                                ShowName = show.Name,
                                episodes = listofotherepisodes.ToList(),
                                ShowID = show.ShowID,
                                seasons = seasonstosend.OrderBy(s => s.SeasonNumber).ToList(),
                                CurrentSeasonNumber = activeSeasonNumber,
                                CurrentSeasonDirectory = activeSeasonDir,

                            };
                            foreach (var season in seasonstosend)
                            {
                                Logger.Log(season.PlaylistName, "VIDEOPLAU", Logger.LogLevelType.Warning);
                            }
                            Logger.Log(activeSeasonDir, "VIDEOPLAU", Logger.LogLevelType.Warning);

                        }

                    }
                }

                //               Debug.WriteLine("Yes Episode");
                //               //     ShowManager.UpdateCurrentSeason(vdprg.FilePath);
                //               var listofotherepisodes = EpisodeDirectory.GetEpisodeShowInfo(vdprg.FilePath);
                //               var sorted = listofotherepisodes
                //.OrderBy(p => int.Parse(p.EpisodeName?.Replace("Episode ", "") ?? "0"));
                //               ShowManager.totalepisodecount = listofotherepisodes.Count;
                //               var filePathsToRemove = sorted.Select(item => item.FilePath).ToHashSet();

                //               // Optimize VusicQueue removal
                //               for (int i = QueueService.VusicQueue.Count - 1; i >= 0; i--)
                //               {
                //                   if (filePathsToRemove.Contains(QueueService.VusicQueue[i].FilePath))
                //                   {
                //                       QueueService.VusicQueue.RemoveAt(i);
                //                   }
                //               }

                //               // Optimize VusicQueueNext removal
                //               for (int i = QueueService.VusicQueueNext.Count - 1; i >= 0; i--)
                //               {
                //                   if (filePathsToRemove.Contains(QueueService.VusicQueueNext[i].FilePath))
                //                   {
                //                       QueueService.VusicQueueNext.RemoveAt(i);
                //                   }
                //               }
                //               QueueService.VusicQueue.Clear();
                //               QueueService.VusicQueueNext.Clear();
                //               foreach (var item in sorted)
                //               {
                //                   var file = await StorageFile.GetFileFromPathAsync(item.FilePath);
                //                   var vidprops = await file.Properties.GetVideoPropertiesAsync();
                //                   var title = vidprops.Title;
                //                   if (title == "")
                //                   {
                //                       title = Path.GetFileName(item.FilePath);
                //                   }
                //                   if (item.FilePath != null)
                //                   {
                //                       QueueService.VusicQueue.Add(new SongModel { Title = title, FilePath = item.FilePath });
                //                   }
                //               }
                //               foreach (var item in QueueService.VusicQueue)
                //               {
                //                   QueueService.VusicQueueNext.Add(new SongModel { Title = item.Title, FilePath = item.FilePath });
                //               }
                //               var exist = QueueService.VusicQueueNext.FirstOrDefault(p => p.FilePath == vdprg.FilePath);

                //               if (exist != null)
                //               {


                //                   int indexbefore = QueueService.VusicQueueNext.IndexOf(exist);
                //                   if (indexbefore == QueueService.VusicQueueNext.Count - 1)
                //                   {
                //                       btnNextEpisode.Content = "Next Season";
                //                       Debug.WriteLine("HERE IT IS BEING UPDATED  3");

                //                   }
                //                   // Ensure the item was actually found (-1 means not found)
                //                   if (indexbefore != -1)
                //                   {
                //                       // Loop indexbefore + 1 times to include the 'exist' item itself
                //                       int itemsToRemove = indexbefore + 1;

                //                       for (int i = 0; i < itemsToRemove; i++)
                //                       {
                //                           if (QueueService.VusicQueueNext.Count > 0)
                //                           {
                //                               QueueService.VusicQueueNext.RemoveAt(0);
                //                           }
                //                       }
                //                   }
                //                   //        QueueService.VusicQueueNext.Remove(exist);
                //               }
                //               foreach (var item in QueueService.VusicQueueNext)
                //               {
                //                   Debug.WriteLine(item.FilePath + " Next");

                //               }
                //               ShowManager.LoadAvailableShow(vdprg.FilePath);
                UpdateNextEpisodeButtonContent(vdprg.FilePath);
            }
            btnNextEpisode.Visibility = Visibility.Visible;
            videoControls.ViewEpisodeVisibility = Visibility.Visible;

            UpdateNextEpisodeButtonContent(vdprg.FilePath);
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            PlayerService.InVideoPage = true;
            btnNextEpisode.Visibility = Visibility.Collapsed;
            Debug.WriteLine("SJDHDHDHDHAUHIU4GU");
            string VideoPath = "";
            isEpisodeVideo = false;
            if (e.Parameter is VideoProgress vdprg && vdprg.FilePath is string path)
            {
                VideoPath = path;
                LoadingProgress = true;
                ShowInformationOpened = vdprg.ShowInformationOfOpen;

                if (vdprg.IsEpisode == true)
                {
                    VideoIsEpisode(vdprg);
                }
                else
                {
                    Debug.WriteLine("Not Episode");

                    btnNextEpisode.Visibility = Visibility.Collapsed;
                    videoControls.ViewEpisodeVisibility = Visibility.Collapsed;
                }

            }
            else if (e.Parameter is string Path)
            {
                VideoPath = Path;
                LoadingProgress = false;

            }
            else if (e.Parameter is EpisodeModel episode)
            {
                Debug.WriteLine("Yesss111");
                btnNextEpisode.Visibility = Visibility.Visible;
                videoControls.ViewEpisodeVisibility = Visibility.Visible;
                if (episode.FilePath == null) return;
                Debug.WriteLine("Yesss222");



                VideoPath = episode.FilePath;
                isEpisodeVideo = true;
            }
            else if (e.Parameter is ShowData show)
            {
                ShowManager.mainShowPlayable = show;
                if (show.episodes.Count > 0)
                {
                    VideoPath = show.episodes[0].FilePath;
                    isEpisodeVideo = true;
                    InitializeShow();
                }
            }
            else if (e.Parameter is bool breakofnavigation)
            {
                if (breakofnavigation == true)
                {
                    if (PlayerService.Masterplayer != null)
                    {
                        // Convert ticks to milliseconds and force an accurate seek
                        SplashGrid.Visibility = Visibility.Collapsed;
                        MainGrid.Visibility = Visibility.Visible;
                        hostMedia.Player = PlayerService.Masterplayer;
                        if (PlayerService.isEpisodeVid)
                        {
                            VideoIsEpisode(new VideoProgress { FilePath = PlayerService.CurrentPlayingPath });
                            PlayerService.isEpisodeVid = false;
                        }

                    }
                    else
                    {
                        txtSplash.Text = "An unexpected error occured! Check log page under App Settings for more details";
                        Logger.Log("[ERR-PLY-001] Media player instance is null. Unable to initialize playback.", "VideoPlayerPage.OpenVideo", Logger.LogLevelType.Error);
                    }
                    return;
                }
            }
            if (PlayerService.Masterplayer == null)
            {
                PlayerService.Masterplayer = new Player();
            }
            PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted;

            PlayerService.Masterplayer.OpenCompleted += Masterplayer_OpenCompleted;
            PlayerService.LookForProgressForNextVideo(VideoPath);




            if (PlayerService.Masterplayer != null)
            {
                // Convert ticks to milliseconds and force an accurate seek
                SplashGrid.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;
                hostMedia.Player = PlayerService.Masterplayer;

            }
            else
            {
                txtSplash.Text = "An unexpected error occured! Check log page under App Settings for more details";
                Logger.Log("[ERR-PLY-001] Media player instance is null. Unable to initialize playback.", "VideoPlayerPage.OpenVideo", Logger.LogLevelType.Error);
            }
            CheckForFavourite();
            base.OnNavigatedTo(e);
        }

        private async void Masterplayer_OpenCompleted(object? sender, OpenCompletedArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            if (PlayerService.CurrentPlayingPath == null) return;
            FadeInOutStoryboardPanel.Begin();
            if (ShowInformationOpened)
            {
                if (PlayerService.DONTSHOWGENERALINFORMATION == false)
                {
                    ShowInformation($"'{PlayerService.CurrentPlayingPath}' opened");
                }
            }

            // Assign it to the UI grid
            PlayerService.Masterplayer.Config.Video.SDRDisplayNits = 55;
            SubtitleTimer?.Start();
            SaveTimer = new DispatcherTimer();
            SaveTimer.Interval = TimeSpan.FromSeconds(4);
            SaveTimer.Tick += async (s, e) =>
            {
                if (string.IsNullOrEmpty(PlayerService.CurrentPlayingPath) || PlayerService.Masterplayer == null) return;

                var settings = await SettingsLoader.LoadSettingsAsync();
                var item = settings.SavedVideoProgress.FirstOrDefault(x => x.FilePath == PlayerService.CurrentPlayingPath);
                if (item != null)
                {
                    //Already Exists
                    int originalindex = settings.SavedVideoProgress.IndexOf(item);
                    if (originalindex != -1)
                    {
                        int lastIndex = settings.SavedVideoProgress.Count - 1;
                        settings.SavedVideoProgress.Move(originalindex, lastIndex);
                    }

                    var currentStream = ViewModelSubtitles.CurrentStream;
                    var index = PlayerService.Masterplayer.Subtitles.Streams.IndexOf(currentStream);
                    int indexofSubtitle = index;
                    bool isSubtitlesEnabled = PlayerService.Masterplayer.Config.Subtitles.Enabled;
                    //  item.PlayCount++;
                    item.IsSubtitlesDisabled = !isSubtitlesEnabled;
                    item.CurrentDuration = PlayerService.Masterplayer.CurTime;
                    item.TotalDuration = PlayerService.Masterplayer.Duration;
                    item.IsEpisode = isEpisodeVideo;
                    item.SubtitleIndex = indexofSubtitle;
                }
                else
                {
                    string path = PlayerService.CurrentPlayingPath;
                    bool isSubtitlesEnabled = PlayerService.Masterplayer.Config.Subtitles.Enabled;
                    var currentStream = ViewModelSubtitles.CurrentStream;
                    var index = PlayerService.Masterplayer.Subtitles.Streams.IndexOf(currentStream);
                    int indexofSubtitle = index;

                    settings.SavedVideoProgress.Add(new VideoProgress
                    {

                        FilePath = path,
                        IsSubtitlesDisabled = !isSubtitlesEnabled,
                        IsEpisode = isEpisodeVideo,
                        SubtitleIndex = indexofSubtitle,

                        CurrentDuration = PlayerService.Masterplayer.CurTime,
                        TotalDuration = PlayerService.Masterplayer.Duration
                    });
                }

                await SettingsLoader.SaveSettingsAsync(settings);
            };
            SaveTimer?.Start();
            if (ContinuePlaying.videoProgressMain is VideoProgress vdprg && LoadingProgress == true)
            {
                //z   isEpisodeVideo = vdprg.IsEpisode ?? false;
                PlayerService.Masterplayer.SeekAccurate((int)(vdprg.CurrentDuration / 10000));
                if (vdprg.IsSubtitlesDisabled == true)
                {
                    PlayerService.Masterplayer.Config.Subtitles.Enabled = false;
                }
                else
                {
                    PlayerService.Masterplayer.Config.Subtitles.Enabled = true;
                }
                Debug.WriteLine(vdprg.SubtitleIndex + " is the subtitle index");

                LoadingProgress = false;
                var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
                mediacontroller.CurrentPosition = curTime.TotalSeconds;
                if (ShowInformationOpened)
                {
                    ShowInformation($"Opened playback of '{PlayerService.CurrentPlayingPath}' at {mediacontroller.RunningDurationString}");
                }


            }
            else if (LoadingProgress == false)
            {
                PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted;

                return;
            }
            //LoadSettings();
            //LoadOptions();

            SetupSMTC();
            PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted;

        }
        private void btnCloseInformation_Click(object sender, RoutedEventArgs e)
        {
            grdInfo.Opacity = 0;

        }

        private void btnPin_Click(object sender, RoutedEventArgs e)
        {
            if (!isPinned)
            {
                isPinned = true;
                Grid.SetRow(grdPlayback, 1);
                FadeInOutStoryboardPanel.Stop();

                // Alignment & Stretch
                grdRootPlayback.HorizontalAlignment = ControlsOverlay.HorizontalAlignment = GlassRoot.HorizontalAlignment = grdPlayback.HorizontalAlignment = grdUnpinnedControls.HorizontalAlignment = HorizontalAlignment.Stretch;
                grdRootPlayback.VerticalAlignment = grdUnpinnedControls.VerticalAlignment = ControlsOverlay.VerticalAlignment = GlassRoot.VerticalAlignment = VerticalAlignment.Stretch;
                grdPlayback.Children.Remove(txtSubtitle);
                MainGrid.Children.Add(txtSubtitle);
                // Appearance
                GlassRoot.CornerRadius = new CornerRadius(0);
                GlassRoot.Margin = new Thickness(0);
                grdUnpinnedControls.Margin = new Thickness(0);

                // Visibility & Text

                mediacontroller.DisplayTextVisibility = Visibility.Collapsed;
                btnPin.Visibility = Visibility.Collapsed;
                txtPinFileName.Visibility = btnPin2.Visibility = Visibility.Visible;
                IsNameVisible = Visibility.Collapsed;
                fntPin2.Glyph = "\uE77A";

                // Gradient Brush
                grdRootPlayback.Background = new LinearGradientBrush(new GradientStopCollection
                    {
                        new GradientStop { Color = ColorHelper.FromArgb(0xCC, 0x1A, 0x1A, 0x1A), Offset = 0.0 },
                        new GradientStop { Color = ColorHelper.FromArgb(0xE6, 0x0A, 0x0A, 0x0A), Offset = 1.0 }
                    }, 0)
                {
                    StartPoint = new Windows.Foundation.Point(0, 0),
                    EndPoint = new Windows.Foundation.Point(0, 1)
                };
            }
            else
            {
                isPinned = false;

                Grid.SetRow(grdPlayback, 0);
                FadeInOutStoryboardPanel.Begin();
                grdUnpinnedControls.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Colors.Transparent);
                MainGrid.Children.Remove(txtSubtitle);

                grdPlayback.Children.Add(txtSubtitle);
                // Reset Alignment
                grdRootPlayback.HorizontalAlignment = grdUnpinnedControls.HorizontalAlignment = ControlsOverlay.HorizontalAlignment = GlassRoot.HorizontalAlignment = grdPlayback.HorizontalAlignment = HorizontalAlignment.Center;
                grdRootPlayback.VerticalAlignment = grdUnpinnedControls.VerticalAlignment = ControlsOverlay.VerticalAlignment = GlassRoot.VerticalAlignment = VerticalAlignment.Bottom;

                // Reset Appearance
                grdRootPlayback.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                GlassRoot.CornerRadius = new CornerRadius(24);
                GlassRoot.Margin = new Thickness(0, 0, 0, 20);
                grdUnpinnedControls.Margin = new Thickness(0, 0, 0, 10);

                // Reset Visibility
                txtPinFileName.Visibility = btnPin2.Visibility = Visibility.Collapsed;
                btnPin.Visibility = Visibility.Visible;
                mediacontroller.DisplayTextVisibility = Visibility.Visible;
            }

        }
        private Visibility _isNameVisible = Visibility.Visible;
        public Visibility IsNameVisible
        {
            get => _isNameVisible;
            set
            {
                if (_isNameVisible != value)
                {
                    _isNameVisible = value;
                    OnPropertyChanged(nameof(IsNameVisible));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        private void btnStopRecording_Click(object sender, RoutedEventArgs e)
        {
            StopRecording();
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                PlayerService.InVideoPage = false;
                App.NavigationFrame.Navigate(typeof(SettingsPage));
            }
        }
        private async void btnNextEpisode_Click(object sender, RoutedEventArgs e)
        {
            if (btnNextEpisode.Content.ToString() == "Next Episode")
            {
                Debug.WriteLine("Nanan0");

                if (PlayerService.Masterplayer == null) return;
                PlayerService.Masterplayer.OpenCompleted -= Masterplayer_OpenCompleted;
                Debug.WriteLine(QueueService.VusicQueueNext.Count + " is the count");
                var nextindex = QueueService.VusicQueueNext[0];
                if (nextindex == null) return;
                Debug.WriteLine("Nanan1");
                UpdateNextEpisodeButtonContent(nextindex.FilePath ?? "");

                PlayerService.LookForProgressForNextVideo(nextindex.FilePath ?? "");

                QueueService.PlayNext();
            }
            else if (btnNextEpisode.Content.ToString() == "Next Season")
            {
                LoadNextSeason();

                Debug.WriteLine("TEST1");

            }
        }
        private async void LoadNextSeason()
        {
            var existingseason = ShowManager.mainShowPlayable.seasons.FirstOrDefault(p => p.PlaylistId == ShowManager.mainShowPlayable.CurrentSeasonDirectory);
            if (existingseason == null) return;
            int index1 = ShowManager.mainShowPlayable.seasons.IndexOf(existingseason);
            Logger.Log("CURRENT SEASON INDEX " + index1, "LOADNEXTSEASON", Logger.LogLevelType.Success);
            int nextindex = index1 + 1;
            if (nextindex <= ShowManager.mainShowPlayable.seasons.Count)
            {
                var nextseason = ShowManager.mainShowPlayable.seasons[nextindex];
                Logger.Log("NEXT SEASON NAME " + nextseason.PlaylistName, "LOADNEXTSEASON", Logger.LogLevelType.Success);

                var enumeratedepisodes = EpisodeDirectory.EnumerateEpisodes(nextseason.PlaylistId);
                ShowManager.mainShowPlayable.CurrentSeasonNumber++;
                if (enumeratedepisodes.Count == 0) return;
                var observablesongcollection = new ObservableCollection<SongModel>();
                foreach (var item in enumeratedepisodes)
                {
                    observablesongcollection.Add(new SongModel { Title = Path.GetFileName(item.FilePath), VisibilityofVideoInfo = Visibility.Visible, VisibilityofAudioMeta = Visibility.Collapsed, Glyph = "\uE8B2", IsAudioItem = false, FilePath = item.FilePath });
                }
                foreach (var item in observablesongcollection)
                {
                    QueueService.VusicQueue.Add(item);
                }
                foreach (var item in observablesongcollection)
                {
                    QueueService.VusicQueueNext.Add(item);
                }
                ShowManager.mainShowPlayable.episodes = enumeratedepisodes;
                ShowManager.mainShowPlayable.CurrentSeasonDirectory = nextseason.PlaylistId;
                var firstitem = enumeratedepisodes[0];
                UpdateNextEpisodeButtonContent(firstitem.FilePath);

                var existingitem = QueueService.VusicQueueNext.FirstOrDefault(p => p.FilePath == firstitem.FilePath);
                if (existingitem != null)
                {
                    QueueService.VusicQueueNext.Remove(existingitem);
                }
                QueueService.PlayNext();
            }
            //   if (Directory.Exists(rootPath))
            //   {
            //       Debug.WriteLine("TEST3");

            //       // 1. Only get the top-level folders (e.g., "Season 1", "Season 2", "Season 3")
            //       var primaryFolders = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly).ToList();
            //       primaryFolders.Insert(0, rootPath);

            //       string pattern = @"\b(season\s*|s)(\d+)\b";

            //       foreach (string path in primaryFolders)
            //       {
            //           string folderName = Path.GetFileName(path);
            //           Match match = Regex.Match(folderName, pattern, RegexOptions.IgnoreCase);

            //           if (path == rootPath) match = Regex.Match(new DirectoryInfo(rootPath).Name, pattern, RegexOptions.IgnoreCase);

            //           if (match.Success)
            //           {
            //               int seasonNum = Convert.ToInt32(match.Groups[2].Value);
            //               string seasonName = $"Season {seasonNum}";

            //               int episodeCount = 0;

            //               // This variable will track the actual deep folder where files are found!
            //               string actualContentPath = path;

            //               foreach (var ext in Extensions.VideoExtensions.List)
            //               {
            //                   string searchPattern = $"*{ext.ToLower()}";

            //                   // Get the full path details of any matching video files inside
            //                   var foundFiles = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories).ToList();

            //                   if (foundFiles.Any())
            //                   {
            //                       episodeCount += foundFiles.Count;

            //                       // Grab the directory name of the first video file found. 
            //                       // This is guaranteed to be the real folder containing the episodes!
            //                       actualContentPath = Path.GetDirectoryName(foundFiles.First())!;
            //                   }
            //               }

            //               string episodeCountString = $"{episodeCount} {(episodeCount == 1 ? "episode" : "episodes")}";

            //               var existingSeason = seasons.FirstOrDefault(p => p.PlaylistName == seasonName);
            //               if (existingSeason == null)
            //               {
            //                   Debug.WriteLine("TEST5");

            //                   seasons.Add(new PlaylistItem
            //                   {
            //                       PlaylistName = seasonName,
            //                       PlaylistCount = episodeCountString,

            //                       // SAVE THIS: Points exactly to "Season 3\Extra Subfolder" if files are deep
            //                       PlaylistId = actualContentPath,

            //                       SeasonNumber = seasonNum
            //                   });
            //               }
            //               else
            //               {
            //                   existingSeason.PlaylistCount = episodeCountString;
            //                   existingSeason.PlaylistId = actualContentPath; // Update path if found
            //               }
            //           }
            //       }

            //       if (seasons.Count != 0)
            //       {
            //           if (PlayerService.CurrentPlayingPath == null) return;
            //           Debug.WriteLine("TEST6");
            //           var seasonsRearranged = seasons.OrderBy(p => p.SeasonNumber).ToList();

            //           foreach (var seas in seasonsRearranged)
            //           {
            //               var path = seas.PlaylistId;
            //               if (path != null)
            //               {
            //                   // Check if the current file path starts with the season's directory path
            //                   if (PlayerService.CurrentPlayingPath.StartsWith(path, StringComparison.OrdinalIgnoreCase))
            //                   {
            //                       var curindex = seasonsRearranged.IndexOf(seas);
            //                       ShowManager.currentseason = curindex;
            //                       break; // Found it, no need to keep looping
            //                   }
            //               }
            //           }
            //           ShowManager.currentseason++;
            //           Debug.WriteLine(ShowManager.currentseason + " is the current season now");
            //           foreach (var item in seasonsRearranged)
            //           {
            //               Debug.WriteLine(item.PlaylistName);
            //           }
            //           var nextseason = seasonsRearranged[ShowManager.currentseason];
            //           Debug.WriteLine(nextseason.PlaylistName + " is the next season");
            //           if (nextseason != null)
            //           {
            //               if (nextseason.PlaylistId == null) return;
            //               var listofotherepisodes = EpisodeDirectory.EnumerateEpisodes(nextseason.PlaylistId);
            //               var sorted = listofotherepisodes
            //.OrderBy(p => int.Parse(p.EpisodeName?.Replace("Episode ", "") ?? "0"));
            //               ShowManager.totalepisodecount = listofotherepisodes.Count;
            //               // Optimize VusicQueue removal


            //               // Optimize VusicQueueNext removal

            //               QueueService.VusicQueue.Clear();
            //               QueueService.VusicQueueNext.Clear();
            //               foreach (var item in sorted)
            //               {
            //                   var file = await StorageFile.GetFileFromPathAsync(item.FilePath);
            //                   var vidprops = await file.Properties.GetVideoPropertiesAsync();
            //                   var title = vidprops.Title;
            //                   if (title == "")
            //                   {
            //                       title = Path.GetFileName(item.FilePath);
            //                   }
            //                   if (item.FilePath != null)
            //                   {
            //                       QueueService.VusicQueue.Add(new SongModel { Title = title, FilePath = item.FilePath });
            //                   }
            //               }
            //               foreach (var item in QueueService.VusicQueue)
            //               {
            //                   QueueService.VusicQueueNext.Add(new SongModel { Title = item.Title, FilePath = item.FilePath });
            //               }
            //               foreach (var item in QueueService.VusicQueueNext)
            //               {
            //                   Debug.WriteLine(item.FilePath + " Next");

            //               }
            //               var firstitem = QueueService.VusicQueueNext[0];

            //               QueueService.PlayNext();
            //               if (firstitem.FilePath != null)
            //                   UpdateNextEpisodeButtonContent(firstitem.FilePath);
            //           }
            //       }
            //   }

        }
        ObservableCollection<PlaylistItem> seasons = new();

        private void UpdateNextEpisodeButtonContent(string path)
        {
            Debug.WriteLine("Nanan2");

            if (ShowManager.mainShowPlayable == null) return;
            Debug.WriteLine("Nanan2.5");

            if (path == "") return;
            Debug.WriteLine("Nanan3");

            var episodePatterns = new List<string>
{
    // 1. Standard SxxExx or just Exx (Looks for 'E' or 'EP' optionally preceded by 'Sxx')
    @"(?i)(?:s\d+)?e(\d+)\b",

    // 2. Multi-episode format: E02-E03, E02E03, e02_03
    @"(?i)e(\d+)(?:[-_]?e?(\d+))?\b",

    // 3. Standard text 'episode' or 'ep' followed by numbers (e.g., Ep.01, Episode 1)
    @"(?i)\b(?:ep|episode)(?:\s*|\s*\.\s*)(\d+)\b",

    // 4. X / Cross format: S01x02, 1x02, 1x2
    @"(?i)\b\d+x(\d+)\b",

    // 5. Bracketed numbers (Anime style): [02], (02)
    @"\[(\d+)\]",
    @"\((\d+)\)",

    // 6. Absolute / Standalone numbers: "Show - 02.mp4" 
    @"(?<=\s+|-|_|#)(\d+)(?=\.\w+$|\s+|-|_)"
};                            //                            // 2. Get only the files that match your video extensions
            var episodeNumber = "Unknown";
            Debug.WriteLine("Nanan4");

            foreach (var pattern in episodePatterns)
            {
                Match match = Regex.Match(Path.GetFileNameWithoutExtension(path), pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var validGroups = match.Groups.Cast<Group>()
                                                  .Skip(1)
                                                  .Where(g => g.Success && !string.IsNullOrEmpty(g.Value))
                                                  .ToList();

                    Debug.WriteLine($"  [Check] Pattern '{pattern}' matched! Found {validGroups.Count} valid capture groups.");

                    if (validGroups.Any())
                    {
                        episodeNumber = validGroups.First().Value;
                        Debug.WriteLine($"  -> Match Found! Episode: {episodeNumber}");

                        break;
                    }
                }
            }
            Debug.WriteLine("Nanan5");
            Debug.WriteLine(episodeNumber);

            if (int.TryParse(episodeNumber, out int episode))
            {
                Debug.WriteLine(episode);
                Debug.WriteLine(ShowManager.mainShowPlayable.episodes.Count);
                Logger.Log("CURRENT EPISODE COUNT: " + ShowManager.mainShowPlayable.episodes.Count, "VideoPlayer.UpdateNextEpisodeButton", Logger.LogLevelType.Information);
                Logger.Log("CURRENT EPISODE NUMBER:  " + episode, "VideoPlayer.UpdateNextEpisodeButton", Logger.LogLevelType.Information);

                if (episode == ShowManager.mainShowPlayable.episodes.Count)
                {
                    Debug.WriteLine("Nanan6");
                    Logger.Log("CURRENT SEASON: " + ShowManager.mainShowPlayable.CurrentSeasonNumber, "VideoPlayer.UpdateNextEpisodeButton", Logger.LogLevelType.Information);
                    Logger.Log("CURRENT SEASON COUNT: " + ShowManager.mainShowPlayable.seasons.Count, "VideoPlayer.UpdateNextEpisodeButton", Logger.LogLevelType.Information);
                    if (ShowManager.mainShowPlayable.CurrentSeasonNumber < ShowManager.mainShowPlayable.seasons.Count)
                    {
                        Debug.WriteLine("Nanan7");
                        Logger.Log("CURRENT SEASON: " + ShowManager.mainShowPlayable.CurrentSeasonNumber, "VideoPlayer.UpdateNextEpisodeButton", Logger.LogLevelType.Information);
                        Logger.Log("CURRENT SEASON COUNT: " + ShowManager.mainShowPlayable.seasons.Count, "VideoPlayer.UpdateNextEpisodeButton", Logger.LogLevelType.Information);
                        Debug.WriteLine("CURRENT SEASON: " + ShowManager.mainShowPlayable.CurrentSeasonNumber);
                        Debug.WriteLine("CURRENT SEASON COUNT: " + ShowManager.mainShowPlayable.seasons.Count);

                        btnNextEpisode.Visibility = Visibility.Visible;
                        Debug.WriteLine("HERE IT IS BEING UPDATED 2");
                        btnNextEpisode.Content = "Next Season";
                    }
                    else
                    {
                        btnNextEpisode.Visibility = Visibility.Collapsed;
                    }
                }
                else
                {
                    btnNextEpisode.Visibility = Visibility.Visible;
                    btnNextEpisode.Content = "Next Episode";

                }
            }

        }

        private void MainGrid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            ShowPanel();
        }

        private void MainGrid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

            ShowPanel();
        }

        private void videoControls_ViewEpisodeClick()
        {
            Debug.WriteLine("Show Episodes Clicked");
            if (ShowManager.CurrentShow == null) return;
            Debug.WriteLine("Show Episodes Clicked2");

            FadeInStoryboardShowInfo.Begin();
            txtShowTitle.Text = ShowManager.CurrentShow.Name;
            txtShowSeasonCount.Text = $"• {ShowManager.CurrentShow.SeasonCount} {(ShowManager.CurrentShow.SeasonCount == 1 ? "season" : "seasons")}";
            txtShowReleaseDate.Text = $"• Released on {ShowManager.CurrentShow.ReleaseDate.ToString("dd MMMM yyyy")}";
            string rootPath = ShowManager.CurrentShow.Directory ?? "";

            if (rootPath == "") return;
            Debug.WriteLine("Yes1");

            if (Directory.Exists(rootPath))
            {
                selbarSeasons.Items.Clear();
                var seasons = new ObservableCollection<PlaylistItem>();
                Debug.WriteLine("Yes3");

                // 1. Only get the top-level folders (e.g., "Season 1", "Season 2", "Season 3")
                var primaryFolders = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly).ToList();
                primaryFolders.Insert(0, rootPath);

                string pattern = @"\b(season\s*|s)(\d+)\b";

                foreach (string path in primaryFolders)
                {
                    string folderName = Path.GetFileName(path);
                    Match match = Regex.Match(folderName, pattern, RegexOptions.IgnoreCase);

                    if (path == rootPath) match = Regex.Match(new DirectoryInfo(rootPath).Name, pattern, RegexOptions.IgnoreCase);

                    if (match.Success)
                    {
                        Debug.WriteLine("Yes4");
                        int seasonNum = Convert.ToInt32(match.Groups[2].Value);
                        string seasonName = $"Season {seasonNum}";

                        int episodeCount = 0;

                        // This variable will track the actual deep folder where files are found!
                        string actualContentPath = path;

                        foreach (var ext in Extensions.VideoExtensions.List)
                        {
                            string searchPattern = $"*{ext.ToLower()}";

                            // Get the full path details of any matching video files inside
                            var foundFiles = Directory.EnumerateFiles(path, searchPattern, SearchOption.AllDirectories).ToList();

                            if (foundFiles.Any())
                            {
                                episodeCount += foundFiles.Count;

                                // Grab the directory name of the first video file found. 
                                // This is guaranteed to be the real folder containing the episodes!
                                actualContentPath = Path.GetDirectoryName(foundFiles.First())!;
                            }
                        }

                        string episodeCountString = $"{episodeCount} {(episodeCount == 1 ? "episode" : "episodes")}";

                        seasons.Add(new PlaylistItem { PlaylistName = seasonName, PlaylistId = actualContentPath, PlaylistCount = episodeCountString, SeasonNumber = seasonNum });


                    }
                }
                var seasonsRearranged = seasons.OrderBy(p => p.SeasonNumber).ToList();
                foreach (var item in seasonsRearranged)
                {
                    selbarSeasons.Items.Add(new SelectorBarItem
                    {
                        Text = item.PlaylistName,
                        Tag = item.PlaylistId   // Use the property from 'item'
                    });
                }
                if (selbarSeasons.Items.Count != 0)
                {
                    selbarSeasons.SelectedItem = selbarSeasons.Items[ShowManager.currentseason];

                }
                else
                {
                    grdNoEpisodes.Visibility = Visibility.Visible;
                    txtNoEpisodes.Text = "No seasons available!";
                }
            }
        }

        private void MainGrid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Space)
            {
                // 1. Perform your custom logic here
                System.Diagnostics.Debug.WriteLine("Spacebar intercepted!");
                PlayerService.PlayPause();
                // 2. Mark the event as handled to stop it from bubbling up to other controls
                e.Handled = true;
            }
            else if (e.Key == Windows.System.VirtualKey.Left)
            {
                PlayerService.SeekBefore();
            }
            else if (e.Key == Windows.System.VirtualKey.Right)
            {
                PlayerService.SeekAhead();
            }
            else if (e.Key == Windows.System.VirtualKey.F)
            {
                mediacontroller.IsFullScreen = !mediacontroller.IsFullScreen;
                FullScreen.FullScreenToggle();
            }
        }
        ObservableCollection<EpisodeModel> EpisodesList = new();

        private void btnCloseShowInfo_Click(object sender, RoutedEventArgs e)
        {
            grdShowInfo.Opacity = 0;
        }

        private async void selbarSeasons_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
        {
            if (sender is SelectorBar && sender.SelectedItem.Tag.ToString() is string folderpath)
            {
                if (Directory.Exists(folderpath))
                {
                    txtSeasonEpisodeCount.Text = "Loading episodes...";
                    var videoExtensions = Extensions.VideoExtensions.List
    .Select(ext => ext.ToLower())
   .ToHashSet();
                    var episodePatterns = new List<string>
{
    // 1. Standard SxxExx or just Exx (Looks for 'E' or 'EP' optionally preceded by 'Sxx')
    @"(?i)(?:s\d+)?e(\d+)\b",

    // 2. Multi-episode format: E02-E03, E02E03, e02_03
    @"(?i)e(\d+)(?:[-_]?e?(\d+))?\b",

    // 3. Standard text 'episode' or 'ep' followed by numbers (e.g., Ep.01, Episode 1)
    @"(?i)\b(?:ep|episode)(?:\s*|\s*\.\s*)(\d+)\b",

    // 4. X / Cross format: S01x02, 1x02, 1x2
    @"(?i)\b\d+x(\d+)\b",

    // 5. Bracketed numbers (Anime style): [02], (02)
    @"\[(\d+)\]",
    @"\((\d+)\)",

    // 6. Absolute / Standalone numbers: "Show - 02.mp4" 
    @"(?<=\s+|-|_|#)(\d+)(?=\.\w+$|\s+|-|_)"
};



                    var videoFiles = Directory.EnumerateFiles(folderpath)
    .Where(file => videoExtensions.Contains(Path.GetExtension(file).ToLower()))
    .OrderBy(file => file)
    .ToList();

                    EpisodesList.Clear();



                    // 1. Pre-populate the list on the UI thread so items stay perfectly sorted
                    var episodePlaceholders = new List<EpisodeModel>();

                    foreach (var filePath in videoFiles)
                    {
                        string fileName = Path.GetFileName(filePath);
                        string episodeNumber = "Unknown";
                        Debug.WriteLine($"Processing File: {fileName}");

                        // 3. Evaluate each regex pattern
                        foreach (var pattern in episodePatterns)
                        {
                            Match match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);
                            if (match.Success)
                            {
                                var validGroups = match.Groups.Cast<Group>()
                                                              .Skip(1)
                                                              .Where(g => g.Success && !string.IsNullOrEmpty(g.Value))
                                                              .ToList();

                                Debug.WriteLine($"  [Check] Pattern '{pattern}' matched! Found {validGroups.Count} valid capture groups.");

                                if (validGroups.Any())
                                {
                                    episodeNumber = validGroups.First().Value;
                                    Debug.WriteLine($"  -> Match Found! Episode: {episodeNumber}");
                                    break;
                                }
                            }
                        }
                        var newEpisode = new EpisodeModel
                        {
                            EpisodeName = $"Episode {episodeNumber}",
                            Description = "Loading...",
                            Duration = "--:--:--",
                            FilePath = filePath,

                            CurrentShowDirectory = Path.GetDirectoryName(filePath)
                        };

                        EpisodesList.Add(newEpisode);
                        episodePlaceholders.Add(newEpisode);
                    }

                    // 2. Offload processing to a background thread
                    await Task.Run(async () =>
                    {
                        using var semaphore = new SemaphoreSlim(3);
                        var processingTasks = new List<Task>();

                        for (int i = 0; i < videoFiles.Count; i++)
                        {
                            var filePath = videoFiles[i];
                            var targetEpisodeModel = episodePlaceholders[i];

                            await semaphore.WaitAsync();
                            string description = "No description available";
                            using (var tagfile = TagLib.File.Create(filePath))
                            {
                                if (!string.IsNullOrEmpty(tagfile.Tag.Comment))
                                {
                                    description = tagfile.Tag.Comment;
                                }
                            }


                            var task = Task.Run(async () =>
                            {
                                try
                                {
                                    // A. HEAVY IO: Read TagLib (Safe for background thread)


                                    // B. WINRT CALL: Get Video Duration
                                    // Since StorageFile demands an STA thread, we hop back to the UI thread briefly
                                    string durationString = "--:--:--";
                                    var tcsDuration = new TaskCompletionSource<string>();
                                    var file = await StorageFile.GetFileFromPathAsync(filePath);
                                    var videoproperties = await file.Properties.GetVideoPropertiesAsync();
                                    tcsDuration.SetResult(videoproperties.Duration.ToString(@"hh\:mm\:ss"));

                                    try { durationString = await tcsDuration.Task; } catch { /* Fallback to default */ }
                                    // C. HEAVY IO: Run FFmpeg to extract the image (Safe for background thread)
                                    string tempFile = await FileThumbnailObtain.ExtractVidThumbnailBasic(filePath);

                                    // D. WINRT CALL: Convert the temp image file into a BitmapImage
                                    // BitmapImage MUST be created and assigned on the UI thread
                                    DispatcherQueue.TryEnqueue(async () =>
                                    {


                                        if (!string.IsNullOrEmpty(tempFile) && File.Exists(tempFile))
                                        {
                                            try
                                            {
                                                targetEpisodeModel.Description = description;
                                                targetEpisodeModel.Duration = durationString;
                                                var bitmap = new BitmapImage();
                                                using (var stream = File.OpenRead(tempFile))
                                                {
                                                    await bitmap.SetSourceAsync(stream.AsRandomAccessStream());
                                                }
                                                targetEpisodeModel.Thumbnail = bitmap;

                                                // Clean up the temp file after loading it into memory
                                                File.Delete(tempFile);
                                            }
                                            catch (Exception bitmapEx)
                                            {
                                                Debug.WriteLine($"Bitmap Load Error: {bitmapEx.Message}");
                                                targetEpisodeModel.Thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/default.png"));
                                            }
                                        }
                                        else
                                        {
                                            targetEpisodeModel.Thumbnail = new BitmapImage(new Uri("ms-appx:///Assets/default.png"));
                                        }
                                    });
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error processing {filePath}: {ex.Message}");
                                }
                                finally
                                {
                                    semaphore.Release();
                                }
                            });

                            processingTasks.Add(task);
                        }

                        await Task.WhenAll(processingTasks);
                        DispatcherQueue.TryEnqueue(() =>
                        {
                            txtSeasonEpisodeCount.Text = $"{videoFiles.Count} {(videoFiles.Count == 1 ? "episode" : "episodes")}";
                            if (EpisodesList.Count == 0)
                            {
                                lstViewEpisodes.Visibility = Visibility.Collapsed;
                                grdNoEpisodes.Visibility = Visibility.Visible;
                                txtNoEpisodes.Text = "No episodes in this season!";
                            }
                            else
                            {
                                lstViewEpisodes.Visibility = Visibility.Visible;
                                grdNoEpisodes.Visibility = Visibility.Collapsed;
                                lstViewEpisodes.ItemsSource = EpisodesList;
                            }


                        });
                    });
                }
            }

        }



        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            if (App.NavigationFrame != null)
            {
                if (PlayerService.InVideoPage == true)
                {
                    PlayerService.isEpisodeVid = isEpisodeVideo;
                    App.NavigationFrame.GoBack();
                    if (PlayerService.Masterplayer != null)
                    {
                        if (PlayerService.Masterplayer.IsPlaying)
                        {
                            PlayerService.Pause();
                        }
                    }
                    PlayerService.InVideoPage = false;
                }

            }
        }

        private void videoControls_FullScreenToggled(bool obj)
        {
            if (obj == true)
            {
                btnBack.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnBack.Visibility = Visibility.Visible;
            }
        }

        private void sldVol_ValueChanged(double obj)
        {
            PlayerService.VolumeChange(obj);
        }

        private void mnftOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            videoControls.OpenFileLocation();
        }

        private void mnftFileInfo_Click(object sender, RoutedEventArgs e)
        {
            videoControls.ShowFileInfo();
        }
        ObservableCollection<DeviceOutputShow> AudioDevices = new ObservableCollection<DeviceOutputShow>();

        private void videoControls_MultiDeviceOutput()
        {
            FadeInStoryboardMultipleOutput.Begin();
            multiOutputMixer.ItemsSource = AudioDevices;
            foreach (var device in Engine.Audio.Devices)
            {
                bool isDefault = (device.Name?.Contains("Default", StringComparison.OrdinalIgnoreCase) ?? false);
                if (!isDefault)
                {
                    var volume = PlayerService.GetVolumeOfDevice(device.Id);
                    AudioDevices.Add(new DeviceOutputShow { DeviceID = device.Id, DeviceName = device.Name ?? "Unknown Device", DeviceVolume = $"{volume * 100.0f}%", Volume = volume * 100.0f });
                }
            }
        }
        ObservableCollection<ChapterModel> vidchapters = new ObservableCollection<ChapterModel>();
        private void videoControls_VideoChapters()
        {
            FadeInStoryboardChapters.Begin();
            if (PlayerService.Masterplayer == null) return;
            lstViewVideoChapters.ItemsSource = vidchapters;
            int chapternumber = 1;
            foreach (var chapter in PlayerService.Masterplayer.Chapters)
            {
                string title = string.IsNullOrEmpty(chapter.Title) ? $"Chapter {chapternumber}" : chapter.Title;
                chapternumber++;
                var starttime = chapter.StartTime;
                var endtime = chapter.EndTime;

                double totalSecondsStart = TimeSpan.FromTicks(starttime).TotalSeconds;
                double totalSecondsEnd = TimeSpan.FromTicks(endtime).TotalSeconds;
                var timespanstart = TimeSpan.FromTicks(starttime);
                var timespanend = TimeSpan.FromTicks(endtime);
                string start = timespanstart.TotalHours >= 1
                    ? $"{(int)timespanstart.TotalHours:D2}:{timespanstart.Minutes:D2}:{timespanstart.Seconds:D2}"
                    : $"{timespanstart.Minutes:D2}:{timespanstart.Seconds:D2}";
                string end = timespanend.TotalHours >= 1
                   ? $"{(int)timespanend.TotalHours:D2}:{timespanend.Minutes:D2}:{timespanend.Seconds:D2}"
                   : $"{timespanend.Minutes:D2}:{timespanend.Seconds:D2}";
                vidchapters.Add(new ChapterModel { ChapterTitle = title, StartTimeStr = start, EndTimeStr = end, StartTime = starttime, EndTime = endtime });
            }
        }

        private void btnCloseChapters_Click(object sender, RoutedEventArgs e)
        {
            FadeOutStoryboardChapters.Begin();
        }

        private void lstViewVideoChapters_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ChapterModel chapter)
            {
                var starttime = chapter.StartTime;
                var targetTime = TimeSpan.FromTicks(starttime);
                if (targetTime < TimeSpan.Zero) targetTime = TimeSpan.Zero;
                if (PlayerService.Masterplayer != null)
                {
                    PlayerService.Masterplayer.SeekAccurate((int)targetTime.TotalMilliseconds);
                    var curTime = TimeSpan.FromTicks(starttime);
                    ShowInformation($"Jumped to {chapter.ChapterTitle} at {chapter.StartTimeStr}");
                    mediacontroller.CurrentPosition = curTime.TotalSeconds;
                }
            }
        }
        private IEnumerable<ChapterModel> GetFilteredResults(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return Enumerable.Empty<ChapterModel>();

            var rawQuery = query.Trim();
            var textQuery = rawQuery;

            textQuery = textQuery.Trim();

            return vidchapters.Where(s =>
            {
                bool textMatch = !string.IsNullOrEmpty(textQuery) && (
                    (s.ChapterTitle?.Contains(textQuery, StringComparison.OrdinalIgnoreCase) == true));
                return textMatch;
            }).OrderByDescending(s => s.ChapterTitle?.StartsWith(textQuery, StringComparison.OrdinalIgnoreCase) == true).ThenBy(s => s.ChapterTitle);
        }

        ObservableCollection<ChapterModel> searchresults = new ObservableCollection<ChapterModel>();
        private void asbSearchChapters_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (string.IsNullOrEmpty(sender.Text))
            {
                searchresults.Clear();
                grdNoSearchResultsChapter.Visibility = Visibility.Collapsed;
                lstViewVideoChapters.Visibility = Visibility.Visible;
                lstViewVideoChapters.ItemsSource = vidchapters;
                grdChapterHeaders.Visibility = Visibility.Visible;
                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var results = GetFilteredResults(sender.Text);

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);

                sender.ItemsSource = results.Any() ? null : new List<string> { "No matches found!" };
                lstViewVideoChapters.ItemsSource = searchresults;
            }
        }


        private void asbSearchChapters_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            var results = GetFilteredResults(sender.Text);

            if (results.Any())
            {
                grdNoSearchResultsChapter.Visibility = Visibility.Collapsed;
                lstViewVideoChapters.Visibility = Visibility.Visible;
                grdChapterHeaders.Visibility = Visibility.Visible;

                searchresults.Clear();
                foreach (var item in results) searchresults.Add(item);
            }
            else if (vidchapters.Count > 0)
            {
                lstViewVideoChapters.Visibility = Visibility.Collapsed;
                grdChapterHeaders.Visibility = Visibility.Collapsed;
                grdNoSearchResultsChapter.Visibility = Visibility.Visible;
            }
        }

        private void btnCloseStats_Click(object sender, RoutedEventArgs e)
        {
            FadeOutStoryboardStats.Stop();
        }
        private void videoControls_MediaStats()
        {
            var statsTimerRealTime = PlayerService.statsTimerRealTime;
            FadeInStoryboardStats.Begin();
            txtFramesDropped.Text = "";
            statsTimerRealTime = new DispatcherTimer();
            statsTimerRealTime.Interval = TimeSpan.FromMilliseconds(50);
            statsTimerRealTime.Tick += StatsTimerRealTime_Tick;
            statsTimerRealTime.Start();
        }

        private void PlayerService_LoggedMessage(string message, Logger.LogLevelType logLevel)
        {
            var Icon = logLevel switch
            {
                Logger.LogLevelType.Information => "ms-appx:///Assets/infoicon.png",
                Logger.LogLevelType.Warning => "ms-appx:///Assets/warning.png",
                Logger.LogLevelType.Error => "ms-appx:///Assets/error.png",
                Logger.LogLevelType.Success => "ms-appx:///Assets/success.png",
                _ => null
            };
            if (PlayerService.Masterplayer == null) return;



            LogEntries.Add(new LogEntry { Level = logLevel, Icon = Icon, Message = message + $" at {DateTime.Now.ToString("HH:mm:ss")}" });
        }

        int count = 0;
        public MediaProperties.AudioProperties.Device ViewModel { get; } = new();

        int decodedaudio = 0;
        private void StatsTimerRealTime_Tick(object? sender, object e)
        {
            if (PlayerService.Masterplayer == null) return;
            count++;
            PlayerService.Masterplayer.Config.Player.Stats = true;
            var config = PlayerService.Masterplayer.Config;

            var (displayedframes, droppedframes) = PlayerService.Masterplayer.GetRealTimeStats();
            var (delayedframes, failedframes) = PlayerService.Masterplayer.GetDelayedFailedFrames();
            txtFramesDisplayed.Text = displayedframes.ToString();
            txtFramesDropped.Text = droppedframes.ToString();
            txtFramesDelayed.Text = delayedframes.ToString();
            txtEncodedFPS.Text = PlayerService.Masterplayer.Video.FPS.ToString("###") + " fps";
            txtDecodedFrames.Text = PlayerService.Masterplayer.VideoDecoder.DecodedVideoFrames.ToString();

            int avDistanceMS = PlayerService.Masterplayer.GetVDist();
            txtAVOffset.Text = $"{avDistanceMS:+0.0;-0.0;0.0} ms";
            txtDecodedAudio.Text = PlayerService.Masterplayer.AudioDecoder.DecodedFrames.ToString();
            if (ViewModel.CurrentDevice != null)
            {
                txtDeviceOutput.Text = ViewModel.CurrentDevice.Name;
            }
      
            txtFramesDroppedAudio.Text = PlayerService.Masterplayer.Audio.FramesDropped.ToString();
            txtAudioPlayed.Text = PlayerService.Masterplayer.Audio.FramesDisplayed.ToString();
            var (bufferMs, delayMs) = PlayerService.Masterplayer.GetAudioDiagnostics();
            txtBufferDuration.Text = $"{bufferMs:0.0} ms";
            txtDeviceDelay.Text = $"{delayMs:0.0} ms";


        }

        private void btnCopyStatsVideo_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== Video Performance Telemetry (Real Time)===");
            sb.AppendLine($"Frames Decoded:   {txtDecodedFrames.Text}");
            sb.AppendLine($"Frames Displayed: {txtFramesDisplayed.Text}");
            sb.AppendLine($"Frames Dropped:   {txtFramesDropped.Text}");
            sb.AppendLine($"Frames Delayed:   {txtFramesDelayed.Text}");
            sb.AppendLine($"Encoded FPS:      {txtEncodedFPS.Text}");
            sb.AppendLine($"A/V Offset:       {txtAVOffset.Text}");

            var package = new DataPackage();
            package.SetText(sb.ToString());
            Clipboard.SetContent(package);
        }

        private void btnCopyStatsAudio_Click(object sender, RoutedEventArgs e)
        {

            var sb = new StringBuilder();
            sb.AppendLine("=== Audio Performance Telemetry (Real Time)===");
            sb.AppendLine($"Frames Decoded:   {txtDecodedAudio.Text}");
            sb.AppendLine($"Frames Played: {txtAudioPlayed.Text}");
            sb.AppendLine($"Frames Dropped:   {txtFramesDroppedAudio.Text}");
            sb.AppendLine($"Device Delay:      {txtDeviceDelay.Text}");
            sb.AppendLine($"Output Device:       {txtDeviceOutput.Text}");
            sb.AppendLine($"Buffered Duration:       {txtBufferDuration.Text}");

            var package = new DataPackage();
            package.SetText(sb.ToString());
            Clipboard.SetContent(package);
        }
    }
}

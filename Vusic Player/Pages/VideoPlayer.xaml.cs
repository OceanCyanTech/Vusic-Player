
using FlyleafLib;
using FlyleafLib.MediaPlayer;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Linq;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.FilePickers;
using Vusic_Player.MediaProperties.VideoProperties;
using Vusic_Player.UI;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Orientation = Vusic_Player.MediaProperties.VideoProperties.Orientation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.Pages
{

    public sealed partial class VideoPlayer : Page
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        bool isPinned = false;
        DateTime recordStartTime;
        DispatcherTimer? SubtitleTimer;
        DispatcherTimer? RecordTimer;
        DispatcherTimer? SaveTimer;
        bool LoadingProgress = false;
        public GlowService Glow => GlowService.Instance;
        public Orientation AngleRotate => Orientation.Instance;

        public VideoPlayer()
        {
            InitializeComponent();
        }
        public static string[] List = {
                ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm",
                ".ts", ".m2ts", ".mts", ".m4v", ".3gp", ".3g2", ".ogv",
                ".mpeg", ".mpg", ".vob", ".rmvb", ".asf", ".m2p"
            };
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
        private void ShowInformation(string information)
        {
            txtInformation.VerticalAlignment = VerticalAlignment.Top;
            txtInformation.HorizontalAlignment = HorizontalAlignment.Center;
            txtInformation.Margin = new Thickness(20, 30, 0, 0);
            txtInformation.Text = information;
            FadeInOutStoryboard.Begin();
        }
        private void Masterplayer_OpenCompleted(object? sender, OpenCompletedArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            FadeInOutStoryboardPanel.Begin();
            ShowInformation($"'{PlayerService.CurrentPlayingPath}' opened");
            //if (ContinuePlaying.videoProgressMain is VideoProgress vdprg && LoadingProgress == true)
            //{
            //    PlayerService.Masterplayer.SeekAccurate((int)(vdprg.CurrentDuration / 10000));

            //    var curTime = TimeSpan.FromTicks(PlayerService.Masterplayer.CurTime);
            //    mediacontroller.CurrentPosition = curTime.TotalSeconds;
            //    ShowInformation($"Opened playback of '{PlayerService.CurrentPlayingPath}' at {mediacontroller.RunningDurationString}");

            //}

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
                    item.CurrentDuration = PlayerService.Masterplayer.CurTime;
                    item.TotalDuration = PlayerService.Masterplayer.Duration;
                }
                else
                {
                    string path = PlayerService.CurrentPlayingPath;

                    settings.SavedVideoProgress.Add(new VideoProgress
                    {

                        FilePath = path,
                        CurrentDuration = PlayerService.Masterplayer.CurTime,
                        TotalDuration = PlayerService.Masterplayer.Duration
                    });
                }

                await SettingsLoader.SaveSettingsAsync(settings);
            };
            SaveTimer?.Start();
            //LoadSettings();
            //LoadOptions();

        }
        private void ShowPanel()
        {
            if (isPinned == false)
            {
                 FadeInOutStoryboardPanel.Begin();
            }
        }
        #region Context Menu Events

        private void mnftOpenVideo_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftVolume_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftPrevious_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftSkipBack_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftPlay_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftSkipForward_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftNext_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftFullScreen_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PlayerService.InVideoPage = true;
            btnNextEpisode.Visibility = Visibility.Collapsed;

            string VideoPath = "";
            if (e.Parameter is VideoProgress vditem && vditem.FilePath is string path)
            {
                VideoPath = path;
                LoadingProgress = true;
            }
            else if (e.Parameter is string Path)
            {
                VideoPath = Path;
                LoadingProgress = false;

            }
           // else if (e.Parameter is EpisodeModel episode)
           // {
           //     btnNextEpisode.Visibility = Visibility.Visible;
           ////     videoControls.ViewEpisodeVisibility = Visibility.Visible;
           // }
            PlayerService.OpenPath(VideoPath);

            if (PlayerService.Masterplayer != null)
            {
                // Convert ticks to milliseconds and force an accurate seek
                SplashGrid.Visibility = Visibility.Collapsed;
                MainGrid.Visibility = Visibility.Visible;
                hostMedia.Player = PlayerService.Masterplayer;
                PlayerService.Masterplayer.OpenCompleted += Masterplayer_OpenCompleted;

            }
            else
            {
                txtSplash.Text = "An unexpected error occured! Check log page under App Settings for more details";
         //       Logger.Log("[ERR-PLY-001] Media player instance is null. Unable to initialize playback.", "VideoPlayerPage.OpenVideo", Logger.LogLevelType.Error);
            }
            base.OnNavigatedTo(e);
        }

        private void mnftSubtitleOptions_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftVideoOptions_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftAudioOptions_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftHome_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        #endregion
        private void btnCloseInformation_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnPin_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnStopRecording_Click(object sender, RoutedEventArgs e)
        {
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            //  this.Frame.Navigate(typeof(Pages.SettingsPage));
        }
        private void btnNextEpisode_Click(object sender, RoutedEventArgs e)
        {
            //   QueueService.PlayNext();
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

        }
    }
}

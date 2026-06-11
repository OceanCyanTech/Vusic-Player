using FlyleafLib;
using FlyleafLib.MediaFramework.MediaStream;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.SubtitlesProperties.ExternalSubtitles;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Helper.VideoProperties;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.FilePickers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.FileProperties;
using WinRT.Interop;
using UserControl = Microsoft.UI.Xaml.Controls.UserControl;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class VideoControls : UserControl
    {
        public event Action? ViewEpisodeClick;
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        public Visibility ViewEpisodeVisibility
        {
            get => (Visibility)GetValue(viewepisodevis);
            set => SetValue(viewepisodevis, value);
        }
        public static readonly DependencyProperty viewepisodevis =
    DependencyProperty.Register(
        nameof(ViewEpisodeVisibility),
        typeof(Visibility),
        typeof(VideoControls),
        new PropertyMetadata(Visibility.Collapsed));
        public VideoControls()
        {
            InitializeComponent();
        }

        private async void btnOpenVid_Click(object sender, RoutedEventArgs e)
        {
         //   if (App.MainWindowInstance != null)
              
        }

        private void sldVol_ValueChanged(double obj)
        {
            PlayerService.VolumeChange(obj);
        }

        private void btnFullScreen_Checked(object sender, RoutedEventArgs e)
        {
            FullScreenToggle();
            mnftFullScreen.IsChecked = btnFullScreen.IsChecked ?? false;

        }
        private Microsoft.UI.Windowing.AppWindow? _appWindow;
        private AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(App.MainWindowInstance);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }
        public event Action<bool>? FullScreenToggled;
        private void FullScreenToggle()
        {
            _appWindow ??= GetAppWindowForCurrentWindow();
            var targetPresenter = btnFullScreen.IsChecked == true ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default;
            var toolTipText = btnFullScreen.IsChecked == true ? "Resize to Normal Window" : "Set Full Screen";
            _appWindow.SetPresenter(targetPresenter);
            FullScreenToggled?.Invoke(btnFullScreen.IsChecked == true);
            string fullscreentext = btnFullScreen.IsChecked == false ? "Resized to Normal" : " Full Screen";
            GeneralInfoService.ShowInfo(fullscreentext);
            ToolTipService.SetToolTip(btnFullScreen, toolTipText);
        }


        private void btnFullScreen_Unchecked(object sender, RoutedEventArgs e)
        {
            FullScreenToggle();
            mnftFullScreen.IsChecked = btnFullScreen.IsChecked ?? false;
        }
        #region Subtitle Menu Events

        private void SubtitleMenu_Opened(object sender, object e)
        {
            if (PlayerService.Masterplayer == null) return;
            for (int i = submnExistingTracks.Items.Count - 1; i >= 0; i--)
            {
                var item = submnExistingTracks.Items[i];


                if (item is MenuFlyoutItem mfi)
                {
                    string text = mfi.Text;

                    if (text != "Search" &&
                        text != "Search Online..." &&
                        text != "None (Disable)" && text != "External")
                    {
                        submnExistingTracks.Items.RemoveAt(i);
                    }
                }
            }
            foreach (var stream in PlayerService.Masterplayer.Subtitles.Streams)
            {
                RadioMenuFlyoutItem streamitem = new();
                streamitem.Text = $"Subtitle {stream.StreamIndex} [{stream.Language}]";
                FontIcon icon = new();
                streamitem.Tag = stream;
                icon.Glyph = "\uE7F0";
                streamitem.Icon = icon;
                streamitem.Click += SubtitleStreamitem_Click; ;
                submnExistingTracks.Items.Add(streamitem);
                if (stream.Enabled == true)
                {
                    streamitem.IsChecked = true;
                }
            }

            for (int i = mnftLoadSub.Items.Count - 1; i >= 0; i--)
            {
                var item = mnftLoadSub.Items[i];


                if (item is MenuFlyoutItem mfi)
                {
                    string text = mfi.Text;

                    if (text != "Load External File"
                     )
                    {
                        mnftLoadSub.Items.RemoveAt(i);
                    }
                }
            }
            foreach (var loaded in External.ExternalSubtitles)
            {
                RadioMenuFlyoutItem radioMenuFlyoutItem = new();
                radioMenuFlyoutItem.Text = loaded.Name;
                radioMenuFlyoutItem.Tag = loaded.Path;
                radioMenuFlyoutItem.Click += (object sender, RoutedEventArgs e) =>
                {
                    PlayerService.Masterplayer.Open(loaded.Path);
                    GeneralInfoService.ShowInfo($"Subtitles set to external path {loaded.Path}");
                };
                mnftLoadSub.Items.Add(radioMenuFlyoutItem);
            }
        }
        private void SubtitleStreamitem_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;

            if (sender is RadioMenuFlyoutItem selectedItem)
            {
                if (selectedItem.Tag is SubtitlesStream stream)
                {
                    Configuration.Helper.SubtitlesProperties.Stream.Set(stream);
                }
            }

        }

        private async void mnftLoadSub_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) return;
            var file = await SubtitlePicker.PickSingle(App.MainWindowInstance, "Choose subtitle file");

            if (file != null)
            {
                Configuration.Helper.SubtitlesProperties.Stream.ExternalSubtitlePath = file.Path;
                Configuration.Helper.SubtitlesProperties.Stream.PathExternal();
                if (PlayerService.Masterplayer != null)
                {
                    PlayerService.Masterplayer.Open(file.Path);
                    string name = Path.GetFileName(file.Path);
                    var externalalready = External.ExternalSubtitles.FirstOrDefault(p => p.Path == file.Path);
                    if (externalalready == null)
                    {
                        External.ExternalSubtitles.Add(new ExternalModel { Path = file.Path, Name = name });
                    }
                    for (int i = mnftLoadSub.Items.Count - 1; i >= 0; i--)
                    {
                        var item = mnftLoadSub.Items[i];


                        if (item is MenuFlyoutItem mfi)
                        {
                            string text = mfi.Text;

                            if (text != "Load External File"
                             )
                            {
                                mnftLoadSub.Items.RemoveAt(i);
                            }
                        }
                    }
                    foreach (var loaded in External.ExternalSubtitles)
                    {
                        RadioMenuFlyoutItem radioMenuFlyoutItem = new();
                        radioMenuFlyoutItem.Text = loaded.Name;
                        radioMenuFlyoutItem.Tag = loaded.Path;
                        radioMenuFlyoutItem.Click += (object sender, RoutedEventArgs e) =>
                        {
                            PlayerService.Masterplayer.Open(loaded.Path);
                            GeneralInfoService.ShowInfo($"Subtitles set to external path {loaded.Path}");
                        };
                        mnftLoadSub.Items.Add(radioMenuFlyoutItem);
                    }
                    Configuration.Helper.SubtitlesProperties.Stream.ExternalSubtitleAdded();
                    GeneralInfoService.ShowInfo($"Subtitles set to external path {file.Path}");

                }
            }
        }

        private void mnftSubtitleEditor_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftCustomizeSub_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
            //var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 2, 1, 15);
        }

        private void mnftSearchSubtitles_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
            //var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 2, 0, 14);

        }

        private void mnftSearchSubtitlesOnline_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void mnftDisableSubs_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Configuration.Helper.SubtitlesProperties.Stream.Disable(mnftDisableSubs.IsChecked);
        }

        private void mnftSubDelayOption_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Use (sender as RadioMenuFlyoutItem).Tag to get the delay value
        }

        private void mnftCustomSubDelay_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
        //    var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 2, 0, 17);

        }

        private void SubtitleStyle_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioMenuFlyoutItem { Tag: string tagValue })
            {
                var parts = tagValue.Split(':');
                if (parts.Length != 2) return;

                string styleKey = parts[0];
                string fontSizeStr = parts[1];

                if (App.Current.Resources.TryGetValue(styleKey, out object resource))
                {
                    if (resource is Style selectedStyle)
                    {
//SubtitlesProperties.Customize.style = selectedStyle;
                    }
                }

                if (double.TryParse(fontSizeStr, out double size))
                {
            //        SubtitlesProperties.Customize.FontSize = size;
                }
        //        SubtitlesProperties.Customize.Call();
            }
        }

        #endregion
        #region Video Menu Events

        private void VideoMenu_Opened(object sender, object e)
        {
            if (PlayerService.Masterplayer == null) return;
            for (int i = submnVideoTracks.Items.Count - 1; i >= 0; i--)
            {
                var item = submnVideoTracks.Items[i];


                if (item is MenuFlyoutItem mfi)
                {
                    string text = mfi.Text;

                    if (text != "Search" &&
                        text != "View Detailed" &&
                        text != "Disable" && text != "External")
                    {
                        submnVideoTracks.Items.RemoveAt(i);
                    }
                }
            }
            foreach (var stream in PlayerService.Masterplayer.Video.Streams)
            {
                RadioMenuFlyoutItem streamitem = new();
                streamitem.Text = stream.Title;
                streamitem.Text = $"Stream {stream.StreamIndex} [{stream.Language}] ({stream.Width}x{stream.Height})";
                FontIcon icon = new();
                streamitem.Tag = stream;
                icon.Glyph = "\uE8B2";
                streamitem.Icon = icon;
                streamitem.Click += VideoStreamitem_Click;
                submnVideoTracks.Items.Add(streamitem);
                if (stream.Enabled == true)
                {
                    streamitem.IsChecked = true;
                }
            }
        }
        private void VideoStreamitem_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;

            if (sender is RadioMenuFlyoutItem selectedItem)
            {
                var stream = selectedItem.Tag as FlyleafLib.MediaFramework.MediaStream.VideoStream;

                if (stream != null)
                {
                    PlayerService.Masterplayer.Config.Video.Enabled = true;
                    // Open the selected audio stream
                    PlayerService.Masterplayer.Open(stream);
                }
                else
                {
                    PlayerService.Masterplayer.Config.Video.Enabled = false;
                }
            }
        }
        private void mnftFullScreen_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            btnFullScreen.IsChecked = mnftFullScreen.IsChecked;
            FullScreenToggle();
        }

        private void mnftTakeSnapshot_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Screen.TakeSnapshot(Screen.SnapshotDirect, Screen.IsTimeStampIncluded, Screen.IsPositionIncluded);
        }


        private void mnftRecord_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer != null)
            {
                if (PlayerService.Masterplayer.IsRecording)
                {
                   Screen.StopRecordRequest();
                }
                else
                {
                    Screen.Record(Screen.RecordDirect);
                }
            }
        }

        private void mnftSpeed_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioMenuFlyoutItem selectedItem)
            {
                string tagValue = selectedItem.Tag.ToString() ?? "1.0";

                if (double.TryParse(tagValue, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double speed))
                {
     //             SpeedService.Set(speed);
                }
            }
        }

        private void mnftCustomSpeed_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
    //        var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 0, 0, 3);

        }

        private void mnftReversePlayback_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
    //        ReverseService.Reverse(mnftReversePlayback.IsChecked);
        }

        private void mnftZoomItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioMenuFlyoutItem { Tag: string tagValue })
            {
                if (PlayerService.Masterplayer == null) return;

                switch (tagValue)
                {
                    case "fit":

                        PlayerService.Masterplayer.Config.Video.AspectRatio = AspectRatio.Keep;
                        PlayerService.Masterplayer.Config.Video.Zoom = 100; // Reset digital zoom
                        break;

                    case "fill":

                        PlayerService.Masterplayer.Config.Video.AspectRatio = AspectRatio.Fill;
                        PlayerService.Masterplayer.Config.Video.Zoom = 100; // Reset digital zoom
                        break;

                    default:
                        if (double.TryParse(tagValue, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out double zoomPercent))
                        {

                            PlayerService.Masterplayer.Config.Video.Zoom = zoomPercent;
                            PlayerService.Masterplayer.Config.Video.AspectRatio = AspectRatio.Keep;
                        }
                        break;
                }
               GeneralInfoService.ShowInfo($"{tagValue}% zoom");
            }
        }

        private void mnftCustomZoom_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

            if (App.MainWindowInstance == null) { return; }
          //  var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 0, 0, 1);

        }

        private void mnftAspectRatioItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioMenuFlyoutItem { Text: string selectedText })
            {
                if (selectedText == "Default")
                {
        //            Aspect.SetDefault();
                }
                else
                {
//Aspect.SetAspectRatio(selectedText);
                }
            }
        }

        private void mnftCustomAspectRatio_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
          //  var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 0, 3, 8);

        }

        private void mnftSearchVideoTracks_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
     //       var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 0, 0, 0);
        }

        private void mnftDisableVideoTracks_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
         //   VideoProperties.Stream.Disable(mnftDisableVideoTracks.IsChecked);

        }

        private void mnftExternalVideoTracks_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            //
        }

        private void mnftSnapshotDirectory_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
      //      var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 0, 0, 2);
        }

        private void mnftVideoOptions_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
  //          var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 0, 0, 0);
        }
        private void mnftRecordDirectory_Click(object sender, RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
     //       var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 0, 0, 4);
        }

        #endregion
        #region Audio Menu Events

        private void AudioMenu_Opened(object sender, object e)
        {
            LoadAudioStreams();
            mnftAudioDevice.Items.Clear();
            foreach (var dev in Engine.Audio.Devices)
            {
                RadioMenuFlyoutItem devitem = new();
                devitem.Text = dev.Name;
                FontIcon icon = new();
                icon.Glyph = "\uE7F5";
                devitem.Icon = icon;
                devitem.Tag = dev;
                if (dev.Id == Engine.Audio.CurrentDevice.Id)
                {
                    devitem.IsChecked = true;
                }
                devitem.Click += (object sender, RoutedEventArgs e) =>
                {
                    Engine.Audio.SetDevice(dev.Id);
                };
                mnftAudioDevice.Items.Add(devitem);
            }
        }
        private void LoadAudioStreams()
        {
            if (PlayerService.Masterplayer == null) return;

            for (int i = submnAudioTracks.Items.Count - 1; i >= 0; i--)
            {
                var item = submnAudioTracks.Items[i];


                if (item is MenuFlyoutItem mfi)
                {
                    string text = mfi.Text;

                    if (text != "Search" &&
                        text != "View Detailed" &&
                        text != "Disable" && text != "External")
                    {
                        submnAudioTracks.Items.RemoveAt(i);
                    }
                }
            }
            foreach (var stream in PlayerService.Masterplayer.Audio.Streams)
            {
                RadioMenuFlyoutItem streamitem = new();
                streamitem.Text = $"Stream {stream.StreamIndex} [{stream.Language}]";
                FontIcon icon = new();
                icon.Glyph = "\uEC4F";
                streamitem.Tag = stream;
                streamitem.Icon = icon;
                streamitem.Click += AudioStreamitem_Click;
                if (stream.Enabled == true)
                {
                    streamitem.IsChecked = true;
                }
                submnAudioTracks.Items.Add(streamitem);

            }

        }
        private void AudioStreamitem_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;

            if (sender is RadioMenuFlyoutItem selectedItem)
            {
                if (selectedItem.Tag is AudioStream stream)
                {
         //           AudioProperties.Stream.Set(stream);
                }

            }
        }

        private void mnftVolumeItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioMenuFlyoutItem { Tag: string tagValue })
            {
                if (PlayerService.Masterplayer == null) return;

                if (double.TryParse(tagValue, out double volumeLevel))
                {
                    PlayerService.VolumeChange(volumeLevel);
                }
            }
        }

        private void mnftCustomVolume_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
      //      var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 1, 0, 16);

        }

        private void mnftSearchAudioTracks_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
       //     var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 1, 0, 10);

        }

        private void mnftDisableAudioTracks_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            //AudioProperties.Stream.Disable(mnftDisableAudioTracks.IsChecked);
        }

        private void mnftExternalAudioTracks_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void mnftAudioDelay_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is MenuFlyoutItem { Tag: string tagValue })
            {
                if (PlayerService.Masterplayer == null) return;

                if (tagValue == "reset")
                {
            //        Delay.Reset();
                    return;
                }

        //        Delay.Apply(tagValue);
            }
        }

        private void mnftCustomAudioDelay_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
    //        var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 1, 0, 12);
        }

        private void mnftPitchItem_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is RadioMenuFlyoutItem selectedItem)
            {
                string tagValue = selectedItem.Tag.ToString() ?? "1.0";

                if (double.TryParse(tagValue, System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture, out double pitch))
                {
                //    Pitch.Apply(pitch);
                }
            }
        }

        private void mnftCustomAudioPitch_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
         //   var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 1, 0, 9);
        }

        private void mnftAudioOptions_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (App.MainWindowInstance == null) { return; }
      //      var dlg = VideoOptionsWindow.ShowWindow(VideoOptionsWindow.OptionType.VideoOptions, VideoOptionsWindow.Video.AspectRatio, 1, 0, 10);
        }

        #endregion
        #region More Options Menu Events

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

        private void mnftMiniPlayer_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private void mnftGlowEffect_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Check state using: (sender as ToggleMenuFlyoutItem).IsChecked
        }


        #endregion

        private void mnftViewEpisodes_Click(object sender, RoutedEventArgs e)
        {
            ViewEpisodeClick?.Invoke();
        }
    }
}

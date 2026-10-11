using Microsoft.UI;
using Microsoft.UI.Windowing;
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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.Helper.AudioProperties;
using Vusic_Player.Configuration.Helper.FileSystem;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.Extensions;
using Vusic_Player.FilePickers;
using Vusic_Player.UI.Dialogs;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Vusic_Player.UI.UserViews.Controls;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Streams;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class PictureInPicture : Window
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        private IntPtr _hwnd;
        private WinUser.SubclassProc? _subclassProc;
        private SystemMediaTransportControls? _smtc;
        public PictureInPicture()
        {
            InitializeComponent();

            int width = 550;
            int height = 300;
            if (!File.Exists(PlayerService.CurrentPlayingPath)) return;
            string fileExtension = Path.GetExtension(PlayerService.CurrentPlayingPath ?? "").ToLower();
            if (Extensions.AudioExtensions.List.Contains(fileExtension))
            {
                width = 300;
                imgCover.Visibility = Visibility.Visible;
                hostMedia.Visibility = Visibility.Collapsed;
                txtTitle.Visibility = Visibility.Visible;
            }
            else if (Extensions.VideoExtensions.List.Contains(fileExtension))
            {
                txtTitle.Visibility = Visibility.Collapsed;

                if (PlayerService.Masterplayer != null)
                {
                    hostMedia.Player = PlayerService.Masterplayer;
                }
            }
            SetupSMTC();
            mediacontroller.IsPIP = true;
            this.SetTitleBar(CustomTitleBar);
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            _hwnd = WindowNative.GetWindowHandle(this);

            // 2. Install a subclass callback to intercept window messages
            _subclassProc = new WinUser.SubclassProc(WindowSubclassCallback);
            WinUser.SetWindowSubclass(_hwnd, _subclassProc, 0, IntPtr.Zero);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(myWndId);
            appWindow.IsShownInSwitchers = false;
            appWindow.Resize(new Windows.Graphics.SizeInt32(width, height));
            var displayArea = Microsoft.UI.Windowing.DisplayArea.GetFromWindowId(myWndId, Microsoft.UI.Windowing.DisplayAreaFallback.Primary);

            int margin = 30;

            // X: Screen Width - Window Width - Margin (Correct)
            int x = displayArea.WorkArea.Width - width - margin;
            int y = displayArea.WorkArea.Y + margin;

            appWindow.Move(new Windows.Graphics.PointInt32(x, y));
            ExtendsContentIntoTitleBar = true;
            Title = "Vusic Player Mini View";

            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.IsAlwaysOnTop = true;

                // Optional: Remove buttons to make it look like a true PiP overlay
                presenter.IsResizable = true;
                presenter.IsMaximizable = false;

                presenter.IsMinimizable = false;
                presenter.SetBorderAndTitleBar(true, false);
            }
            appWindow.Changed += (sender, args) =>
            {
                if (args.DidSizeChange)
                {
                    int maxWidth = 800;
                    int maxHeight = 600;
                    bool needsResize = false;

                    int newWidth = sender.Size.Width;
                    int newHeight = sender.Size.Height;

                    if (newWidth > maxWidth) { newWidth = maxWidth; needsResize = true; }
                    if (newHeight > maxHeight) { newHeight = maxHeight; needsResize = true; }

                    if (needsResize)
                    {
                        sender.Resize(new Windows.Graphics.SizeInt32(newWidth, newHeight));
                    }
                }
            };

            InitializeSave();
            this.Closed += PictureInPicture_Closed;
        }
        DispatcherTimer? SaveTimer;

        private async void InitializeSave()
        {
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
            var settings = await SettingsLoader.LoadSettingsAsync();
            if (settings.IsVideoHistoryDisabled == false)
            {
                SaveTimer?.Start();
            }
        }
        private void PictureInPicture_Closed(object sender, WindowEventArgs args)
        {
            mediacontroller.IsPIP = false;

        }

        [System.Runtime.InteropServices.ComImport]
        [System.Runtime.InteropServices.Guid("12604699-522A-45AD-8C80-6078674914F5")]
        [System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIInspectable)]
        public interface ISystemMediaTransportControlsInterop
        {
            IntPtr GetForWindow(IntPtr appWindow, [System.Runtime.InteropServices.In] ref Guid riid);
        }
        public async void SetupSMTC()
        {
            // Get the controls for the current view
            _smtc = BackgroundMediaPlayer.Current.SystemMediaTransportControls;

            // 2. Get the window handle (HWND) for your current window

            // Enable the buttons you want to support
            _smtc.IsPlayEnabled = true;
            _smtc.IsPauseEnabled = true;
            _smtc.IsNextEnabled = true;
            _smtc.IsPreviousEnabled = true;

            var updater = _smtc.DisplayUpdater;
            _smtc.PlaybackStatus = MediaPlaybackStatus.Playing;
            updater.Type = MediaPlaybackType.Video;

            updater.VideoProperties.Title = "Vusic Player";
            if (File.Exists(PlayerService.CurrentPlayingPath))
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(PlayerService.CurrentPlayingPath);

                // 2. Create the Stream Reference the SMTC expects
                updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(file);
                updater.VideoProperties.Subtitle = Path.GetFileName(PlayerService.CurrentPlayingPath);
                updater.Update();
                // Hook up the event handler for button presses
                _smtc.ButtonPressed += SystemControls_ButtonPressed;
                PlayerService.PlayPauseChanged += PlayerService_PlayPauseChanged;
            }
        }
        public async Task<BitmapImage> GetFileThumbnailAsync(string path)
        {
            // Define your fallback asset
            Uri fallbackUri = new Uri("ms-appx:///Assets/default.png");

            try
            {
                if (string.IsNullOrEmpty(path))
                    return new BitmapImage(fallbackUri);
                if (!File.Exists(path)) return new BitmapImage(fallbackUri); ;
                StorageFile file = await StorageFile.GetFileFromPathAsync(path);

                // Get thumbnail from the file's metadata
                using var thumbnail = await file.GetScaledImageAsThumbnailAsync(
                    ThumbnailMode.VideosView, // Better for video files
                    320,
                    ThumbnailOptions.UseCurrentScale);

                if (thumbnail != null)
                {
                    BitmapImage bitmapImage = new BitmapImage();
                    // This connects the stream to the UI object
                    await bitmapImage.SetSourceAsync(thumbnail);
                    return bitmapImage;
                }
            }
            catch
            {
            }

            // If everything fails, return the app icon
            return new BitmapImage(fallbackUri);
        }

        private void PlayerService_PlayPauseChanged()
        {
            if (PlayerService.Masterplayer == null) return;
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

        private const int WM_NCLBUTTONDBLCLK = 0x00A3; // The "Non-Client Left Button Double Click" message
        private const int HTCAPTION = 2;
        private IntPtr WindowSubclassCallback(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData)
        {
            // WM_GETMINMAXINFO message code is 0x0024
            if (msg == 0x0024)
            {
                // Marshal the pointer to the MINMAXINFO structure
                WinUser.MINMAXINFO mmi = Marshal.PtrToStructure<WinUser.MINMAXINFO>(lParam);

                // Set your minimum dimensions (in pixels)
                mmi.ptMinTrackSize.X = 400; // Minimum Width
                mmi.ptMinTrackSize.Y = 200; // Minimum Height

                Marshal.StructureToPtr(mmi, lParam, false);
            }
            else if (msg == WM_NCLBUTTONDBLCLK && wParam.ToInt32() == HTCAPTION)
            {
                // Return 0 to indicate we've "handled" the message, preventing the maximize action
                return IntPtr.Zero;
            }

            return WinUser.DefSubclassProc(hWnd, msg, wParam, lParam);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            App.MainWindowInstance?.Close();
            OceanContentDialog.KillDlg();
            VideoOptionsWindow.KillDialog();
            SubtitleEditorWindow.KillDialog();
            this.Close();

        }

        private void sldMain_DragStarted()
        {
            PlayerService.SldMain_DragStarted();
        }

        private void sldMain_DragCompleted()
        {
            PlayerService.SldMain_DragCompleted(sldMain);
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            FadeInOutStoryboard.Begin();
        }

        private void Grid_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            FadeInOutStoryboard.Begin();
        }

        private void btnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            if (PlayerService.Masterplayer.IsPlaying)
            {
                PlayerService.Pause();
                ToolTipService.SetToolTip(btnPlayPause, "Play");
            }
            else
            {
                PlayerService.Play();
                ToolTipService.SetToolTip(btnPlayPause, "Pause");
            }
        }

        private void btnRestore_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.ShowWindow();
            PlayerService.PIPRestoreAction();
            this.Close();
        }
        private void sldVol_ValueChanged(double obj)
        {
            PlayerService.VolumeChange(obj);
        }
        private async void mnftOpenVideo_Click(object sender, RoutedEventArgs e)
        {
            var media = await MediaPicker.PickSingle(this, "Open Media");
            if (media != null)
            {
                bool isAudio = AudioExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                bool isVideo = VideoExtensions.List.Contains(media.FileType, StringComparer.OrdinalIgnoreCase);
                if (isAudio)
                {
                    imgCover.Visibility = Visibility.Visible;
                    hostMedia.Visibility = Visibility.Collapsed;
                    txtTitle.Visibility = Visibility.Visible;
                    if (File.Exists(media.Path))
                    {
                        if (App.NavigationFrame != null)
                        {
                            


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
                    imgCover.Visibility = Visibility.Collapsed;
                    hostMedia.Visibility = Visibility.Visible;
                    txtTitle.Visibility = Visibility.Collapsed;

                    if (PlayerService.Masterplayer != null)
                    {
                        hostMedia.Player = PlayerService.Masterplayer;
                    }
                }
                else
                {
                    //Handle other cases
                }
            }
        }
        private void mnftFileInfo_Click(object sender, RoutedEventArgs e)
        {
            ShowFileInfo();   
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
        private void mnftOpenFileLocation_Click(object sender, RoutedEventArgs e)
        {
            OpenFileLocation.PathSelect(PlayerService.CurrentPlayingPath);
        }

        private void mnftSkipForward_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            PlayerService.SeekAhead();
        }

        private void mnftNext_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            QueueService.PlayNext();
        }

        public void ShowFileInfo()
        {
            if (App.MainWindowInstance is MainWindow wind)
            {
                if (PlayerService.CurrentPlayingPath != null)
                {
                    wind.ShowFileInfo(PlayerService.CurrentPlayingPath);
                }
            }
        }
        private void mnftVolume_Click(object sender, RoutedEventArgs e)
        {
            ttVolume.IsOpen = true;
        }
    }
}

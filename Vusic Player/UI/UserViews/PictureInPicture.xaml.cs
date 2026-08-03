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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.Playback;
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
        private WinUser.SubclassProc _subclassProc;
        private SystemMediaTransportControls? _smtc;
        public PictureInPicture()
        {
            InitializeComponent();

            int width = 550;
            int height = 300;
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
            StorageFile file = await StorageFile.GetFileFromPathAsync(PlayerService.CurrentPlayingPath);

            // 2. Create the Stream Reference the SMTC expects
            updater.Thumbnail = RandomAccessStreamReference.CreateFromFile(file);
            updater.VideoProperties.Subtitle = Path.GetFileName(PlayerService.CurrentPlayingPath);
            updater.Update();
            // Hook up the event handler for button presses
            _smtc.ButtonPressed += SystemControls_ButtonPressed;
            PlayerService.PlayPauseChanged += PlayerService_PlayPauseChanged;
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
    }
}

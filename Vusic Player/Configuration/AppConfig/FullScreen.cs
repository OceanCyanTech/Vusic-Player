using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;
using WinRT.Interop;

namespace Vusic_Player.Configuration.AppConfig
{
    public class FullScreen
    {
        private static Microsoft.UI.Windowing.AppWindow? _appWindow;
        private static AppWindow GetAppWindowForCurrentWindow()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(App.MainWindowInstance);
            WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            return AppWindow.GetFromWindowId(myWndId);
        }
        public event Action<bool>? FullScreenToggled;
        public static MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        public static void FullScreenToggle()
        {
            _appWindow ??= GetAppWindowForCurrentWindow();
            var targetPresenter = mediacontroller.IsFullScreen == true ? AppWindowPresenterKind.FullScreen : AppWindowPresenterKind.Default;
            var toolTipText = mediacontroller.IsFullScreen == true ? "Resize to Normal Window" : "Set Full Screen";

            _appWindow.SetPresenter(targetPresenter);
            string fullscreentext = mediacontroller.IsFullScreen == false ? "Resized to Normal" : " Full Screen";
            GeneralInfoService.ShowInfo(fullscreentext);
            mediacontroller.FullScreenToolTip = toolTipText;
        }
    }
}

using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration.Helper.UI;
using Windows.Foundation;
using Windows.Foundation.Collections;
using WinRT;
using WinRT.Interop;
using static Vusic_Player.Configuration.AppConfig.WinUser;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SubtitleEditorWindow : Window
    {
        private static SubtitleEditorWindow? _instance;
        public static AppWindow? _appWindow;

        Microsoft.UI.Composition.SystemBackdrops.DesktopAcrylicController? acrylicController;
        Microsoft.UI.Composition.SystemBackdrops.SystemBackdropConfiguration? configurationSource;

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        private void DisableCloseButton()
        {
            // 1. Get the HWND (Window Handle) for the WinUI 3 Window
            IntPtr hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            if (hWnd != IntPtr.Zero)
            {
                // 2. Get the System Menu handle for this window
                IntPtr hMenu = GetSystemMenu(hWnd, false);

                if (hMenu != IntPtr.Zero)
                {
                    // 3. Gray out and disable the Close (Alt+F4) option in the context menu
                    EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED | MF_DISABLED);
                }

                // 4. Force a redraw of the window title bar to update the UI
                int style = GetWindowLong(hWnd, GWL_STYLE);
                SetWindowLong(hWnd, GWL_STYLE, style & ~WS_SYSMENU);
                SetWindowLong(hWnd, GWL_STYLE, style | WS_SYSMENU);
            }
        }
        private void AppWindow_Closing(Microsoft.UI.Windowing.AppWindow sender, Microsoft.UI.Windowing.AppWindowClosingEventArgs args)
        {
            // This blocks Alt+F4, taskbar "Close Window", and any other close attempts
            args.Cancel = true;

            // Optional: Add custom logic here, like showing a teaching tip or dialog
            System.Diagnostics.Debug.WriteLine("Close attempt blocked!");
        }
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x00080000;
        private const uint SC_CLOSE = 0xF060;
        private const uint MF_BYCOMMAND = 0x00000000;
        private const uint MF_GRAYED = 0x00000001;
        private const uint MF_DISABLED = 0x00000002;
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        const int GWL_HWNDPARENT = -8;

        public SubtitleEditorWindow()
        {
            InitializeComponent();
            this.ExtendsContentIntoTitleBar = true;
            TrySetAcrylicBackdrop(true);
            DisableCloseButton();
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.IsShownInSwitchers = false;
            _appWindow = appWindow;
            appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            appWindow.SetPresenter(AppWindowPresenterKind.Overlapped);
            var parentHwnd = WindowNative.GetWindowHandle(App.MainWindowInstance);
            var dialogHwnd = WindowNative.GetWindowHandle(this);
            if (dialogHwnd != IntPtr.Zero && parentHwnd != IntPtr.Zero)
            {
                // Check if they are already parented to avoid redundant OS calls
                IntPtr currentParent = GetWindowLongPtr(dialogHwnd, GWL_HWNDPARENT);

                if (currentParent != parentHwnd)
                {
                    SetWindowLongPtr(dialogHwnd, GWL_HWNDPARENT, parentHwnd);
                }
            }
            this.SetTitleBar(DragRegion);
            _subclassProc = new SubclassProc(WindowSubclassCallback);
            ResizeWind(1300, 800);
            // 3. Set the subclass
            SetWindowSubclass(dialogHwnd, _subclassProc, 0, IntPtr.Zero);
            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(true, false);
                presenter.IsMaximizable = true;
                presenter.IsMinimizable = false;
                presenter.IsResizable = true;
            }
        }
        private void ResizeWind(int Width, int Height)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            if (hwnd != IntPtr.Zero) // Guard against closed windows
            {
                var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
                if (appWindow != null) // Guard against uninitialized windows
                {
                    appWindow.Resize(new Windows.Graphics.SizeInt32(Width, Height));
                }
            }


        }
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (configurationSource != null)
            {
                configurationSource.IsInputActive =
                    args.WindowActivationState != WindowActivationState.Deactivated;
            }

            // Reattach acrylic if needed
            if (acrylicController == null && DesktopAcrylicController.IsSupported())
            {
                TrySetAcrylicBackdrop(true);
            }
        }
        private void Window_Closed(object sender, WindowEventArgs args)
        {
            // Make sure any Mica/Acrylic controller is disposed
            if (acrylicController != null)
            {
                acrylicController.Dispose();
                acrylicController = null;
            }
            Activated -= Window_Activated;
            configurationSource = null;
        }
        private void Window_ThemeChanged(FrameworkElement sender, object args)
        {
            if (configurationSource != null)
            {
                SetConfigurationSourceTheme();
            }
        }
        private void SetConfigurationSourceTheme()

        {
            if (configurationSource == null) return;
            switch (((FrameworkElement)this.Content).ActualTheme)
            {
                case ElementTheme.Dark: configurationSource.Theme = SystemBackdropTheme.Dark; break;
                case ElementTheme.Light: configurationSource.Theme = SystemBackdropTheme.Light; break;
                case ElementTheme.Default: configurationSource.Theme = SystemBackdropTheme.Default; break;
            }
        }

        bool TrySetAcrylicBackdrop(bool useAcrylicThin)
        {
            if (DesktopAcrylicController.IsSupported())
            {
                DispatcherQueue.EnsureSystemDispatcherQueue();

                configurationSource = new SystemBackdropConfiguration();
                Activated += Window_Activated;


                Closed += Window_Closed;
                ((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;

                configurationSource.IsInputActive = true;
                SetConfigurationSourceTheme();

                acrylicController = new DesktopAcrylicController();
                acrylicController.Kind = useAcrylicThin ? DesktopAcrylicKind.Thin : DesktopAcrylicKind.Base;

                // Enable the system backdrop.

                acrylicController.AddSystemBackdropTarget(this.As<ICompositionSupportsSystemBackdrop>());
                acrylicController.SetSystemBackdropConfiguration(configurationSource);

                return true; // Succeeded.
            }

            return false; // Acrylic is not supported on this system.
        }

        private IntPtr WindowSubclassCallback(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData)
        {
            // Check if the message is a double click on the non-client area (title bar)
            if (uMsg == WM_NCLBUTTONDBLCLK && wParam.ToInt32() == HTCAPTION)
            {
                // Return 0 to indicate we've "handled" the message, preventing the maximize action
                return IntPtr.Zero;
            }

            // Pass all other messages to the default handler
            return DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }
        private const int WM_NCLBUTTONDBLCLK = 0x00A3;
        private const int HTCAPTION = 2;

        // Delegate for the subclass procedure
        private delegate IntPtr SubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        private static extern bool SetWindowSubclass(IntPtr hWnd, SubclassProc pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

        [DllImport("Comctl32.dll", SetLastError = true)]
        private static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

        private SubclassProc _subclassProc;

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            var presenter = this.AppWindow.Presenter as OverlappedPresenter;

            if (presenter != null)
            {
                if (presenter.State == OverlappedPresenterState.Maximized)
                {
                    // Restore to normal
                    presenter.Restore();
                    MaximizeIcon.Glyph = "\uE922"; // Square icon
                    ToolTipService.SetToolTip(MaximizeButton, "Maximize");
                }
                else
                {
                    // Maximize
                    presenter.Maximize();
                    ToolTipService.SetToolTip(MaximizeButton, "Restore");

                    MaximizeIcon.Glyph = "\uE923"; // Two-squares (restore) icon
                }
            }
        }

        private void btnSaveandClose_Click(object sender, RoutedEventArgs e)
        {
            subeditor.Save();
            if (_appWindow == null) return;
            MainWindow.ShowWindow();
            _appWindow.Hide();
        }
        EditorView editorview => EditorView.Instance;

        public static SubtitleEditorWindow ShowDialog(bool isLyricEditor = false)
        {
            if (_instance == null)
            {
                _instance = new SubtitleEditorWindow();
            }
            Debug.WriteLine("eesd");
            App.SubtitleEditorDialogInstance = _instance;
            if (isLyricEditor)
            {
                _instance.subeditor.Visibility = Visibility.Collapsed;
                _instance.lyreditor.Visibility = Visibility.Visible;
            }
            else
            {
                _instance.subeditor.Visibility = Visibility.Visible;
                _instance.lyreditor.Visibility = Visibility.Collapsed;
            }
            //    _instance.ResizeWind(Width, Height);

            _instance.Activate();
            return _instance;
        }

        private void btnCancelandClose_Click(object sender, RoutedEventArgs e)
        {
            if (_appWindow == null) return;
            MainWindow.ShowWindow();
            _appWindow.Hide();
        }
    }
}

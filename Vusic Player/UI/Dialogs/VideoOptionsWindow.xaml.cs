using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Runtime.InteropServices;
using System.Windows.Media.Media3D;
using Vusic_Player.Configuration.Helper.UI;
using WinRT;
using WinRT.Interop;



namespace Vusic_Player.UI.Dialogs
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class VideoOptionsWindow : Window
    {
        public event Action? CloseRequested;
        public event Action? PrimaryRequested;
        public event Action? SecondaryRequested;

        const int GWL_HWNDPARENT = -8;
        private static VideoOptionsWindow? _instance;
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


        public VideoOptionsWindow()
        {
            InitializeComponent();
            TrySetAcrylicBackdrop(true);
            DispatcherQueue.EnsureSystemDispatcherQueue();
            DisableCloseButton();
            this.AppWindow.Closing += AppWindow_Closing;

            this.ExtendsContentIntoTitleBar = true;
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
        private T FindChild<T>(DependencyObject parent, string childName)
where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is T typedChild &&
                    (child as FrameworkElement)?.Name == childName)
                {
                    return typedChild;
                }

                var result = FindChild<T>(child, childName);
                if (result != null)
                    return result;
            }

            return null!;
        }

        private void StartShimmer(Button button)
        {
            button.ApplyTemplate();

            var transform = FindChild<TranslateTransform>(button, "GradientTransform");
            if (transform == null)
                return;

            var animation = new DoubleAnimation
            {
                From = -1,
                To = 1,
                Duration = new Duration(TimeSpan.FromSeconds(2)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(animation, transform);
            Storyboard.SetTargetProperty(animation, "X");

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
        public static void HideDialog()
        {
            if (_appWindow == null) return;
        //    MainWindow.ShowWindow();
            _appWindow.Hide();
        }

        bool TrySetAcrylicBackdrop(bool useAcrylicThin)
        {
            if (DesktopAcrylicController.IsSupported())
            {
                DispatcherQueue.EnsureSystemDispatcherQueue();

                // Hooking up the policy object
                configurationSource = new SystemBackdropConfiguration();
                Activated += Window_Activated;


                Closed += Window_Closed;
                ((FrameworkElement)Content).ActualThemeChanged += Window_ThemeChanged;

                // Initial configuration state.
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
        private void OceanDialog_CloseRequested()
        {
            HideDialog();
        }
        public void Setup(Grid contents, bool showact)
        {
            if (contents.Parent is Panel parentPanel)
                parentPanel.Children.Remove(contents);

            Contents.Children.Clear();
            Contents.Children.Add(contents);
            StartShimmer(btnPrimary);
            if (showact == true)
            {
                stkActionButtons.Visibility = Visibility.Visible;
            }
            else
            {
                stkActionButtons.Visibility = Visibility.Collapsed;
            }
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
            HideDialog();
        }

        private void btnSecondary_Click(object sender, RoutedEventArgs e)
        {
            SecondaryRequested?.Invoke();
        }

        private void btnPrimary_Click(object sender, RoutedEventArgs e)
        {
            PrimaryRequested?.Invoke();
        }

        private void ChangeDialogSize_Click(object sender, RoutedEventArgs e)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new Windows.Graphics.SizeInt32(400, 260));
        }
        public enum OptionType
        {
            VideoOptions,
            AudioOptions,
            SubtitlesOptions
        }

        public enum Video
        {
            General,
            Filters,
            Orientation,
            AspectRatio
        }
        public static VideoOptionsWindow ShowWindow(OptionType optionType, Video video, int TabIndex, int SubTabIndex, int PanelIndex)
        {
            if (_instance == null)
            {
                _instance = new VideoOptionsWindow();
            }
            _instance.Navigate(TabIndex, SubTabIndex, PanelIndex);
            _instance.ResizeWind(800, 800);
            _instance.Activate();
            return _instance;
        }
        private void Navigate(int TabIndex, int SubTabIndex, int PanelIndex)
        {
            if (_instance == null) return;
            TabInd = TabIndex;
            SubTabInd = SubTabIndex;
            PanInd = PanelIndex;
          //  _instance.playersettings.Loaded += Playersettings_Loaded;

        }
        private int TabInd = 0;
        private int SubTabInd = 0;
        private int PanInd = 0;


        private void Playersettings_Loaded(object sender, RoutedEventArgs e)
        {
            ManualNavigationVideoSettings.TabIndex = TabInd;
            ManualNavigationVideoSettings.SubtabIndex = SubTabInd;
            ManualNavigationVideoSettings.PanelIndex = PanInd;
            ManualNavigationVideoSettings.CallNavig();
        }

        public static VideoOptionsWindow ShowDialog(Grid contents, int Width, int Height, bool ShowActionButtons)
        {
            if (_instance == null)
            {
                _instance = new VideoOptionsWindow();
            }
            App.OceanDialogInstance = _instance;

            _instance.Setup(contents, ShowActionButtons);
            _instance.ResizeWind(Width, Height);

            _instance.Activate();
            return _instance;
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
    }
}

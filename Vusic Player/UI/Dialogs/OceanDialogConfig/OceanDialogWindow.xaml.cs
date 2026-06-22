using Microsoft.UI;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Configuration.UserSettings;
using WinRT.Interop;
using WinRT;


namespace Vusic_Player.UI.Dialogs.OceanDialogConfig
{
    public sealed partial class OceanDialogWindow : Window
    {
        DesktopAcrylicController? acrylicController;
        SystemBackdropConfiguration? configurationSource;
        private void Window_Activated(object sender, WindowActivatedEventArgs args)
        {

            if (configurationSource != null)
            {
                configurationSource.IsInputActive =
                args.WindowActivationState != WindowActivationState.Deactivated;
            }

            if (acrylicController == null && DesktopAcrylicController.IsSupported())
            {
                TrySetAcrylicBackdrop(true);
            }
        }

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
        public void HideDialog()
        {
            if (_appWindow == null) return;
            _appWindow.Hide();
        }
        public void KillDialog()
        {
            if (_instance != null)
            {
                _instance.Close();
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

        private void OceanDialog_CloseRequested()
        {
            HideDialog();
        }

        private string _titleText = "Title";
        private string _closebuttonText = "Close";

        public string TitleText
        {
            get => _titleText;
            set
            {
                _titleText = value;
                if (txtTitle != null)
                    txtTitle.Text = value;
            }
        }

        public string CloseButtonText
        {
            get => _closebuttonText;
            set
            {
                _closebuttonText = value;
                //   if (txtClose != null)
                //            txtClose.Text = value;
            }
        }
        public string PrimaryButtonIcon = "";

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        public event Action? CloseRequested;
        public event Action? PrimaryRequested;
        public event Action? SecondaryRequested;

        const int GWL_HWNDPARENT = -8;
        private static OceanDialogWindow? _instance;
        void SetImage(Image img, string iconName)
        {
            if (!string.IsNullOrEmpty(iconName))
            {
                img.Source = new BitmapImage(
                    new Uri($"ms-appx:///Assets/{iconName}.png")
                );
            }
            else
            {
                img.Visibility = Visibility.Collapsed;
            }
        }
        public void Setup(string Title, bool secondaryvisible, ContentType contentType, OceanContentDialogDefault dlg, string CloseButtonTex, string primarybtntex, string secondarybtntext, string pbi, string sbi, string cbi, ObservableCollection<SongModel> existingitems, string SuggestedPlaylistName, string mssgimageicon, string mssgtext, PlaylistItem playlist, bool isPlaylistEdit, string artist = "", string AlbumDetails = "", bool isAudioPlaylist = true)
        {
            txtTitle.Text = Title;
            PlaylistCreation.Visibility = Visibility.Collapsed;
            InfoBoxControl.Visibility = Visibility.Collapsed;
            OnlineArtistPic.Visibility = Visibility.Collapsed;
            MessageBox.Visibility = Visibility.Collapsed;
            AlbumEditorBox.Visibility = Visibility.Collapsed;
            MassEdit.Visibility = Visibility.Collapsed;
            ShowMod.Visibility = Visibility.Collapsed;
            if (contentType == ContentType.PlaylistCreation || contentType == ContentType.PlaylistEdit)
            {
                PlaylistCreation.Visibility = Visibility.Visible;
                PlaylistCreation.ClearStuff();
                PlaylistCreation.AllSongs = existingitems;
                PlaylistCreation.PlaylistNameSuggested = SuggestedPlaylistName;
                if (isAudioPlaylist == false)
                {
                    Debug.WriteLine("Hhaha");

                    PlaylistCreation.VisibilityOfGenreProperty = Visibility.Visible;
                    PlaylistCreation.AddedMediaHeader = "Added Videos";
                    PlaylistCreation.AddedSongsButton = "Add Videos";
                    PlaylistCreation.EmptyListDisplay = "No videos have been added yet!";
                    PlaylistCreation.IsVideoPlaylist = true;
                    PlaylistCreation.SearchPlaceHolderText = "Find videos that you added...";
                }
                else
                {
                    Debug.WriteLine("Hhaha2");

                    PlaylistCreation.VisibilityOfGenreProperty = Visibility.Visible;
                    PlaylistCreation.AddedMediaHeader = "Added Songs";
                    PlaylistCreation.AddedSongsButton = "Add Songs";
                    PlaylistCreation.EmptyListDisplay = "No songs have been added yet!";
                    PlaylistCreation.IsVideoPlaylist = true;
                    PlaylistCreation.SearchPlaceHolderText = "Find songs that you added...";
                }
                if (isPlaylistEdit == true)
                {
                    Debug.WriteLine("YES EDIT");
                    PlaylistCreation.PlaylistEdit(playlist);
                }
            }
            else if (contentType == ContentType.MessageShow)
            {
                MessageBox.Visibility = Visibility.Visible;
                MessageBox.UpdateImage(mssgimageicon);
                MessageBox.UpdateMessage(mssgtext);
            }
            //}
            else if (contentType == ContentType.FileInformation)
            {
                InfoBoxControl.Visibility = Visibility.Visible;
                InfoBoxControl.ScrollIntoView();
            }

            // else if (contentType == ContentType.OnlineArtistPicture)
            // {
            //     OnlineArtistPic.Visibility = Visibility.Visible;
            ////     OnlineArtistPic.UpdateArtistName(artist);
            // }
            // else if (contentType == ContentType.AlbumDetails)
            // {
            //     AlbumEditorBox.Visibility = Visibility.Visible;
            //   //  AlbumEditorBox.LoadAlbum(AlbumDetails);
            // }
            else if (contentType == ContentType.MassEditing)
            {
                MassEdit.Visibility = Visibility.Visible;
                MassEdit.LoadItems(existingitems);
            }
            else if (contentType == ContentType.ShowModel)
            {
                ShowMod.Visibility = Visibility.Visible;
            }
            cnt = contentType;
            SetImage(imgPrimary, pbi);
            SetImage(imgSecondary, sbi);
            SetImage(imgClose, cbi);
            txtClose.Text = CloseButtonTex;
            btnClose.Visibility = string.IsNullOrEmpty(CloseButtonTex)
             ? Visibility.Collapsed
             : Visibility.Visible;
            txtPrimary.Text = primarybtntex;
            txtSecondary.Text = secondarybtntext;
            btnSecondary.Visibility = secondaryvisible
                 ? Visibility.Visible
                 : Visibility.Collapsed;
            if (primarybtntex == "")
            {
                btnPrimary.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnPrimary.Visibility = Visibility.Visible;
            }
            var style = (Style)rootgrr.Resources["OceanShimmer"];

            btnClose.Style = null;
            btnPrimary.Style = null;
            btnSecondary.Style = null;

            Button? target = null;

            if (dlg == OceanContentDialogDefault.Primary)
                target = btnPrimary;
            else if (dlg == OceanContentDialogDefault.Secondary)
                target = btnSecondary;
            else if (dlg == OceanContentDialogDefault.Close)
                target = btnClose;

            if (target != null)
            {
                target.Style = style;
                StartShimmer(target);
            }

        }
        ContentType cnt;
        public enum ContentType
        {
            PlaylistCreation,
            PlaylistEdit,
            FileInformation,
            OnlineArtistPicture,
            MessageShow,
            AlbumDetails,
            MassEditing,
            ShowModel,
        }
        public static OceanDialogWindow ShowDialog(string Title, bool secondaryvisible, ContentType contentType, OceanContentDialogDefault dlg, string CloseButtonTex, string primarybuttex, string secondarybuttontex, int Width, int Height, OceanContentDialogType DialogType, Window wind, string Primarybtnicon, string Secondarybtnicon, string Closebtnicon, ObservableCollection<SongModel> existingitems, string SuggestedPlaylistName, string mssgtext, string mssgimage, string artist, string AlbumDetails, PlaylistItem playlist, bool isPlaylistEdit, bool isAudioPlaylist)
        {
            if (_instance == null)
            {
                _instance = new OceanDialogWindow();
            }
            App.OceanDialogInstance = _instance;

            _instance.Setup(Title, secondaryvisible, contentType, dlg, CloseButtonTex, primarybuttex, secondarybuttontex, Primarybtnicon, Secondarybtnicon, Closebtnicon, existingitems, SuggestedPlaylistName, mssgimage, mssgtext, playlist, isPlaylistEdit, artist, AlbumDetails, isAudioPlaylist);
            _instance.ResizeWind(Width, Height);
            _instance.Activate();
            return _instance;
        }
        private AppWindow? _appWindow;
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


        public OceanDialogWindow()
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


            if (appWindow.Presenter is OverlappedPresenter presenter)
            {
                presenter.SetBorderAndTitleBar(true, false);
                presenter.IsMaximizable = false;
                presenter.IsMinimizable = false;
                presenter.IsResizable = false;

            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke();
            MainWindow.ShowWindow();

        }

        private void btnSecondary_Click(object sender, RoutedEventArgs e)
        {
            SecondaryRequested?.Invoke();
        }
        bool _isClosing = false;
        private async void btnPrimary_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Yes");
            if (_isClosing) return;
            _isClosing = true;
            try
            {
                Debug.WriteLine("Yes2");

                if (cnt == ContentType.OnlineArtistPicture)
                {
                    //      string image = OnlineArtistPic.GetSelectedImage();
                    //  await OnlineArtistPic.DownloadImageAsync(image);
                }
                else if (cnt == ContentType.PlaylistEdit)
                {
                    Debug.WriteLine("tEST0");

                    var plitem = Configuration.Helper.UI.PlaylistCreation.playlistItem;
                    if (plitem != null && plitem.PlaylistId != null)
                    {
                        var currentSettings = await SettingsLoader.LoadSettingsAsync();
                        var playlists = currentSettings.SavedPlaylists;
                        var exist = playlists.FirstOrDefault(p => p.PlaylistId == plitem.PlaylistId);

                        if (exist != null)
                        {
                            Debug.WriteLine("SHSHS");
                            var playlistItem = PlaylistCreation.GetEditedPlaylistItem();
                            string? baseName = playlistItem.PlaylistName;

                            if (string.IsNullOrEmpty(baseName)) baseName = "Playlist";
                            string? finalName = baseName;
                            if (baseName != exist.PlaylistName)
                            {

                                int counter = 1;
                                while (currentSettings.SavedPlaylists.Any(p =>
                                    string.Equals(p.PlaylistName, finalName, StringComparison.OrdinalIgnoreCase)))
                                {
                                    finalName = $"{baseName} ({counter++})";
                                }
                            }
                            exist.PlaylistName = finalName;
                            exist.PlaylistGenre = playlistItem.PlaylistGenre;
                            exist.SongsPaths = playlistItem.SongsPaths;
                            exist.Thumbnail = playlistItem.Thumbnail;
                            exist.PlaylistCount = $"{playlistItem.SongsPaths.Count} {(playlistItem.SongsPaths.Count == 1 ? "item" : "items")}";
                            await SettingsLoader.SaveSettingsAsync(currentSettings);
                        }
                    }
                }
                PrimaryRequested?.Invoke();
            }
            finally
            {
                _isClosing = false;
            }
        }

        private void ChangeDialogSize_Click(object sender, RoutedEventArgs e)
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

            appWindow.Resize(new Windows.Graphics.SizeInt32(400, 260));
        }
        public void ClearInternalEvents()
        {
            PrimaryRequested = null;
            SecondaryRequested = null;
            CloseRequested = null;
        }
        private void DragRegion_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;

        }
    }

}

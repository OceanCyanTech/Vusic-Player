using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.ObjectModel;
using Vusic_Player.Configuration.AppConfig;
using Vusic_Player.Configuration.ClassModels;

namespace Vusic_Player.UI.Dialogs.OceanDialogConfig
{
    public enum OceanContentDialogDefault
    {
        Primary,
        Secondary,
        Close
    }

    public enum OceanContentDialogType
    {
        Elevated,
    }

    public class OceanContentDialog
    {
        private static Action? _primaryAction;
        private static Action? _secondaryAction;
        private static Action? _closeAction;

        public static event Action PrimaryRequested
        {
            add => _primaryAction = value;
            remove
            {
                if (_primaryAction == value) _primaryAction = null;
            }
        }

        public static event Action SecondaryRequested
        {
            add => _secondaryAction = value;
            remove
            {
                if (_secondaryAction == value) _secondaryAction = null;
            }
        }

        public static event Action CloseRequested
        {
            add => _closeAction = value;
            remove
            {
                if (_closeAction == value) _closeAction = null;
            }
        }

        static OceanPopup? populs;
        private static OceanDialogWindow? currentDialog;

        public static void KillDlg()
        {
            if (currentDialog == null) return;
            currentDialog.KillDialog();
        }

        public static void HideDlg()
        {
            if (populs == null)
            {
                populs = new();
            }
            if (currentDialog == null) return;

            var close = _closeAction;
            ClearSubscribers();
            close?.Invoke();

            currentDialog.HideDialog();
            populs.Hide();

            RestoreMainWindowMaximization();
        }

        public static void ClearSubscribers()
        {
            _primaryAction = null;
            _secondaryAction = null;
            _closeAction = null;
        }

        private static void RestoreMainWindowMaximization()
        {
            if (App.MainWindowInstance is MainWindow windowhome)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(windowhome);
                WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(myWndId);

                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsMaximizable = true;
                }
            }
        }

        public static void Show(
            string Title, string PrimaryButtonText, string SecondaryButtonText, string CloseButtonText,
            OceanDialogWindow.ContentType contentType, OceanContentDialogDefault DefaultButton,
            XamlRoot root, int Width, int Height, OceanContentDialogType DialogType, Window ParentWindow,
            string PrimaryButtonIcon, string SecondaryButtonIcon, string CloseButtonIcon,
            ObservableCollection<SongModel> existingitems, string SuggestedPlaylistName,
            string mssgtext = "", string mssgimage = "appicon", string artist = "",
            string AlbumDetails = "", PlaylistItem? playlist = null,
            bool isPlaylistEdit = false, bool isAudioPlaylist = true)
        {
            bool secondaryvisible = !string.IsNullOrEmpty(SecondaryButtonText);

            if (App.MainWindowInstance is MainWindow windowhome)
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(windowhome);
                WindowId myWndId = Win32Interop.GetWindowIdFromWindow(hWnd);
                AppWindow appWindow = AppWindow.GetFromWindowId(myWndId);

                if (appWindow.Presenter is OverlappedPresenter presenter)
                {
                    presenter.IsMaximizable = false;
                }
            }

            currentDialog = OceanDialogWindow.ShowDialog(
                Title, secondaryvisible, contentType, DefaultButton, CloseButtonText,
                PrimaryButtonText, SecondaryButtonText, Width, Height, DialogType, ParentWindow,
                PrimaryButtonIcon, SecondaryButtonIcon, CloseButtonIcon, existingitems,
                SuggestedPlaylistName, mssgtext, mssgimage, artist, AlbumDetails,
                playlist!, isPlaylistEdit, isAudioPlaylist);

            currentDialog.ClearInternalEvents();

            if (populs == null)
            {
                populs = new();
            }
            populs.Hide();

            CenterDialog.CenterDialogRec(currentDialog, ParentWindow);
            currentDialog.Activate();

            // Primary action: trigger & clear all pending handlers
            currentDialog.PrimaryRequested += () =>
            {
                var action = _primaryAction;
                ClearSubscribers();
                action?.Invoke();
            };

            // Secondary action: trigger & clear all pending handlers
            currentDialog.SecondaryRequested += () =>
            {
                var action = _secondaryAction;
                ClearSubscribers();
                action?.Invoke();
            };

            // Close action: trigger, hide popup, restore window presenter, and clear handlers
            currentDialog.CloseRequested += () =>
            {
                var action = _closeAction;
                ClearSubscribers();
                action?.Invoke();

                currentDialog.HideDialog();
                populs.Hide();
                RestoreMainWindowMaximization();
            };

            populs.Show(root, "");
        }
    }
}
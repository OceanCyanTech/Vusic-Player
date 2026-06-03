using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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


    public class OceanContentDialog()
    {
        public static event Action? PrimaryRequested;
        public static event Action? SecondaryRequested;
        public static event Action? CloseRequested;
        public static event Action? HideRequested;
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
            CloseRequested?.Invoke();
            currentDialog.HideDialog();
          //  MainWindow.ShowWindow();
            populs.Hide();
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
        public static void ClearSubscribers()
        {
            PrimaryRequested = null;
        }
        static PlaylistItem? pl;
        public static void Show(string Title, string PrimaryButtonText, string SecondaryButtonText, string CloseButtonText, OceanDialogWindow.ContentType contentType, OceanContentDialogDefault DefaultButton, XamlRoot root, int Width, int Height, OceanContentDialogType DialogType, Window ParentWindow, string PrimaryButtonIcon, string SecondaryButtonIcon, string CloseButtonIcon, ObservableCollection<SongModel> existingitems, string SuggestedPlaylistName, string mssgtext = "", string mssgimage = "appicon", string artist = "", string AlbumDetails = "", PlaylistItem? playlist = null, bool isPlaylistEdit = false, bool isAudioPlaylist = true)
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
  Title, secondaryvisible, contentType, DefaultButton, CloseButtonText, PrimaryButtonText, SecondaryButtonText, Width, Height, DialogType, ParentWindow, PrimaryButtonIcon, SecondaryButtonIcon, CloseButtonIcon, existingitems, SuggestedPlaylistName, mssgtext, mssgimage, artist, AlbumDetails, playlist!, isPlaylistEdit, isAudioPlaylist);
            currentDialog.ClearInternalEvents();
            if (populs == null)
            {
                populs = new();
            }
            //     dlg.CloseButtonText = CloseButtonText;
            populs.Hide();
            CenterDialog.CenterDialogRec(currentDialog, ParentWindow);
            currentDialog.Activate();
            currentDialog.PrimaryRequested += () => PrimaryRequested?.Invoke();
            currentDialog.SecondaryRequested += () => SecondaryRequested?.Invoke();

            populs.Show(root, "");

            currentDialog.CloseRequested += () =>
            {
                CloseRequested?.Invoke();
                currentDialog.HideDialog();
         //       MainWindow.ShowWindow();
                populs.Hide();
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
            };


        }


    }

}

using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Vusic_Player.Extensions;
using Windows.Storage;
using WinRT.Interop;
using FileOpenPicker = Windows.Storage.Pickers.FileOpenPicker;

namespace Vusic_Player.FilePickers
{
    public class MediaPicker
    {
        public static async Task<StorageFile?> PickSingleVideo(Window window, string commitbuttontext)
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(openPicker, hWnd);
            foreach (var ext in VideoExtensions.List)
            {
                openPicker.FileTypeFilter.Add(ext);
            }
            openPicker.CommitButtonText = commitbuttontext;
            openPicker.FileTypeFilter.Add("*");
            var file = await openPicker.PickSingleFileAsync();
            return file;
        }
        public static async Task<StorageFile?> PickSingleImageFileAsync(Window wind, string commitbuttontext)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.CommitButtonText = commitbuttontext;
            IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(wind);
            if (hwnd == IntPtr.Zero)
            {
                hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
            }
            WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            #region ImageFileTypes 
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.FileTypeFilter.Add(".bmp");
            picker.FileTypeFilter.Add(".gif");
            picker.FileTypeFilter.Add(".tiff");
            picker.FileTypeFilter.Add(".webp");
            picker.FileTypeFilter.Add(".heic");
            picker.FileTypeFilter.Add(".svg");
            #endregion
            var files = await picker.PickSingleFileAsync();
            return files;

        }

        public static async Task<StorageFile?> PickSingle(Window window, string commitbuttontext)
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(openPicker, hWnd);
            foreach (var ext in VideoExtensions.List)
            {
                openPicker.FileTypeFilter.Add(ext);
            }
            foreach (var ext in AudioExtensions.List)
            {
                openPicker.FileTypeFilter.Add(ext);
            }
            openPicker.CommitButtonText = commitbuttontext;
            openPicker.FileTypeFilter.Add("*");
            var file = await openPicker.PickSingleFileAsync();
            return file;
        }

    }
}

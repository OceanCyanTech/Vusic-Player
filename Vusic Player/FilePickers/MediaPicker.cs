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

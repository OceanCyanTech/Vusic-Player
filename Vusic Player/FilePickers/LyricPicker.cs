using System;
using Microsoft.UI.Xaml;
using System.Threading.Tasks;
using Vusic_Player.Extensions;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Vusic_Player.FilePickers
{
    public class LyricPicker
    {
        public static async Task<StorageFile?> PickSingle(Window window, string commitbuttontext)
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(openPicker, hWnd);
            
            foreach (var ext in LyricExtensions.List)
            {
                openPicker.FileTypeFilter.Add(ext);
            }
            openPicker.CommitButtonText = commitbuttontext;
            var file = await openPicker.PickSingleFileAsync();
            return file;
        }

    }
}

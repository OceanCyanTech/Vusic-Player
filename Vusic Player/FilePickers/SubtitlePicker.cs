using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Vusic_Player.Extensions;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
namespace Vusic_Player.FilePickers
{
    public class SubtitlePicker
    {
        public static async Task<StorageFile?> PickSingle(Window window, string commitbuttontext)
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(openPicker, hWnd);
            foreach (var ext in SubtitlesExtensions.List)
            {
                openPicker.FileTypeFilter.Add(ext);
            }
            openPicker.CommitButtonText = commitbuttontext;
            var file = await openPicker.PickSingleFileAsync();
            return file;
        }
    }

}

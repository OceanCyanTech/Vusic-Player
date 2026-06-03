using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;
using Microsoft.UI.Xaml;

namespace Vusic_Player.FilePickers
{
    public class FolderPickerFunct
    {
        public static async Task<StorageFolder> PickFolder(Window window, string CommitString, PickerLocationId SuggestedStart)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            var hwnd = WindowNative.GetWindowHandle(window);
            InitializeWithWindow.Initialize(picker, hwnd);
            picker.SuggestedStartLocation = SuggestedStart;
            picker.CommitButtonText = CommitString;
            StorageFolder folder = await picker.PickSingleFolderAsync();
            return folder;
        }

    }
}

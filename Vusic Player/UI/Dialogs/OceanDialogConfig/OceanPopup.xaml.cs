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
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.OceanDialogConfig
{
    public sealed partial class OceanPopup : UserControl
    {
        private Popup popup;
        public OceanPopup()
        {
            this.InitializeComponent();


            popup = new Popup();
            popup.Child = this;

            // Make it cover the whole window to block input
            this.Loaded += (s, e) =>
            {
                if (App.MainWindowInstance != null)
                {
                    this.Width = App.MainWindowInstance.Bounds.Width;
                    this.Height = App.MainWindowInstance.Bounds.Height;
                }
            };

            if (App.MainWindowInstance != null)
            {
                App.MainWindowInstance.SizeChanged += MainWindowInstance_SizeChanged;
                UpdateLayout(App.MainWindowInstance.Bounds.Width, App.MainWindowInstance.Bounds.Height);
            }
        }
        public void Hide()
        {
            if (App.MainWindowInstance != null)
                App.MainWindowInstance.SizeChanged -= MainWindowInstance_SizeChanged;

            popup.IsOpen = false;
        }
        private void MainWindowInstance_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            Debug.WriteLine("Size modified");
            UpdateLayout(args.Size.Width, args.Size.Height);
        }
        private void UpdateLayout(double width, double height)
        {
            this.Width = width;
            this.Height = height;
        }
        public async void Show(XamlRoot root, string Title)
        {
            // This tells the popup exactly where to render in the window hierarchy
            popup.XamlRoot = root;
            popup.IsOpen = true;
            // await ShowOceanDialog(Title);
        }

        private Task WaitForClose(Window dialog)
        {
            var tcs = new TaskCompletionSource();

            dialog.Closed += (s, e) =>
            {
                tcs.SetResult();
            };

            return tcs.Task;
        }
        public async Task ShowOceanDialog(string Title)
        {

        }
        private void CenterDialog(Window dialog)
        {
            var parent = App.MainWindowInstance;

            var parentHwnd = WinRT.Interop.WindowNative.GetWindowHandle(parent);
            var dialogHwnd = WinRT.Interop.WindowNative.GetWindowHandle(dialog);

            var parentId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(parentHwnd);
            var dialogId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(dialogHwnd);

            var parentApp = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(parentId);
            var dialogApp = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(dialogId);

            var parentPos = parentApp.Position;
            var parentSize = parentApp.Size;
            var dialogSize = dialogApp.Size;

            dialogApp.Move(new Windows.Graphics.PointInt32(
                parentPos.X + (parentSize.Width - dialogSize.Width) / 2,
                parentPos.Y + (parentSize.Height - dialogSize.Height) / 2
            ));
        }
        public enum DefaultButton
        {
            Primary, Secondary, Close
        }
    }
}

using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.AppConfig
{
    public class CenterDialog
    {
        static Window? Parent;
        static Window? dlg;
        public static void CenterDialogRec(Window dialog, Window Parentwind)
        {
            Parent = Parentwind;
            var parent = Parentwind;
            if (parent == null) return;
            dlg = dialog;
            parent.SizeChanged += Parent_SizeChanged;
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

        private static void Parent_SizeChanged(object sender, WindowSizeChangedEventArgs args)
        {
            if (dlg == null || Parent == null) return;
            CenterDialogRec(dlg, Parent);
        }
    }
}

// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.7.0 (Build 131126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

#region Namespaces
using Microsoft.UI.Xaml;
#endregion

namespace Vusic_Player.Configuration.AppConfig
{
    /*  OCEANCYAN TECH HEADER
VUSIC PLAYER - VERSION v1.0.7.0
© OCEANCYAN TECH 2026

CLASS - CENTERDIALOG - Helper method to center a secondary dialog corresponding to its parent dialog
BUILD 131126
*/
    public class CenterDialog
    {
        #region Fields
        static Window? Parent;
        static Window? dlg;
        #endregion

        #region Functions
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
        #endregion
    }
}

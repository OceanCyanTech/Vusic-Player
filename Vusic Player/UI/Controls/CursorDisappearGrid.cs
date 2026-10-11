using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.UI.Controls
{
    public class CursorDisappearGrid : Grid
    {
        public void SetCursorVisibility(bool isVisible)
        {
            if (isVisible)
            {
                // Reset to default arrow cursor
                this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
            }
            else
            {
                // Creating a cursor and immediately disposing its underlying 
                // native handle makes it completely invisible to the compositor
                var hiddenCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
                hiddenCursor.Dispose();
                this.ProtectedCursor = hiddenCursor;
            }
        }
    }
}

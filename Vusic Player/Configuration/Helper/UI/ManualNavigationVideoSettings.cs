using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class ManualNavigationVideoSettings
    {
        public static int TabIndex = 0;
        public static int SubtabIndex = 0;
        public static int PanelIndex = 0;

        public static event Action? NavigCalled;
        public static void CallNavig()
        {
            NavigCalled?.Invoke();
        }
    }
}

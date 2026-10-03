using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public static event Action? ClearWatermarkvalues;
        public static void CallClearance()
        {
            ClearWatermarkvalues?.Invoke();
        }
        public static void CallNavig()
        {
            Debug.WriteLine("SPEAK I KNOW HY: " + TabIndex + " " + SubtabIndex + " " + PanelIndex);

            NavigCalled?.Invoke();
        }
    }
}

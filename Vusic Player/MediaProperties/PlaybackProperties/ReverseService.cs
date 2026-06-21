using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;

namespace Vusic_Player.MediaProperties.PlaybackProperties
{
    public class ReverseService
    {
        public static MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        public static void Reverse(bool IsReverseTrue)
        {
            if (PlayerService.Masterplayer == null) return;
            PlayerService.Masterplayer.ReversePlayback = IsReverseTrue;
            GeneralInfoService.ShowInfo("Playback Reversed");
            mediacontroller.IsReversePlayback = IsReverseTrue;
        }
    }
}

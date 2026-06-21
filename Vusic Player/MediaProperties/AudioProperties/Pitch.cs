using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;

namespace Vusic_Player.MediaProperties.AudioProperties
{
    public class Pitch
    {
        public static MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        public static void Apply(double pitch)
        {
            if (PlayerService.Masterplayer == null) return;

            PlayerService.Masterplayer.Config.Audio.Pitch = pitch;
            PlayerService.Masterplayer.Config.Audio.ReloadFilters();
            GeneralInfoService.ShowInfo($"Pitch set to {pitch.ToString("F1")}");
            mediacontroller.PitchValue = pitch;
        }
    }
}

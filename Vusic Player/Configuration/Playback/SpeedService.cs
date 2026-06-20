using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.UI;

namespace Vusic_Player.Configuration.Playback
{
    public class SpeedService
    {
        public static void Set(double value)
        {
            if (PlayerService.Masterplayer != null)
            {
                PlayerService.Masterplayer.Speed = value;
                GeneralInfoService.ShowInfo($"Speed set to {value.ToString("F1")}x");
            }
        }
    }
}

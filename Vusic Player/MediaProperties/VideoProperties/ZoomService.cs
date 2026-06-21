using System.Diagnostics;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.UI;
using Vusic_Player.Configuration.Playback;

namespace Vusic_Player.MediaProperties.VideoProperties
{
    public class ZoomService
    {
        public static MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;

        public static void Set(double zoom)
        {
            if (PlayerService.Masterplayer == null) return;
            PlayerService.Masterplayer.Config.Video.Zoom = zoom;
            Debug.WriteLine("Zoom Percent is " + zoom);
            GeneralInfoService.ShowInfo($"Zoom set to {zoom.ToString("F1")}%");
            mediacontroller.ZoomValue = zoom;

        }
    }
}

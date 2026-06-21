using FlyleafLib;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vortice.Direct3D11;
using Vusic_Player.Configuration;
using Vusic_Player.UI.UserViews.Controls;

namespace Vusic_Player.MediaProperties.VideoProperties
{
    public class Filter
    {
        public static void UpdateFilter(VideoProcessorFilter d3, FLFilters fl, int val, OceanSlider s, NumberBox n, TextBlock t)
        {
            if (t == null) return;
            if (PlayerService.Masterplayer == null) return;
            PlayerService.Masterplayer.Config.Video.D3Filters[d3].Value = val;
            PlayerService.Masterplayer.Config.Video.FLFilters[fl].Value = val;
            if (s.Value != val) s.Value = val;
            if (n.Value != val) n.Value = val;
            t.Text = $"{val}%";
        }
    }
}

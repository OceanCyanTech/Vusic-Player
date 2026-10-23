// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.0.0 (Build 211026) . All Rights Reserved.
// Licensed under the MIT License. See LICENSE in the project root.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SharpDX.XAudio2.Fx;
using SharpDX.XAudio2;
using SharpDX.XAPO.Fx;
using Vusic_Player.Configuration;
namespace Vusic_Player.MediaProperties.AudioProperties
{
    public class EqualizerService
    {
        public static void Create(double[]eqgains)
        {
            if (PlayerService.Masterplayer == null) return;

            PlayerService.Masterplayer.Config.Audio.EQGains = eqgains;
            PlayerService.Masterplayer.Config.Audio.EnableEqualizer = true;
            PlayerService.Masterplayer.Config.Audio.ReloadFilters();


        }
    }
}

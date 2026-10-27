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
using System.Collections.ObjectModel;
using System.ComponentModel;
using Vusic_Player.Configuration.ClassModels;
using System.Runtime.CompilerServices;
namespace Vusic_Player.MediaProperties.AudioProperties
{
    public class EqualizerService : INotifyPropertyChanged
    {
        public static EqualizerService instance = new EqualizerService();
        private ObservableCollection<EqualizerPreset> _eqpresetsglobal = new ObservableCollection<EqualizerPreset>();

        public static void Create(double[]eqgains)
        {
            if (PlayerService.Masterplayer == null) return;

            PlayerService.Masterplayer.Config.Audio.EQGains = eqgains;
            PlayerService.Masterplayer.Config.Audio.EnableEqualizer = true;
            PlayerService.Masterplayer.Config.Audio.ReloadFilters();


        }
        public ObservableCollection<EqualizerPreset> EqPresetsGlobal
        {
            get => _eqpresetsglobal;
            set
            {
                if (_eqpresetsglobal != value)
                {
                    _eqpresetsglobal = value;
                    OnPropertyChanged();
                }
            }
        }
        #region INotifyImplementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        #endregion

    }
}

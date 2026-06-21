using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration;

namespace Vusic_Player.MediaProperties.AudioProperties
{
    public class Delay : INotifyPropertyChanged
    {
        public static Delay Instance { get; } = new Delay();
        private double delval = 0;
        public double DelayValue
        {
            get => delval;
            set
            {
                if (PlayerService.Masterplayer == null) return;
                if (delval == 0)
                {
                    PlayerService.Masterplayer.Config.Audio.Delay = 0;
                }
                else
                {
                    long longValue = (long)value;
                    var finalval = longValue * 10000;

                    Debug.WriteLine("Donang " + finalval);
                    PlayerService.Masterplayer.Config.Audio.Delay += finalval;
                }
                delval = value;
                OnPropertyChanged();
            }
        }

        public static void Reset()
        {
            if (PlayerService.Masterplayer == null) return;
            PlayerService.Masterplayer.Config.Audio.Delay = 0;
            Instance.DelayValue = 0;

        }
        public static void Apply(string tagValue)
        {
            if (PlayerService.Masterplayer == null) return;

            if (long.TryParse(tagValue, out long ms))
            {
                Reset();

                long additionalTicks = ms * 10000;
                Debug.WriteLine("Ticks " + additionalTicks);
                PlayerService.Masterplayer.Config.Audio.Delay += additionalTicks;
                Instance.DelayValue = ms;
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }

}

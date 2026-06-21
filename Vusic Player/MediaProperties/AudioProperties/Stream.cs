using FlyleafLib.MediaFramework.MediaStream;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.UI;

namespace Vusic_Player.MediaProperties.AudioProperties
{
    public class Stream : INotifyPropertyChanged
    {
        public static void Set(AudioStream stream)
        {
            if (PlayerService.Masterplayer == null) return;
            if (stream != null)
            {
                PlayerService.Masterplayer.Config.Audio.Enabled = true;
                // Open the selected audio stream
                GeneralInfoService.ShowInfo($"Audio set to {stream.StreamIndex}. {stream.Language}");
                PlayerService.Masterplayer.Open(stream);
            }
            else
            {
                PlayerService.Masterplayer.Config.Audio.Enabled = false;
            }
        }
        public static void Disable(bool shouldDisable)
        {
            if (PlayerService.Masterplayer == null) return;
            bool disable = true;
            if (PlayerService.Masterplayer.IsPlaying == false)
            {
                disable = false;
            }
            PlayerService.Masterplayer.Config.Audio.Enabled = !shouldDisable;
            if (disable == false)
            {
                PlayerService.Masterplayer.Pause();
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<AudioStream>? AudStreams => PlayerService.Masterplayer?.Audio.Streams;
        public AudioStream? CurrentStream
        {

            get => PlayerService.Masterplayer?.Audio.Streams.FirstOrDefault(s => s.Enabled);
            set
            {
                if (value != null && !value.Enabled)
                {
                    if (PlayerService.Masterplayer == null) return;
                    PlayerService.Masterplayer.Config.Audio.Enabled = true;
                    PlayerService.Masterplayer.Open(value);
                    GeneralInfoService.ShowInfo($"Audio set to Stream {value.StreamIndex} {value.Language}");
                    OnPropertyChanged();
                }
            }
        }
        public void RaiseAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(CurrentStream));
            OnPropertyChanged(nameof(AudStreams));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

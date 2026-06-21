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

namespace Vusic_Player.MediaProperties.VideoProperties
{
    public class Stream : INotifyPropertyChanged

    {
        public static event Action? VideoSearch;
        public static void SearchInitiated()
        {
            VideoSearch?.Invoke();
        }

        public static void Set(VideoStream streamsub)
        {
            if (PlayerService.Masterplayer == null) return;
            var stream = streamsub;

            if (stream != null)
            {
                PlayerService.Masterplayer.Open(stream);
            }
            else
            {
                PlayerService.Masterplayer.Config.Video.Enabled = false;
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
            PlayerService.Masterplayer.Config.Video.Enabled = !shouldDisable;
            if (disable == false)
            {
                PlayerService.Masterplayer.Pause();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<VideoStream>? VidStreams => PlayerService.Masterplayer?.Video.Streams;
        public VideoStream? CurrentStream
        {

            get => PlayerService.Masterplayer?.Video.Streams.FirstOrDefault(s => s.Enabled);
            set
            {
                if (value != null && !value.Enabled)
                {
                    if (PlayerService.Masterplayer == null) return;
                    PlayerService.Masterplayer.Config.Video.Enabled = true;
                    GeneralInfoService.ShowInfo($"Video set to Stream {value.StreamIndex} {value.Language} ({value.Width}x{value.Height}) ");

                    PlayerService.Masterplayer.Open(value);
                    OnPropertyChanged();
                }
            }
        }
        public void RaiseAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(CurrentStream));
            OnPropertyChanged(nameof(VidStreams));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}

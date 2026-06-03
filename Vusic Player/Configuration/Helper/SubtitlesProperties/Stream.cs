using FlyleafLib.MediaFramework.MediaStream;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.UI;

namespace Vusic_Player.Configuration.Helper.SubtitlesProperties
{
    public class Stream : INotifyPropertyChanged
    {
        public static string ExternalSubtitlePath = string.Empty;
        public static event Action? SubtitlePathChanged;
        public static event Action? SubtitleSearch;
        public static event Action? ExternalAdded;
        public static void PathExternal()
        {
            SubtitlePathChanged?.Invoke();
        }
        public static void ExternalSubtitleAdded()
        {
            ExternalAdded?.Invoke();
        }
        public static void SearchInitiated()
        {
            SubtitleSearch?.Invoke();
        }
        public static void Set(SubtitlesStream streamsub)
        {
            if (PlayerService.Masterplayer == null) return;
            var stream = streamsub;
            PlayerService.Masterplayer.Config.Subtitles.Enabled = true;

            if (stream != null)
            {
                PlayerService.Masterplayer.Open(stream);
                GeneralInfoService.ShowInfo($"Subtitles set to {stream.StreamIndex}. {stream.Language}");
            }
            else
            {
                PlayerService.Masterplayer.Config.Subtitles.Enabled = false;
            }
        }
        public static void Disable(bool shouldDisable)
        {
            if (PlayerService.Masterplayer == null) return;
            PlayerService.Masterplayer.Config.Subtitles.Enabled = !shouldDisable;
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<SubtitlesStream>? SubStreams => PlayerService.Masterplayer?.Subtitles.Streams;
        public SubtitlesStream? CurrentStream
        {

            get => PlayerService.Masterplayer?.Subtitles.Streams.FirstOrDefault(s => s.Enabled);
            set
            {
                if (value != null && !value.Enabled)
                {
                    if (PlayerService.Masterplayer == null) return;
                    PlayerService.Masterplayer.Config.Subtitles.Enabled = true;
                    PlayerService.Masterplayer.Open(value);
                    GeneralInfoService.ShowInfo($"Subtitles set to {value.StreamIndex}. {value.Language}");
                    OnPropertyChanged();
                }
            }
        }
        public void RaiseAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(CurrentStream));
            OnPropertyChanged(nameof(SubStreams));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

}

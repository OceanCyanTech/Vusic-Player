using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Helper.UI;

namespace Vusic_Player.MediaProperties.VideoProperties
{
    public class Aspect : INotifyPropertyChanged
    {
        public static Aspect Instance { get; } = new Aspect();

        private double width = 16;
        public double Width
        {
            get => width;
            set
            {
                width = value;
                OnPropertyChanged();
            }
        }
        private string aspectdis = "16:9";
        public string AspectDisplay
        {
            get => aspectdis;
            set
            {
                aspectdis = value;
                OnPropertyChanged();
            }
        }
        private double height = 9;
        public double Height
        {
            get => height;
            set
            {
                height = value;
                OnPropertyChanged();
            }
        }
        public static void SetDefault()
        {
            if (PlayerService.Masterplayer == null) return;

            PlayerService.Masterplayer.Config.Video.AspectRatio = FlyleafLib.AspectRatio.Keep;
            Instance.AspectDisplay = PlayerService.Masterplayer.Video.AspectRatio.ValueStr;
            GeneralInfoService.ShowInfo($"{PlayerService.Masterplayer.Video.AspectRatio} Aspect Ratio (default)");

        }

        public static void SetAspectRatio(FlyleafLib.AspectRatio aspectratio)
        {
            if (PlayerService.Masterplayer == null) return;

            PlayerService.Masterplayer.Config.Video.AspectRatio = aspectratio;
            GeneralInfoService.ShowInfo($"{aspectratio} Aspect Ratio");
            Instance.AspectDisplay = $"{aspectratio}";
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}

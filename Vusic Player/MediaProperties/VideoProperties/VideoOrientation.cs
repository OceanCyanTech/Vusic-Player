using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration;

namespace Vusic_Player.MediaProperties.VideoProperties
{
    public enum FlipOrientation
    {
        Horizontal,
        Vertical
    }
    public class Orientation : INotifyPropertyChanged
    {
        public static Orientation Instance { get; } = new Orientation();
        private double anglerot = 0;
        public double AngleRotation
        {
            get => anglerot;
            set
            {
                anglerot = value;
                OnPropertyChanged();
            }
        }
        public static void Flip(FlipOrientation flip, bool isTrue)
        {
            if (flip == FlipOrientation.Horizontal)
            {
                if (PlayerService.Masterplayer == null) return;
                PlayerService.Masterplayer.Config.Video.HFlip = isTrue;
            }
            else if (flip == FlipOrientation.Vertical)
            {
                if (PlayerService.Masterplayer == null) return;
                PlayerService.Masterplayer.Config.Video.VFlip = isTrue;
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}

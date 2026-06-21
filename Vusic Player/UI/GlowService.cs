using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Threading;
using Windows.UI;
using DispatcherTimer = Microsoft.UI.Xaml.DispatcherTimer;

namespace Vusic_Player.UI
{
    public class GlowService : INotifyPropertyChanged
    {
        #region Fields
        private static Microsoft.UI.Xaml.DispatcherTimer? _rgbTimer;
        private static double _hue = 0;
        private static Visibility _glowEffectVisiblity = Visibility.Visible;
        private Color glowcolorprop = Colors.Crimson;

        #endregion
        public static GlowService Instance { get; } = new GlowService();
        public Color GlowColor
        {
            get => glowcolorprop;
            set
            {
                glowcolorprop = value;
                OnPropertyChanged();
            }
        }
        public Visibility GlowEffectVisibility
        {
            get => _glowEffectVisiblity;
            set
            {
                _glowEffectVisiblity = value;
                OnPropertyChanged();
            }
        }
        public static Color ColorFromHSV(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0) return Color.FromArgb(255, v, t, p);
            if (hi == 1) return Color.FromArgb(255, q, v, p);
            if (hi == 2) return Color.FromArgb(255, p, v, t);
            if (hi == 3) return Color.FromArgb(255, p, q, v);
            if (hi == 4) return Color.FromArgb(255, t, p, v);
            return Color.FromArgb(255, v, p, q);
        }
        public static void StopAnimation(Color original)
        {
            _rgbTimer?.Stop();
            Instance.GlowColor = original;
        }
        public static double GlowSpeed;
        public static void StartNeonGlowAnimation()
        {
            _rgbTimer = new DispatcherTimer();
            _rgbTimer.Interval = TimeSpan.FromMilliseconds(GlowSpeed);
            _rgbTimer.Tick += (s, e) =>
            {

                _hue += GlowSpeed;
                if (_hue >= 360) _hue = 0;


                Color newColor = ColorFromHSV(_hue, 1.0, 1.0);

                Instance.GlowColor = newColor;
            };

            _rgbTimer.Start();
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

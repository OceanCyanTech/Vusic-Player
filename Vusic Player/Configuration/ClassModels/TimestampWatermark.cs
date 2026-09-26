using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class TimestampWatermark : INotifyPropertyChanged
    {
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private string _startString = "00:00:00.000";
        private string _format = "HH:MM:SS:FF";
        private int _formatindex = 0;
        private string _endString = "00:00:00.000";
        private string _label = string.Empty;
        private string _fontName = "Segoe UI";
        private string _position = "Top Left";
        private bool _isshown =true;
        private double _fontSize = 24;
        private SolidColorBrush _fontColor = new SolidColorBrush(Microsoft.UI.Colors.DarkCyan);
        private static readonly string[] Formats = { @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\,fff" };
        public string StartString
        {
            get => _startString;
            set
            {
                if (SetProperty(ref _startString, value))
                {
                    if (TimeSpan.TryParseExact(value, Formats, CultureInfo.InvariantCulture, out TimeSpan parsed) && _startTime != parsed)
                    {
                        _startTime = parsed;
                        OnPropertyChanged(nameof(StartTime));
                    }
                }
            }
        }
        public string Position
        {
            get => _position;
            set
            {
                if (SetProperty(ref _position, value))
                {

                    OnPropertyChanged(nameof(Position));

                }
            }
        }
        public bool IsShown
        {
            get => _isshown;
            set
            {
                if (SetProperty(ref _isshown, value))
                {

                    OnPropertyChanged(nameof(IsShown));

                }
            }
        }
        public string Format
        {
            get => _format;
            set
            {
                if (SetProperty(ref _format, value))
                {

                    OnPropertyChanged(nameof(Format));

                }
            }
        }
        public int FormatIndex
        {
            get => _formatindex;
            set
            {
                if (SetProperty(ref _formatindex, value))
                {

                    OnPropertyChanged(nameof(FormatIndex));

                }
            }
        }
        public string EndString
        {
            get => _endString;
            set
            {
                if (SetProperty(ref _endString, value))
                {
                    if (TimeSpan.TryParseExact(value, Formats, CultureInfo.InvariantCulture, out TimeSpan parsed) && _endTime != parsed)
                    {
                        _endTime = parsed;
                        OnPropertyChanged(nameof(EndTime));
                    }
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;

        public TimeSpan StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public TimeSpan EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value))
            {
                return false;
            }

            storage = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }

}

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
    public class SubtitleCueModel : INotifyPropertyChanged
    {
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private string _startString = "00:00:00.000";
        private string _endString = "00:00:00.000";
        private string _text = string.Empty;
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

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
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

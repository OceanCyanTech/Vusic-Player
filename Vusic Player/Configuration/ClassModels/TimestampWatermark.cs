using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Playback;

namespace Vusic_Player.Configuration.ClassModels
{
    public class TimestampWatermark : INotifyPropertyChanged
    {
        private TimeSpan _startTime;
        private TimeSpan _endTime;
        private string _startString = "00:00:00.000";
        private string _format = @"hh\:mm\:ss\:ff";
        private int _formatindex = 0;
        private int _modeindex = 0;
        private int _positionindex = 0;
        private string _endString = "00:00:00.000";

        private string _label = string.Empty;
        private string _fontName = "Segoe UI";
        private string _position = "Top Left";
        private bool _isshown = true;
        private double _fontSize = 24;
        private SolidColorBrush _fontColor = new SolidColorBrush(Microsoft.UI.Colors.DarkCyan);
        private static readonly string[] Formats = { @"hh\:mm\:ss\.fff", @"hh\:mm\:ss\,fff" };

        private TimeSpan _offset = TimeSpan.Zero;
        private WatermarkMode _mode = WatermarkMode.ElapsedPlaybackTime;

        // Backing fields for FrameRate, BPM, and BeatsPerBar
        private double _frameRate = 30.0;
        private string _customstaticwatermark ="";
        private int _frameRateIndex = 4; // Default to index of 30 fps
        private double _bpm = 120.0;
        private int _beatsPerBar = 4;
        private int _beatsPerBarIndex = 1; // Default to index of 4/4

        #region Offset & Mode Properties

        public TimeSpan Offset
        {
            get => _offset;
            set
            {
                if (SetProperty(ref _offset, value))
                {
                    OnPropertyChanged(nameof(OffsetMilliseconds));
                }
            }
        }

        public double OffsetMilliseconds
        {
            get => _offset.TotalMilliseconds;
            set
            {
                if (_offset.TotalMilliseconds != value)
                {
                    _offset = TimeSpan.FromMilliseconds(value);
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Offset));
                }
            }
        }

        public WatermarkMode Mode
        {
            get => _mode;
            set
            {
                if (SetProperty(ref _mode, value))
                {
                    if (_modeindex != (int)value)
                    {
                        _modeindex = (int)value;
                        OnPropertyChanged(nameof(ModeIndex));
                    }
                }
            }
        }

        public int ModeIndex
        {
            get => _modeindex;
            set
            {
                if (SetProperty(ref _modeindex, value))
                {
                    if (Enum.IsDefined(typeof(WatermarkMode), value))
                    {
                        _mode = (WatermarkMode)value;
                        OnPropertyChanged(nameof(Mode));
                    }
                }
            }
        }

        #endregion

        #region FrameRate & Musical Timecode Properties

        public double FrameRate
        {
            get => _frameRate;
            set => SetProperty(ref _frameRate, value);
        }

        public int FrameRateIndex
        {
            get => _frameRateIndex;
            set => SetProperty(ref _frameRateIndex, value);
        }

        public double BPM
        {
            get => _bpm;
            set => SetProperty(ref _bpm, value);
        }

        public int BeatsPerBar
        {
            get => _beatsPerBar;
            set => SetProperty(ref _beatsPerBar, value);
        }

        public int BeatsPerBarIndex
        {
            get => _beatsPerBarIndex;
            set => SetProperty(ref _beatsPerBarIndex, value);
        }

        #endregion

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
            set => SetProperty(ref _position, value);
        }
        public string CustomStaticWatermarkText
        {
            get => _customstaticwatermark;
            set => SetProperty(ref _customstaticwatermark, value);
        }

        public bool IsShown
        {
            get => _isshown;
            set => SetProperty(ref _isshown, value);
        }

        public string Format
        {
            get => _format;
            set => SetProperty(ref _format, value);
        }

        public int FormatIndex
        {
            get => _formatindex;
            set => SetProperty(ref _formatindex, value);
        }

        public int PositionIndex
        {
            get => _positionindex;
            set => SetProperty(ref _positionindex, value);
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

        public string FontName
        {
            get => _fontName;
            set => SetProperty(ref _fontName, value);
        }

        public double FontSize
        {
            get => _fontSize;
            set => SetProperty(ref _fontSize, value);
        }

        public SolidColorBrush FontColor
        {
            get => _fontColor;
            set => SetProperty(ref _fontColor, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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
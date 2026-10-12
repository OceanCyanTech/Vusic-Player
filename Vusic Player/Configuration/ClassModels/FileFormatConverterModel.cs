using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Extensions;

namespace Vusic_Player.Configuration.ClassModels
{
    public class FileFormatConverterModel : INotifyPropertyChanged
    {
        #region Back Properties
        private string _filepath = "";
        private string _filename = "";
        private string _glyph = "\uEC4F";
        private string _fromformat = ".mp3";
        public TimeSpan? Duration { get; set; }

        private string _toformat = ".wav";
        private string _outputpath = "";
        private string _info = "";
        private BitmapImage _icon = new BitmapImage();
        private double _progress = 0;
        #endregion
        #region Main Properties
        public string FilePath
        {
            get => _filepath;
            set
            {
                if (_filepath != value)
                {
                    _filepath = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> AudioExtension = new List<string>(AudioExtensions.List);
        public List<string> VideoExtension = new List<string>(VideoExtensions.List);
        public string FormattedDuration
        {
            get
            {
                if (!Duration.HasValue)
                    return "00:00";

                TimeSpan ts = Duration.Value;

                return ts.TotalHours >= 1
                    ? $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}"
                    : $"{ts.Minutes:D2}:{ts.Seconds:D2}";
            }
        }
        public string Glyph
        {
            get => _glyph;
            set
            {
                if (_glyph != value)
                {
                    _glyph = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FileName
        {
            get => _filename;
            set
            {
                if (_filename != value)
                {
                    _filename = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FromFormat
        {
            get => _fromformat;
            set
            {
                if (_fromformat != value)
                {
                    _fromformat = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ToFormat
        {
            get => _toformat;
            set
            {
                if (_toformat != value)
                {
                    _toformat = value;
                    OnPropertyChanged();
                }
            }
        }
        public string OutputPath
        {
            get => _outputpath;
            set
            {
                if (_outputpath != value)
                {
                    _outputpath = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Info
        {
            get => _info;
            set
            {
                if (_info != value)
                {
                    _info = value;
                    OnPropertyChanged();
                }
            }
        }
        public BitmapImage Icon
        {
            get => _icon;
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Progress
        {
            get => _progress;
            set
            {
                if (_progress != value)
                {
                    _progress = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion
        #region INotifyImplementation
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        #endregion
    }
}

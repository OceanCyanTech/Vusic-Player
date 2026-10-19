using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Vusic_Player.Configuration.ClassModels
{
    public class FileFormatConverterModel : INotifyPropertyChanged
    {
        #region Back Properties
        private string _filepath = "";
        private string _filename = "";
        private string _glyph = "\uEC4F";
        private bool _isenabled = true;
        private string _fromformat = ".mp3";
        private string _imagestate = "";
        private List<string> _audioextensions = new List<string>();
        private List<string> _videoextensions = new List<string>();
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
        public bool IsEnabled
        {
            get => _isenabled;
            set
            {
                if (_isenabled != value)
                {
                    _isenabled = value;
                    OnPropertyChanged();
                }
            }
        }
        public List<string> VideoExtensionsList
        {
            get => _videoextensions;
            set
            {
                if (_videoextensions != value)
                {
                    _videoextensions = value;
                    OnPropertyChanged();
                }
            }
        }
        // Inside FileFormatConverterModel
        public bool CanPlayOutput => !string.IsNullOrEmpty(OutputPath) && File.Exists(OutputPath) && Progress >= 100;
        public List<string> AudioExtensionsList
        {
            get => _audioextensions;
            set
            {
                if (_audioextensions != value)
                {
                    _audioextensions = value;
                    OnPropertyChanged();
                }
            }
        }


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
        public string ImageState
        {
            get => _imagestate;
            set
            {
                if (_imagestate != value)
                {
                    _imagestate = value;
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
                    OnPropertyChanged(nameof(CanPlayOutput));
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
                    OnPropertyChanged(nameof(CanPlayOutput));
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

// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.8.0 (Build 241126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

using Flyleaf.FFmpeg;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels.Media
{
    public record MediaMetadata : INotifyPropertyChanged
    {
        #region Back Properties
        private string _title = "";
        private string _videoresolution = "Unknown x Unknown";
        private string _artist = "Unknown Artist";
        private string _album = "Unknown Album";
        private string _durationstring = "00:00";
        private string _lyricist = "";
        private string _fileformat = "";
        private string _contributingartists = "";
        private string _composer = "";
        private string _copyright = "";
        private string _publisher = "";
        private string _date = "05 December 1998";
        private string _performer = "";
        private string _genre = "";
        private string _comment = "";
        private string _videocodec = "";
private string _audiocodec = "";
private TimeSpan _duration = TimeSpan.Zero;
        private int _width = 0;
        private int _height = 0;
        private int _totaltracks = 0;
        private int _tracknumber = 0;
        private int _year = 1998;
        private string _audiobitrate = "0 kbps";
        private string _videobitrate = "0 kbps";
        private string _channels = "0 channels";
        private string _samplerate = "0 Hz";
        private string _filesize = "0 mb";
        private long _totalframes = 0;
        private BitmapImage _cover = new BitmapImage();
        #endregion
        #region Main Properties
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }
        public string VideoResolution
        {
            get => _videoresolution;
            set
            {
                if (_videoresolution != value)
                {
                    _videoresolution = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Artist
        {
            get => _artist;
            set
            {
                if (_artist != value)
                {
                    _artist = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Album
        {
            get => _album;
            set
            {
                if (_album != value)
                {
                    _album = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DurationString
        {
            get => _durationstring;
            set
            {
                if (_durationstring != value)
                {
                    _durationstring = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Lyricist
        {
            get => _lyricist;
            set
            {
                if (_lyricist != value)
                {
                    _lyricist = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FileFormat
        {
            get => _fileformat;
            set
            {
                if (_fileformat != value)
                {
                    _fileformat = value;
                    OnPropertyChanged();
                }
            }
        }
        public string ContributingArtists
        {
            get => _contributingartists;
            set
            {
                if (_contributingartists != value)
                {
                    _contributingartists = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Composer
        {
            get => _composer;
            set
            {
                if (_composer != value)
                {
                    _composer = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Copyright
        {
            get => _copyright;
            set
            {
                if (_copyright != value)
                {
                    _copyright = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Publisher
        {
            get => _publisher;
            set
            {
                if (_publisher != value)
                {
                    _publisher = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Date
        {
            get => _date;
            set
            {
                if (_date != value)
                {
                    _date = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Performer
        {
            get => _performer;
            set
            {
                if (_performer != value)
                {
                    _performer = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Genre
        {
            get => _genre;
            set
            {
                if (_genre != value)
                {
                    _genre = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Comment
        {
            get => _comment;
            set
            {
                if (_comment != value)
                {
                    _comment = value;
                    OnPropertyChanged();
                }
            }
        }
        public string VideoCodec
        {
            get => _videocodec;
            set
            {
                if (_videocodec != value)
                {
                    _videocodec = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AudioCodec
        {
            get => _audiocodec;
            set
            {
                if (_audiocodec != value)
                {
                    _audiocodec = value;
                    OnPropertyChanged();
                }
            }
        }
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }
        public int TotalTracks
        {
            get => _totaltracks;
            set
            {
                if (_totaltracks != value)
                {
                    _totaltracks = value;
                    OnPropertyChanged();
                }
            }
        }
        public int TrackNumber
        {
            get => _tracknumber;
            set
            {
                if (_tracknumber != value)
                {
                    _tracknumber = value;
                    OnPropertyChanged();
                }
            }
        }
        public int Year
        {
            get => _year;
            set
            {
                if (_year != value)
                {
                    _year = value;
                    OnPropertyChanged();
                }
            }
        }
        public string AudioBitrate
        {
            get => _audiobitrate;
            set
            {
                if (_audiobitrate != value)
                {
                    _audiobitrate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string VideoBitrate
        {
            get => _videobitrate;
            set
            {
                if (_videobitrate != value)
                {
                    _videobitrate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Channels
        {
            get => _channels;
            set
            {
                if (_channels != value)
                {
                    _channels = value;
                    OnPropertyChanged();
                }
            }
        }
        public string SampleRate
        {
            get => _samplerate;
            set
            {
                if (_samplerate != value)
                {
                    _samplerate = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FileSize
        {
            get => _filesize;
            set
            {
                if (_filesize != value)
                {
                    _filesize = value;
                    OnPropertyChanged();
                }
            }
        }
        public long TotalFrames
        {
            get => _totalframes;
            set
            {
                if (_totalframes != value)
                {
                    _totalframes = value;
                    OnPropertyChanged();
                }
            }
        }
        public BitmapImage Cover
        {
            get => _cover;
            set
            {
                if (_cover != value)
                {
                    _cover = value;
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

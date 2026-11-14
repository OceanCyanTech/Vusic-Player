// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.7.0 (Build 131126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

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
        private string _artist = "Unknown Artist";
        private string _album = "Unknown Album";
        private TimeSpan _duration = TimeSpan.Zero;
        private string _durationstring = "00:00";
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

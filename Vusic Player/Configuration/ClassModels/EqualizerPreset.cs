using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class EqualizerPreset : INotifyPropertyChanged
    {
        #region Back Properties
        private string _presetname = "";
        private double _gain31 = 0;
        private double _gain63 = 0;
        private double _gain125 = 0;
        private double _gain250 = 0;
        private double _gain500 = 0;
        private double _gain1000 = 0;
        private double _gain2000 = 0;
        private double _gain4000 = 0;
        private double _gain8000 = 0;
        private double _gain16000 = 0;
        #endregion
        #region Main Properties
        public string PresetName
        {
            get => _presetname;
            set
            {
                if (_presetname != value)
                {
                    _presetname = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain31
        {
            get => _gain31;
            set
            {
                if (_gain31 != value)
                {
                    _gain31 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain63
        {
            get => _gain63;
            set
            {
                if (_gain63 != value)
                {
                    _gain63 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain125
        {
            get => _gain125;
            set
            {
                if (_gain125 != value)
                {
                    _gain125 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain250
        {
            get => _gain250;
            set
            {
                if (_gain250 != value)
                {
                    _gain250 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain500
        {
            get => _gain500;
            set
            {
                if (_gain500 != value)
                {
                    _gain500 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain1000
        {
            get => _gain1000;
            set
            {
                if (_gain1000 != value)
                {
                    _gain1000 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain2000
        {
            get => _gain2000;
            set
            {
                if (_gain2000 != value)
                {
                    _gain2000 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain4000
        {
            get => _gain4000;
            set
            {
                if (_gain4000 != value)
                {
                    _gain4000 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain8000
        {
            get => _gain8000;
            set
            {
                if (_gain8000 != value)
                {
                    _gain8000 = value;
                    OnPropertyChanged();
                }
            }
        }
        public double Gain16000
        {
            get => _gain16000;
            set
            {
                if (_gain16000 != value)
                {
                    _gain16000 = value;
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

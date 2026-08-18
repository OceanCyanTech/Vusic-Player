using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vusic_Player.Configuration.ClassModels
{
    public class DeviceOutputShow : INotifyPropertyChanged
    {
        private string _deviceName = "";
        private string _deviceUserName = "";
        private string _deviceThumbnail = "ms-appx:///Assets/appicon.png";
        private string _deviceID = "";
        private string _deviceVolume = "";
        private double _volume = 100;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DeviceName
        {
            get => _deviceName;
            set => SetProperty(ref _deviceName, value);
        }
        public string DeviceUserName
        {
            get => _deviceUserName;
            set => SetProperty(ref _deviceUserName, value);
        }

        public string DeviceThumbnail
        {
            get => _deviceThumbnail;
            set => SetProperty(ref _deviceThumbnail, value);
        }

        public string DeviceID
        {
            get => _deviceID;
            set => SetProperty(ref _deviceID, value);
        }

        public string DeviceVolume
        {
            get => _deviceVolume;
            set => SetProperty(ref _deviceVolume, value);
        }

        public double Volume
        {
            get => _volume;
            set => SetProperty(ref _volume, value);
        }

        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using FlyleafLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.UI;

namespace Vusic_Player.MediaProperties.AudioProperties
{
    public class Device : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public ObservableCollection<AudioEngine.AudioEndpoint> Devices => Engine.Audio.Devices;
        public AudioEngine.AudioEndpoint? CurrentDevice
        {

            get => Devices.FirstOrDefault(d => d.Id == Engine.Audio.CurrentDevice?.Id);
            set
            {
                if (value == null || value.Id == Engine.Audio.CurrentDevice?.Id)
                    return;

                // 2. Perform the update
                Engine.Audio.SetDevice(value.Id);
                GeneralInfoService.ShowInfo($"Audio Device set to {value.Name}");
                // 3. Notify the UI (This is now safe because step 1 prevents loops)
                OnPropertyChanged();
            }
        }
        public void RaiseAllPropertiesChanged()
        {
            OnPropertyChanged(nameof(CurrentDevice));
            OnPropertyChanged(nameof(Devices));
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

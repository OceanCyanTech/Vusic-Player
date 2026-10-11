using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Helper.UI.Customization;

namespace Vusic_Player.Configuration.Playback
{
    public class SeekService : INotifyPropertyChanged
    {

        private double _skipahead = 10;
        private double _skipback = 10;

        public double SkipAhead
        {
            get => _skipahead;
            set => SetProperty(ref _skipahead, value);
        }

        public double SkipBack
        {
            get => _skipback;
            set => SetProperty(ref _skipback, value);
        }
        public static SeekService Instance { get; } = new SeekService();

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler? PropertyChanged;


        /// Compares current value with new value. If different, updates and raises notification.

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            var dispatcher = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()
                             ?? App.MainWindowInstance?.DispatcherQueue;

            if (dispatcher != null && !dispatcher.HasThreadAccess)
            {
                dispatcher.TryEnqueue(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)));
            }
            else
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }

        #endregion

    }
}

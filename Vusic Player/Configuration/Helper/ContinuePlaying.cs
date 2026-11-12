using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vusic_Player.Configuration.ClassModels;

namespace Vusic_Player.Configuration.Helper
{
    public class ContinuePlaying : INotifyPropertyChanged
    {
        // Singleton instance so WinUI 3 XAML {x:Bind} can observe property changes
        public static ContinuePlaying Instance { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;
        public static event Action? InvokeList;

        private VideoProgress? _videoProgressMain;
        public VideoProgress? VideoProgressMain
        {
            get => _videoProgressMain;
            set => SetProperty(ref _videoProgressMain, value);
        }

        public static void InvokeCall()
        {
            InvokeList?.Invoke();
        }

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
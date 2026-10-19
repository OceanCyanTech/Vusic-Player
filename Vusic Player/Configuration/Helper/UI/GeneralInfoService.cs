using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class GeneralInfoService : INotifyPropertyChanged
    {
        public string? InfoText { get; set; }

        public static event Action<string>? OnInfoRequest;
        public static void ShowInfo(string information)
        {
            string text = information;

            OnInfoRequest?.Invoke(text);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

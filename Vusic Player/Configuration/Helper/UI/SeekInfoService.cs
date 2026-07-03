using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class SeekInfoService : INotifyPropertyChanged
    {
        public string? SeekText { get; set; }

        public static event Action<string, bool>? OnSeekRequest;
        public static void ShowSeek(int seconds)
        {
            string text = seconds > 0 ? $"+{seconds}" : $"{seconds}";
            bool isForward = seconds > 0;

            // Fire the event
            OnSeekRequest?.Invoke(text, isForward);
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}

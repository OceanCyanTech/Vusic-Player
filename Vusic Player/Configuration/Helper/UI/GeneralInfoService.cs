using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Playback;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class EditorView : INotifyPropertyChanged
    {

        public static EditorView Instance { get; } = new EditorView();

        private string _header = "Subtitle Editor";
        private string _btnnewsubtitlefile = "New Subtitle File";
        private string _btnloadsubtitle = "Load Subtitle File";
        private Visibility _cmbPlaceholdertext = Visibility.Visible;
        private string _editorViewPlural = "Subtitles";
        public string Header
        {
            get => _header;
            set
            {
                if (SetProperty(ref _header, value))
                {
                    OnPropertyChanged(nameof(Header));
                }
            }
        }
        public string BtnNewSubtitleFile
        {
            get => _btnnewsubtitlefile;
            set
            {
                if (SetProperty(ref _btnnewsubtitlefile, value))
                {
                    OnPropertyChanged(nameof(BtnNewSubtitleFile));
                }
            }
        }
        public string BtnLoadSubtitleFile
        {
            get => _btnloadsubtitle;
            set
            {
                if (SetProperty(ref _btnloadsubtitle, value))
                {
                    OnPropertyChanged(nameof(BtnLoadSubtitleFile));
                }
            }
        }

        public Visibility CmbPlaceholderText
        {
            get => _cmbPlaceholdertext;
            set
            {
                if (SetProperty(ref _cmbPlaceholdertext, value))
                {
                    OnPropertyChanged(nameof(CmbPlaceholderText));
                }
            }
        }
        public string ViewPlural
        {
            get => _editorViewPlural;
            set
            {
                if (SetProperty(ref _editorViewPlural, value))
                {
                    OnPropertyChanged(nameof(ViewPlural));
                }
            }
        }

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

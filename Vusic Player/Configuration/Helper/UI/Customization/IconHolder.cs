using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.UI.Customization
{
    public class IconHolder: INotifyPropertyChanged
    {
        private string _theme = "Ocean";
        private string _backgroundblur = "ms-appx:///Assets/Themes/Ocean/backgroundblur.png";

        private string _skipback = "ms-appx:///Assets/Themes/Ocean/skip10back.png";
        private string _next= $"ms-appx:///Assets/Themes/Ocean/next.png";
        private string _refresh= $"ms-appx:///Assets/Themes/Ocean/refresh.png";
        private string _error= $"ms-appx:///Assets/Themes/Ocean/error.png";
        private string _success= $"ms-appx:///Assets/Themes/Ocean/success.png";
        private string _info= $"ms-appx:///Assets/Themes/Ocean/infoicon.png";
        private string _skipahead = $"ms-appx:///Assets/Themes/Ocean/skip10ahead.png";
        private string _fontfamily = $"Bahnschrift";
        private string _openfolder = "ms-appx:///Assets/Themes/Ocean/openfolder.png";
        private string _importicon = "ms-appx:///Assets/Themes/Ocean/importicon.png";
        private string _removeicon = "ms-appx:///Assets/Themes/Ocean/removeicon.png";
        private string _foldericon = "ms-appx:///Assets/Themes/Ocean/foldericon.png";
        private string _sampleimage = "ms-appx:///Assets/Themes/Ocean/sampleimage.png";
        private string _tickicon = "ms-appx:///Assets/Themes/Ocean/tickicon.png";
        private string _defaultvideo = "ms-appx:///Assets/Themes/Ocean/defaultvideo.png";

        public static IconHolder Instance { get; } = new IconHolder();
        public string SampleImage
        {
            get => _sampleimage;
            set
            {
                if (_sampleimage != value)
                {
                    _sampleimage = value;
                    OnPropertyChanged();
                }
            }
        }
        public string TickIcon
        {
            get => _tickicon;
            set
            {
                if (_tickicon != value)
                {
                    _tickicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string DefaultVideo
        {
            get => _defaultvideo;
            set
            {
                if (_defaultvideo != value)
                {
                    _defaultvideo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Theme
        {
            get => _theme;
            set => SetProperty(ref _theme, value);
        }
        public string Refresh
        {
            get => _refresh;
            set => SetProperty(ref _refresh, value);
        }
        public string Error
        {
            get => _error;
            set => SetProperty(ref _error, value);
        }
        public string BackgroundBlur
        {
            get => _backgroundblur;
            set
            {
                if (_backgroundblur != value)
                {
                    _backgroundblur = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Openfolder
        {
            get => _openfolder;
            set
            {
                if (_openfolder != value)
                {
                    _openfolder = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Importicon
        {
            get => _importicon;
            set
            {
                if (_importicon != value)
                {
                    _importicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Removeicon
        {
            get => _removeicon;
            set
            {
                if (_removeicon != value)
                {
                    _removeicon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FolderIcon
        {
            get => _foldericon;
            set
            {
                if (_foldericon != value)
                {
                    _foldericon = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Next
        {
            get => _next;
            set => SetProperty(ref _next, value);
        }

        public string Success
        {
            get => _success;
            set => SetProperty(ref _success, value);
        }

        public string InfoIcon
        {
            get => _info;
            set => SetProperty(ref _info, value);
        }
        public string SkipBack
        {
            get => _skipback;
            set => SetProperty(ref _skipback, value);
        }
        public string SkipAhead
        {
            get => _skipahead;
            set => SetProperty(ref _skipahead, value);
        }

        public string FontFamily
        {
            get => _fontfamily;
            set => SetProperty(ref _fontfamily, value);
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

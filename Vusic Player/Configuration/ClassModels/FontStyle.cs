using Microsoft.UI.Xaml.Media;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vusic_Player.Configuration.ClassModels
{
    public class FontStyleModel
    {
        #region Back Properties
        private FontFamily _fontfam = new FontFamily("Segoe UI Variable Text");
        private string _fontname = "Segoe UI Variable Text";
        #endregion
        #region Main Properties
        public FontFamily FontFam
        {
            get => _fontfam;
            set
            {
                if (_fontfam != value)
                {
                    _fontfam = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FontName
        {
            get => _fontname;
            set
            {
                if (_fontname != value)
                {
                    _fontname = value;
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

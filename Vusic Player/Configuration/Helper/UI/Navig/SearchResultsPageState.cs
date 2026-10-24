using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Vusic_Player.Configuration.Helper.UI.Navig
{
    public class SearchResultsPageState : INotifyPropertyChanged
    {
        public static SearchResultsPageState Instance = new SearchResultsPageState();
        #region Back Properties
        private bool _alreadynavigatedtosearchresultspage = false;
        private bool _alreadynavigatedtosettingspage = false;
        #endregion
        #region Main Properties
        public bool alreadyNavigatedtoSearchResultsPage
        {
            get => _alreadynavigatedtosearchresultspage;
            set
            {
                if (_alreadynavigatedtosearchresultspage != value)
                {
                    _alreadynavigatedtosearchresultspage = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool alreadyNavigatedtoSettingsPage
        {
            get => _alreadynavigatedtosettingspage;
            set
            {
                if (_alreadynavigatedtosettingspage != value)
                {
                    _alreadynavigatedtosettingspage = value;
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

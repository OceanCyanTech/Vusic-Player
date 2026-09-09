using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class ShowData : INotifyPropertyChanged
    {
        private string _showName = "";
        private string _showID = "";
        private string _currentSeasonDirectory = "";
        private string _mainShowDirectory = "";
        private List<EpisodeModel> _episodes = new();
        private List<PlaylistItem> _seasons = new();
        private bool _isFromFirst = true;
        private int _currentSeasonNumber = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ShowName
        {
            get => _showName;
            set => SetField(ref _showName, value);
        }
        public string MainShowDirectory
        {
            get => _mainShowDirectory;
            set => SetField(ref _mainShowDirectory, value);
        }

        public string ShowID
        {
            get => _showID;
            set => SetField(ref _showID, value);
        }

        public string CurrentSeasonDirectory
        {
            get => _currentSeasonDirectory;
            set => SetField(ref _currentSeasonDirectory, value);
        }

        public List<EpisodeModel> episodes
        {
            get => _episodes;
            set => SetField(ref _episodes, value);
        }

        public List<PlaylistItem> seasons
        {
            get => _seasons;
            set => SetField(ref _seasons, value);
        }
        private DateTimeOffset _releaseDate;
        public DateTimeOffset ReleaseDate
        {
            get => _releaseDate;
            set { if (_releaseDate != value) { _releaseDate = value; OnPropertyChanged(); } }
        }

        public bool IsFromFirst
        {
            get => _isFromFirst;
            set => SetField(ref _isFromFirst, value);
        }

        public int CurrentSeasonNumber
        {
            get => _currentSeasonNumber;
            set => SetField(ref _currentSeasonNumber, value);
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}

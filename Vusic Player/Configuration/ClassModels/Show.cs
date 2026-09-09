using Microsoft.UI.Dispatching;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;

namespace Vusic_Player.Configuration.ClassModels
{
    public class Show: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        private string _name = "";
        public string Name
        {
            get => _name;
            set { if (_name != value) { _name = value; OnPropertyChanged(); } }
        }

        private string _poster = "ms-appx:///Assets/appicon.png";
        public string Poster
        {
            get => _poster;
            set { if (_poster != value) { _poster = value; OnPropertyChanged(); } }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set { if (_description != value) { _description = value; OnPropertyChanged(); } }
        }

        private string _showID = "";
        public string ShowID
        {
            get => _showID;
            set { if (_showID != value) { _showID = value; OnPropertyChanged(); } }
        }

        private string? _genre;
        public string? Genre
        {
            get => _genre;
            set { if (_genre != value) { _genre = value; OnPropertyChanged(); } }
        }

        private DateTimeOffset _releaseDate;
        public DateTimeOffset ReleaseDate
        {
            get => _releaseDate;
            set { if (_releaseDate != value) { _releaseDate = value; OnPropertyChanged(); } }
        }

        private string _releaseDateString = "01 January 2000";
        public string ReleaseDateString
        {
            get => _releaseDateString;
            set { if (_releaseDateString != value) { _releaseDateString = value; OnPropertyChanged(); } }
        }

        private string _seasonCountString = "0 seasons";
        public string SeasonCountString
        {
            get => _seasonCountString;
            set { if (_seasonCountString != value) { _seasonCountString = value; OnPropertyChanged(); } }
        }

        private string _creators = "";
        public string Creators
        {
            get => _creators;
            set { if (_creators != value) { _creators = value; OnPropertyChanged(); } }
        }

        private ObservableCollection<string> _addedSeasons = new();
        public ObservableCollection<string> AddedSeasons
        {
            get => _addedSeasons;
            set { if (_addedSeasons != value) { _addedSeasons = value; OnPropertyChanged(); } }
        }

        private ObservableCollection<PlaylistItem> _seasonsToSend = new();
        public ObservableCollection<PlaylistItem> SeasonsToSend
        {
            get => _seasonsToSend;
            set { if (_seasonsToSend != value) { _seasonsToSend = value; OnPropertyChanged(); } }
        }

        private ObservableCollection<string> _unlinkedSeasons = new();
        public ObservableCollection<string> UnlinkedSeasons
        {
            get => _unlinkedSeasons;
            set { if (_unlinkedSeasons != value) { _unlinkedSeasons = value; OnPropertyChanged(); } }
        }

        private int _seasonCount = 0;
        public int SeasonCount
        {
            get => _seasonCount;
            set { if (_seasonCount != value) { _seasonCount = value; OnPropertyChanged(); } }
        }

        private string _crew = "";
        public string Crew
        {
            get => _crew;
            set { if (_crew != value) { _crew = value; OnPropertyChanged(); } }
        }

        private PlaylistItem? _season;
        public PlaylistItem? Season
        {
            get => _season;
            set { if (_season != value) { _season = value; OnPropertyChanged(); } }
        }

        private bool _isSeasonPage = false;
        public bool isSeasonPage
        {
            get => _isSeasonPage;
            set { if (_isSeasonPage != value) { _isSeasonPage = value; OnPropertyChanged(); } }
        }

        private string _directory = "";
        public string Directory
        {
            get => _directory;
            set { if (_directory != value) { _directory = value; OnPropertyChanged(); } }
        }

        private string? _tags;
        public string? Tags
        {
            get => _tags;
            set { if (_tags != value) { _tags = value; OnPropertyChanged(); } }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            });
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class MasterSearchModel : INotifyPropertyChanged
    {
        private string _resultMain = "";
        private int _score = 0;
        private string _imageThumbnail = "";
        private string _subInformation = "";
        private string _filePath = "";
        private string _artist = "";
        private string _album = "";
        private string _playlistID = "";
        private string _showID = "";
        private string _folderPath = "";
        private Filters _searchFilter = Filters.All;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ResultMain
        {
            get => _resultMain;
            set => SetField(ref _resultMain, value);
        }

        public int Score
        {
            get => _score;
            set => SetField(ref _score, value);
        }

        public string ImageThumbnail
        {
            get => _imageThumbnail;
            set => SetField(ref _imageThumbnail, value);
        }

        public string SubInformation
        {
            get => _subInformation;
            set => SetField(ref _subInformation, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetField(ref _filePath, value);
        }

        public string Artist
        {
            get => _artist;
            set => SetField(ref _artist, value);
        }

        public string Album
        {
            get => _album;
            set => SetField(ref _album, value);
        }

        public string PlaylistID
        {
            get => _playlistID;
            set => SetField(ref _playlistID, value);
        }

        public string ShowID
        {
            get => _showID;
            set => SetField(ref _showID, value);
        }

        public string FolderPath
        {
            get => _folderPath;
            set => SetField(ref _folderPath, value);
        }

        public Filters SearchFilter
        {
            get => _searchFilter;
            set => SetField(ref _searchFilter, value);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
    public enum Filters
    {
        All,
        Music,
        Videos,
        Artist,
        Playlist,
        Album,
        Settings,
        Pages,
        Playlists,
        Shows,
        Genres,
        Folders
    }
}

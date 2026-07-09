using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;

namespace Vusic_Player.Configuration.ClassModels
{
    public class RecentMusicModel : INotifyPropertyChanged

    {
        // --- Backing Fields ---
        private string _songName = "";
        private string _songPath = "";
        private int _playCount = 0;
        private string _folderName = "";
        private string _playCountDisplay = "0 times";
        private string _lastPlayed = "";
        private string _lastLyricPath = "";

        // --- Public Properties ---
        public string SongName
        {
            get => _songName;
            set { _songName = value; OnPropertyChanged(); }
        }

        public string SongPath
        {
            get => _songPath;
            set { _songPath = value; OnPropertyChanged(); }
        }

        public int PlayCount
        {
            get => _playCount;
            set { _playCount = value; OnPropertyChanged(); }
        }

        public string FolderName
        {
            get => _folderName;
            set { _folderName = value; OnPropertyChanged(); }
        }

        public string PlayCountDisplay
        {
            get => _playCountDisplay;
            set { _playCountDisplay = value; OnPropertyChanged(); }
        }

        public string LastPlayed
        {
            get => _lastPlayed;
            set { _lastPlayed = value; OnPropertyChanged(); }
        }

        public string LastLyricPath
        {
            get => _lastLyricPath;
            set { _lastLyricPath = value; OnPropertyChanged(); }
        }

        // --- Property Changed Notification ---
        public event PropertyChangedEventHandler? PropertyChanged;

     
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        [JsonIgnore]
        private BitmapImage _thumbnail = new();

        public BitmapImage Thumbnail
        {
            get => _thumbnail;
            set
            {
                _thumbnail = value;
                OnPropertyChanged(); // Crucial for telling WinUI to draw the image
            }
        }
    }
}

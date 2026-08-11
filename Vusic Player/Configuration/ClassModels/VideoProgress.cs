using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class VideoProgress : INotifyPropertyChanged
    {
        private string _fileName = "";
        public string FileName 
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        private string _folderName = "";
        public string FolderName
        {
            get => _folderName;
            set { _folderName = value; OnPropertyChanged(); }
        }

        private string? _thumbnailPath;
        public string? ThumbnailPath
        {
            get => _thumbnailPath;
            set { _thumbnailPath = value; OnPropertyChanged(); }
        }

        private string _filePath = "";
        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }
        private string _toolTipHover = "";
        public string HoverText
        {
            get => _toolTipHover;
            set { _toolTipHover = value; OnPropertyChanged(); }
        }

        private Visibility _visibiltyOfLoading = Visibility.Collapsed;
        public Visibility LoadingVisibility
        {
            get => _visibiltyOfLoading;
            set { _visibiltyOfLoading = value; OnPropertyChanged(); }
        }

        private double _currentDuration;
        public double CurrentDuration
        {
            get => _currentDuration;
            set { _currentDuration = value; OnPropertyChanged(); }
        }

        private double _totalDuration;
        public double TotalDuration
        {
            get => _totalDuration;
            set { _totalDuration = value; OnPropertyChanged(); }
        }
        public bool? IsNew { get; set; } = false;
        public bool? IsSubtitlesDisabled { get; set; } = false;
        public int SubtitleIndex { get; set; } = 0;
        public int PlayCount { get; set; } = 0;
        public bool? IsEpisode { get; set; } = false;
        public bool ShowInformationOfOpen { get; set; } = true;
        
        public string ShowAssociatedID { get; set; } = "";
        public int SeasonAssociated { get; set; } = 1;
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
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

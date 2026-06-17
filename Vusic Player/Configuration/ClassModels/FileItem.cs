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
    public class FileItem : INotifyPropertyChanged
    {
        public string OpenContext { get; set; } = "Play";
        public string OpenContextGlyph { get; set; } = "\uE768";
        public string FileInfoContext { get; set; } = "File Info";
        private string _filePath = "";
        private string _fileName = "File";
        private string _filehoverinfo = "File";
        private string _FavString = "File";

        public string Path
        {
            get => _filePath;
            set
            {
                if (_filePath != value)
                {
                    _filePath = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FavString
        {
            get => _FavString;
            set
            {
                if (_FavString != value)
                {
                    _FavString = value;
                    OnPropertyChanged();
                }
            }
        }
        public string FileHoverInfo
        {
            get => _filehoverinfo;
            set
            {
                if (_filehoverinfo != value)
                {
                    _filehoverinfo = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _fileName;
            set
            {
                if (_fileName != value)
                {
                    _fileName = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool IsFavourite
        {
            get => isFavourite;
            set
            {
                if (isFavourite != value)
                {
                    isFavourite = value;
                    OnPropertyChanged();
                }
            }
        }
        public Visibility VisibilityOfFileProperties
        {
            get => _VisibilityOfFileProperties;
            set
            {
                if (_VisibilityOfFileProperties != value)
                {
                    _VisibilityOfFileProperties = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool isFavourite { get; set; } = false;
        public long FileSize { get; set; } = 0;
        public bool isFolder { get; set; } = false;
        public Visibility _VisibilityOfFileProperties { get; set; } = Visibility.Visible;
        public DateTime FileCreationTime { get; set; } = DateTime.Now;
        public DateTime FileModifiedTime { get; set; } = DateTime.Now;
        public string Extension { get; set; } = "mp4";
      
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

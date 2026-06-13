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
        public string Name { get; set; } = "File";
        public string OpenContext { get; set; } = "Play";
        public string OpenContextGlyph { get; set; } = "\uE768";
        public string FileInfoContext { get; set; } = "File Info";
        public string Path { get; set; } = "";
        public string FileHoverInfo { get; set; } = "";
        public bool isFavourite { get; set; } = false;
        public long FileSize { get; set; } = 0;
        public bool isFolder { get; set; } = false;
        public Visibility VisibilityOfFileProperties { get; set; } = Visibility.Visible;
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

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
        public string SongName { get; set; } = "";
        public string SongPath { get; set; } = "";
        public int PlayCount { get; set; } = 0;
        public string FolderName { get; set; } = "";
        public string PlayCountDisplay { get; set; } = "0 times";
        public string LastPlayed { get; set; } = "";
        public string LastLyricPath { get; set; } = "";

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

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
    public class EpisodeModel : INotifyPropertyChanged
    {
        public string? EpisodeName { get; set; }
        public string? EpisodeCount { get; set; }
        private string _description = "No description available!";
        private string _duration = "00:00:00";
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(); }
        }

        public string Duration
        {
            get => _duration;
            set { _duration = value; OnPropertyChanged(); }
        }
        public string? FilePath { get; set; }
        [JsonIgnore]
        private BitmapImage? _thumbnail;
        public BitmapImage? Thumbnail
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

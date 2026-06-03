using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        [JsonIgnore]
        public BitmapImage? Thumbnail { get; set; } = null;
    }
}

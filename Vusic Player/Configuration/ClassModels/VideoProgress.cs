using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class VideoProgress
    {
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public bool? IsNew { get; set; } = false;
        public bool? IsEpisode { get; set; } = false;
        public double CurrentDuration { get; set; }
        public double TotalDuration { get; set; }
        [JsonIgnore]
        public BitmapImage? Thumbnail { get; set; }
    }
}

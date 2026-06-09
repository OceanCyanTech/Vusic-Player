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
        public bool? IsSubtitlesDisabled { get; set; } = false;
        public int SubtitleIndex { get; set; } = 0;
        public int PlayCount { get; set; } = 0;
        public bool? IsEpisode { get; set; } = false;
        public bool ShowInformationOfOpen { get; set; } = true;
        public double CurrentDuration { get; set; }
        public double TotalDuration { get; set; }
        public string ShowAssociatedID { get; set; } = "";
        public int SeasonAssociated { get; set; } = 1;
        [JsonIgnore]
        public BitmapImage? Thumbnail { get; set; }
    }
}

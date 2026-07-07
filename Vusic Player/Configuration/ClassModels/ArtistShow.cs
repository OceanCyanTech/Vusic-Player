using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Documents;

namespace Vusic_Player.Configuration.ClassModels
{
    public class ArtistShow
    {
        public string ArtistName { get; set; } = string.Empty;
        public string ArtistThumbnail { get; set; } = "ms-appx:///Assets/artistdefault.png";
        public string ArtistSongCount { get; set; } = string.Empty;
        public string ArtistAlbumCount { get; set; } = string.Empty;
        public List<SongModel> Songs { get; set; } = new();

        [JsonIgnore]
        public BitmapImage? ArtistThumbnailImage { get; set; }
    }
}

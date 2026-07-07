using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class ArtistDiscAlbumModel
    {
        public string AlbumName { get; set; } = "";

        public string AlbumCount { get; set; } = "";
        public string AlbumYear { get; set; } = "";
        public string AlbumArtists { get; set; } = "";
        public string Thumbnail { get; set; } = "";
        public List<SongModel> Songs { get; set; } = new();


        [JsonIgnore]
        public BitmapImage? AlbumCoverThumbnail { get; set; }
    }
}

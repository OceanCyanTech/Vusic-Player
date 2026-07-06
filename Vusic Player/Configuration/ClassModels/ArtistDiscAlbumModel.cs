using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class ArtistDiscAlbumModel
    {
        public string AlbumName { get; set; } = "";

        public string AlbumCount { get; set; } = "";
        public string AlbumYear { get; set; } = "";
        [JsonIgnore]
        public BitmapImage? AlbumCoverThumbnail { get; set; }
    }
}

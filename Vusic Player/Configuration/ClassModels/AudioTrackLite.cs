using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class AudioTrackLite
    {
        public string Title { get; set; } = string.Empty;
        public string AlbumName { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public TimeSpan? SongDuration { get; set; }
        public bool IsFavourite { get; set; }
    }
}

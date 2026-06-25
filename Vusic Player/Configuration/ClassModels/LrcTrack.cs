using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class LrcTrack
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("trackName")]
        public string TrackName { get; set; } = "";

        [JsonPropertyName("artistName")]
        public string ArtistName { get; set; } = "";

        [JsonPropertyName("albumName")]
        public string AlbumName { get; set; } = "";

        [JsonPropertyName("duration")]
        public double Duration { get; set; }
    

        [JsonPropertyName("plainLyrics")]
        public string PlainLyrics { get; set; } = "";

        [JsonPropertyName("syncedLyrics")]
        public string SyncedLyrics { get; set; } = "";
        [JsonIgnore]
        public string StringDuration { get; set; } = "";
    }
}

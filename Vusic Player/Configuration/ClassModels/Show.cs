using System;

namespace Vusic_Player.Configuration.ClassModels
{
    public class Show
    {
        public string? Name { get; set; }
        public string? Poster { get; set; }
        public string? Description { get; set; }
        public string? ShowID { get; set; }
        public string? Genre { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }
        public string? Creators { get; set; }
        public string? SeasonCount { get; set; }
        public string? Crew { get; set; }
        public PlaylistItem? Season { get; set; }
        public bool isSeasonPage { get; set; } = false;
        public string? Directory { get; set; }
        public string? Tags { get; set; }
    }
}

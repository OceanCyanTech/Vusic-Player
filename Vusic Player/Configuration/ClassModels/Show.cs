using System;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace Vusic_Player.Configuration.ClassModels
{
    public class Show
    {
        public string Name { get; set; } = "";
        public string? Poster { get; set; }
        public string? Description { get; set; }
        public string ShowID { get; set; } = "";
        public string? Genre { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }
        public string ReleaseDateString { get; set; } = "01 January 2000";
        public string SeasonCountString { get; set; } = "0 seasons";
        public string Creators { get; set; } = "";
        public ObservableCollection<string> AddedSeasons { get; set; } = new();
        public ObservableCollection<PlaylistItem> SeasonsToSend { get; set; } = new();
        public ObservableCollection<string> UnlinkedSeasons { get; set; } = new();
        public int SeasonCount { get; set; } = 0;
        public string Crew { get; set; } = "";
        public PlaylistItem? Season { get; set; }
        public bool isSeasonPage { get; set; } = false;
        public string Directory { get; set; } = "";
        public string? Tags { get; set; }
    }
}

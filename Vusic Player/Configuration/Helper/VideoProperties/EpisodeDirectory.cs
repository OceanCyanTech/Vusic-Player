using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration.ClassModels;
using Vusic_Player.Extensions;

namespace Vusic_Player.Configuration.Helper.VideoProperties
{
    public class EpisodeDirectory
    {
        public static ObservableCollection<EpisodeModel> EpisodeList = new ObservableCollection<EpisodeModel>();
        public static List<EpisodeModel> GetEpisodeShowInfo(string EpisodePath)
        {
            var episodePlaceholders = new List<EpisodeModel>();

            var updirectory = Path.GetDirectoryName(EpisodePath);
            if (updirectory == null) return new List<EpisodeModel>();
            var videoFiles = Directory.EnumerateFiles(updirectory)
        .Where(file => VideoExtensions.List.Contains(Path.GetExtension(file).ToLower()))
        .OrderBy(file => file)
        .ToList();
            var episodePatterns = new List<string>
{
    // 1. Standard SxxExx or just Exx (Looks for 'E' or 'EP' optionally preceded by 'Sxx')
    @"(?i)(?:s\d+)?e(\d+)\b",

    // 2. Multi-episode format: E02-E03, E02E03, e02_03
    @"(?i)e(\d+)(?:[-_]?e?(\d+))?\b",

    // 3. Standard text 'episode' or 'ep' followed by numbers (e.g., Ep.01, Episode 1)
    @"(?i)\b(?:ep|episode)(?:\s*|\s*\.\s*)(\d+)\b",

    // 4. X / Cross format: S01x02, 1x02, 1x2
    @"(?i)\b\d+x(\d+)\b",

    // 5. Bracketed numbers (Anime style): [02], (02)
    @"\[(\d+)\]",
    @"\((\d+)\)",

    // 6. Absolute / Standalone numbers: "Show - 02.mp4" 
    @"(?<=\s+|-|_|#)(\d+)(?=\.\w+$|\s+|-|_)"
};
            foreach (var filePath in videoFiles)
            {
                string fileName = Path.GetFileName(filePath);
                string episodeNumber = "Unknown";
                Debug.WriteLine($"Processing File: {fileName}");

                foreach (var pattern in episodePatterns)
                {
                    Match match = Regex.Match(fileName, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        var validGroups = match.Groups.Cast<Group>()
                                                      .Skip(1)
                                                      .Where(g => g.Success && !string.IsNullOrEmpty(g.Value))
                                                      .ToList();

                        Debug.WriteLine($"  [Check] Pattern '{pattern}' matched! Found {validGroups.Count} valid capture groups.");

                        if (validGroups.Any())
                        {
                            episodeNumber = validGroups.First().Value;
                            Debug.WriteLine($"  -> Match Found! Episode: {episodeNumber}");
                            break;
                        }
                    }
                }
                var newEpisode = new EpisodeModel
                {
                    EpisodeName = $"Episode {episodeNumber}",
                    Description = "Loading...",
                    Duration = "--:--:--",
                    FilePath = filePath,
                    CurrentShowDirectory = Path.GetDirectoryName(filePath)
                };

                EpisodeList.Add(newEpisode);
                episodePlaceholders.Add(newEpisode);
            }
            return episodePlaceholders;
        }
    }
}

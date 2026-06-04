using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vusic_Player.Configuration.UserSettings;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class ShowManager
    {
        public static int currentseason = 1;
        public static int currentepisode = 1;
        public static int totalepisodecount = 1;
        public static bool isNextSeasonAvailable = false;
        public static string ShowDirectory = "";
        public static string CurrentEpisodePath = "";
        public static void GetNextSeasonEpisodes()
        {

        }
        public static void UpdateCurrentSeason(string FilePath)
        {
            if (FilePath != null)
            {
                string pattern = @"\b(season\s*|s)(\d+)\b";
                string rootdirectory = Path.GetDirectoryName(FilePath) ?? string.Empty;
                if (rootdirectory != null)
                {
                    Match match = Regex.Match(rootdirectory, pattern, RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        int seasonNum = Convert.ToInt32(match.Groups[2].Value);
                        currentseason = seasonNum;
                    }
                }
            }
        }
        public static async void LoadAvailableShow()
        {

        }
        public static async void PlayNextEpisode()
        {
            for (currentepisode = 0;  currentepisode < totalepisodecount; currentepisode++)
            {
                if (currentepisode == totalepisodecount - 2)
                {
                    var currentSettings = await SettingsLoader.LoadSettingsAsync();
                    var showsavail = currentSettings.Shows;
               //     var existshow = 
                   // foreach(var show in showsavail)
                }
            }
        }
    }
}

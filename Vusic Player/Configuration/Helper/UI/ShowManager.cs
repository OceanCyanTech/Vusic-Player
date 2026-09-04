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
using Vusic_Player.Configuration.UserSettings;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class ShowManager
    {
     public static   ShowData mainShowPlayable = new ShowData();

        public static bool isLastEpisode = false;
        public static int currentseason = 1;
        public static int currentepisode = 1;
        public static int totalepisodecount = 1;
        public static bool isNextSeasonAvailable = false;
        public static string ShowDirectory = "";
        public static string CurrentEpisodePath = "";
        public static Show? CurrentShow;
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
        public static ObservableCollection<EpisodeModel> EpisodeList = new();
        public static async void LoadAvailableShow(string EpisodePath)
        {
            EpisodeList.Clear();
            Debug.WriteLine("Yes there are shows");
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var shows = currentSettings.Shows;
            foreach (var show in shows)
            {
                Debug.WriteLine(show.Name + " Show Name");
                var directorypath = show.Directory;
                if (directorypath != null)
                {
                    Debug.WriteLine("not null " + directorypath);
                        bool isInsideAndExists = IsFileInDirectory(directorypath, EpisodePath)
                                    && File.Exists(EpisodePath);
                    if (isInsideAndExists)
                    {
                        Debug.WriteLine("not null and exists");
                        CurrentShow = show;
                        Debug.WriteLine("Season count: " + CurrentShow.SeasonCount);
                        string rootPath = directorypath;
                        if (Directory.Exists(rootPath))
                        {
                            var primaryFolders = Directory.GetDirectories(rootPath, "*", SearchOption.TopDirectoryOnly).ToList();
                            primaryFolders.Insert(0, rootPath);

                            string pattern = @"\b(season\s*|s)(\d+)\b";

                            foreach (string path in primaryFolders)
                            {
                                string folderName = Path.GetFileName(path);
                                Match match = Regex.Match(folderName, pattern, RegexOptions.IgnoreCase);

                                if (path == rootPath) match = Regex.Match(new DirectoryInfo(rootPath).Name, pattern, RegexOptions.IgnoreCase);

                                if (match.Success)
                                {
                                    int seasonNum = Convert.ToInt32(match.Groups[2].Value);
                                    string seasonName = $"Season {seasonNum}";

                                }
                            }
                        }

                    }
                    return;
                }
            }
        }
        
        public static bool IsFileInDirectory(string directoryPath, string filePath)
        {
            string fullDirPath = Path.GetFullPath(directoryPath);
            string fullFilePath = Path.GetFullPath(filePath);

            if (!fullDirPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                fullDirPath += Path.DirectorySeparatorChar;
            }

            return fullFilePath.StartsWith(fullDirPath, StringComparison.OrdinalIgnoreCase);
        }
        public static async void PlayNextEpisode()
        {
            for (currentepisode = 0; currentepisode < totalepisodecount; currentepisode++)
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

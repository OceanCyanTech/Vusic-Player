using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.UserSettings;

namespace Vusic_Player.Configuration.AppConfig
{
    public class AppVersion
    {
        public static string VersionString = "1.1.0.0";

        static int counter2;
        public static async void LoadBuildCounter()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var counter = currentSettings.VersionCounter;
            if (counter.Count == 0)
            {
                counter.Add(0);
            }
            counter2 = counter.FirstOrDefault() + 1;
            Debug.WriteLine(counter.FirstOrDefault() + " shsh");
            counter.Clear();
            counter.Add(counter2);
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            BuildNumber = $"{DateTime.Now.ToString("MMddyy")}.{counter2}";
            Debug.WriteLine(BuildNumber + " hshs");
        }
        public static string BuildNumber = "";
        public static string VersionType = "Development Preview BETA 1 (BRANCH DEV)";
    }

}

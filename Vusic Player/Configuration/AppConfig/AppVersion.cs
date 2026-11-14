// Copyright © OceanCyan Tech 2026 - Vusic Player Version 1.0.7.0 (Build 131126) . All Rights Reserved.
// Licensed under the GNU License. See LICENSE in the project root.

#region Namespaces
using System;
using System.Linq;
using Vusic_Player.Configuration.UserSettings;
#endregion 

namespace Vusic_Player.Configuration.AppConfig
{
    /*  OCEANCYAN TECH HEADER
VUSIC PLAYER - VERSION v1.0.7.0
© OCEANCYAN TECH 2026

CLASS - APPVERSION - For Development Build Distinction
BUILD 131126
*/
    public class AppVersion
    {
        #region Fields
        public static string VersionString = "1.0.7.0";
        static int counter2;
        public static string BuildNumber = "";
        public static string VersionType = "RTM";

        #endregion

        #region Functions
        public static async void LoadBuildCounter()
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            var counter = currentSettings.VersionCounter;
            if (counter.Count == 0)
            {
                counter.Add(0);
            }
            counter2 = counter.FirstOrDefault() + 1;
            counter.Clear();
            counter.Add(counter2);
            await SettingsLoader.SaveSettingsAsync(currentSettings);
            BuildNumber = $"{DateTime.Now.ToString("MMddyy")}.{counter2}";
        }
        #endregion
    }

}

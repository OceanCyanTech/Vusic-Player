using FlyleafLib;
using System;

namespace Vusic_Player.Configuration
{
    public class EngineService
    {
        public static void StartEngine()
        {
            try
            {
                string packageFolder = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                string ffmpegDir = System.IO.Path.Combine(packageFolder, "FFmpeg");

                Engine.Start(new EngineConfig()
                {
#if DEBUG
                    LogOutput = ":debug",
                    LogLevel = LogLevel.Debug,
                    FFmpegLogLevel = Flyleaf.FFmpeg.LogLevel.Warn,
#endif
                    UIRefresh = false,
                    FFmpegPath = System.IO.Directory.Exists(ffmpegDir) ? ffmpegDir : ":FFmpeg",
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Engine start failed: {ex.Message}");
            }
        }
    }
}

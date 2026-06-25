using FlyleafLib;

namespace Vusic_Player.Configuration
{
    public class EngineService
    {
        public static void StartEngine()
        {
            Engine.Start(new EngineConfig()
            {
#if DEBUG
                LogOutput = ":debug",
                LogLevel = LogLevel.Debug,
                FFmpegLogLevel = Flyleaf.FFmpeg.LogLevel.Warn,
#endif

                UIRefresh = false, // For Activity Mode usage
                                   //   PluginsPath = ":Plugins",
                FFmpegPath = ":FFmpeg",
            });
       
        }
    }
}

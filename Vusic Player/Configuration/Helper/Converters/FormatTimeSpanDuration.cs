using System;

namespace Vusic_Player.Configuration.Helper.Converters
{
    public class FormatTimeSpanDuration
    {
        public static string Format(TimeSpan duration)
        {
            if (duration.Hours > 0)
                return duration.ToString(@"hh\:mm\:ss");

            return duration.ToString(@"mm\:ss");
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper
{
    public static class FormatValidator
    {
        // Check if the format is valid for TimeSpan (e.g. @"hh\:mm\:ss\.fff")
        public static bool IsValidTimeSpanFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format)) return false;

            try
            {
                TimeSpan.Zero.ToString(format);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Check if the format is valid for DateTime (e.g. "dd-MM-yyyy HH:mm:ss")
        public static bool IsValidDateTimeFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format)) return false;

            try
            {
                DateTime.Now.ToString(format);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        // Unified check: detects whether it targets DateTime or TimeSpan and validates
        public static bool IsValidCustomFormat(string format)
        {
            if (string.IsNullOrWhiteSpace(format)) return false;

            // Calendar date tokens require DateTime validation
            if (format.Contains("dd") || format.Contains("MM") || format.Contains("yy"))
            {
                return IsValidDateTimeFormat(format);
            }

            // Otherwise validate as TimeSpan
            return IsValidTimeSpanFormat(format);
        }
    }
}

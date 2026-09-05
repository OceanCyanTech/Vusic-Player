using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Vusic_Player.Configuration.AppConfig.Logger;

namespace Vusic_Player.Configuration.AppConfig
{
    public class LogEntry
    {
        public string? Icon { get; set; }
        public DateTime Timestamp { get; set; }
        public LogLevelType Level { get; set; }
        public string? Source { get; set; }
        public string Message { get; set; } = "";
    }
}

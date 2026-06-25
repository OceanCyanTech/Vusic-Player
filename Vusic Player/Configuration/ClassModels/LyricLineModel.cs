using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class LyricLineModel
    {
        public TimeSpan Timestamp { get; set; } 
        public string Line { get; set; } = "";
    }
}

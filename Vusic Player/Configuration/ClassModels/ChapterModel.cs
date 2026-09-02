using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class ChapterModel
    {
        public string ChapterTitle { get; set; } = "";
        public long StartTime { get; set; } = 0;
        public string StartTimeStr { get; set; } = "";
        public string EndTimeStr { get; set; } = "";
        public long EndTime { get; set; } = 0;

      
    }
}

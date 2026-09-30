using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vusic_Player.Configuration.Playback;

namespace Vusic_Player.Configuration.ClassModels
{
    public class LabelInfo
    {
        public WatermarkMode Mode { get; set; }
        public string Format { get; set; } = "";
        public TimeSpan Offset { get; set; } = TimeSpan.Zero;
        public DateTime BaseDateTime { get; set; } = DateTime.Now;
        public double FrameRate { get; set; } = 30.0; // Used for RunningFrames
        public double BPM { get; set; } = 120.0;       // Used for MusicalTimecode
        public int BeatsPerBar { get; set; } = 4;
    }
}

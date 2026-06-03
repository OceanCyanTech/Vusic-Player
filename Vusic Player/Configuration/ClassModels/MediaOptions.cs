using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class MediaOptions
    {
        // Basic Info & Tracking
        public string MediaPath { get; set; } = "";
        public bool IncludeMediaCurrentPositionTimestamp { get; set; } = true;

        // Visual Filters
        public double Brightness { get; set; } = 1.0;
        public double Saturation { get; set; } = 1.0;
        public double Hue { get; set; } = 0.0;
        public double Contrast { get; set; } = 1.0;
        public string AspectRatio { get; set; } = "16:9";

        // Transform & Orientation
        public bool FlipHorizontal { get; set; } = false;
        public bool FlipVertical { get; set; } = false;
        public double Rotation { get; set; } = 0.0;
        public double Zoom { get; set; } = 1.0;

        // Playback Settings
        public double Speed { get; set; } = 1.0;
        public bool ReversePlayback { get; set; } = false;

        // Output & Snapshot Settings
        public string SnapshotDirectory { get; set; } = "";
        public string VideoRecordingDirectory { get; set; } = "";
        public bool SnapshotIncludeTimestamp { get; set; } = true;

        // Subtitle Styling
        public string SubtitleFontName { get; set; } = "Segoe UI Variable Display";
        public double SubtitleFontSize { get; set; } = 28.0;
        public string SubtitleColor { get; set; } = "#FFFFFF";
        public bool SubtitleIsBold { get; set; } = false;
        public bool SubtitleIsItalic { get; set; } = false;

        // Logo / Watermark Settings (App Session Overlay)
        public string LogoPath { get; set; } = "";
        public int LogoOpacity { get; set; } = 255; // 0 (transparent) to 255 (opaque)
        public int LogoX { get; set; } = 10;        // Horizontal offset
        public int LogoY { get; set; } = 10;        // Vertical offset
        public double LogoScale { get; set; } = 1.0;

        // File Metadata & State
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";


        /// Current playback position in seconds

        public double CurrentDuration { get; set; }
        public double Volume { get; set; }

        /// Total length of the media in seconds

        public double TotalDuration { get; set; }

        [JsonIgnore]
        public BitmapImage? Thumbnail { get; set; }
    }
}
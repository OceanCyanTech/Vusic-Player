using Microsoft.UI.Xaml;

namespace Vusic_Player.Configuration.ClassModels
{
    public class SettingSearchResult
    {
        public string? Name { get; set; }        // e.g., "Aspect Ratio"
        public string[]? Keywords { get; set; }    // e.g., "width, height, 16:9, zoom"
        public int TabIndex { get; set; }       // 0 = Video, 1 = Audio, 2 = Subtitles
        public int SegmentIndex { get; set; }   // Index in the SelectorBar (0, 1, 2...)
        public FrameworkElement? TargetGrid { get; set; } // The actual Grid to scroll to
    }
}

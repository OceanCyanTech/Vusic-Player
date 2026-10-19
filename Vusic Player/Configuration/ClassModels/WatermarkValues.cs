using Microsoft.UI.Xaml;
using System;
using Vusic_Player.Configuration.Playback;
using Windows.UI.Text;

namespace Vusic_Player.Configuration.ClassModels
{
    public class WatermarkLabelSettingsModel
    {
        public Visibility IsVisible { get; set; } = Visibility.Collapsed; // Default to hidden until explicitly enabled
        public double FontSize { get; set; } = 36;
        public string HexColor { get; set; } = "#FFFFFFFF";
        public string FontFamily { get; set; } = "Segoe UI";
        public ushort FontWeight { get; set; } = 400;
        public FontStyle FontStyle { get; set; } = FontStyle.Normal;
        public TimeSpan Offset { get; set; } = TimeSpan.Zero;
        public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
        public WatermarkMode Mode { get; set; } = WatermarkMode.ElapsedPlaybackTime;
    }
    public class WatermarkSettingsModel
    {
        public WatermarkLabelSettingsModel Label1 { get; set; } = new();
        public WatermarkLabelSettingsModel Label2 { get; set; } = new();
        public WatermarkLabelSettingsModel Label3 { get; set; } = new();
        public WatermarkLabelSettingsModel Label4 { get; set; } = new();
        public WatermarkLabelSettingsModel Label5 { get; set; } = new();
        public WatermarkLabelSettingsModel Label6 { get; set; } = new();
        public WatermarkLabelSettingsModel Label7 { get; set; } = new();
        public WatermarkLabelSettingsModel Label8 { get; set; } = new();
        public WatermarkLabelSettingsModel Label9 { get; set; } = new();
    }
}

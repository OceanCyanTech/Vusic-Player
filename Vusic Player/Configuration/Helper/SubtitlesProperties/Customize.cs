using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Text;

namespace Vusic_Player.Configuration.Helper.SubtitlesProperties
{
    public class Customize
    {
        public static List<string> LoadFonts() // Removed Task<>
        {
            var fonts = CanvasTextFormat.GetSystemFontFamilies()
                        .OrderBy(f => f)
                        .ToList();
            return fonts;
        }
        public static event Action? OnSubtitleCustomizeRequest;
        public static HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center;
        public static VerticalAlignment verticalAlignment = VerticalAlignment.Bottom;
        public static Thickness thickness;
        public static Microsoft.UI.Xaml.Media.FontFamily fontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI Variable Display");
        public static double FontSize = 28;
        public static FontWeight FontWeight = FontWeights.Normal;
        public static FontStyle FontStyle = FontStyle.Normal;
        public static FontStretch FontStretch = FontStretch.Normal;

        public static Brush Foreground = new SolidColorBrush(Microsoft.UI.Colors.White);
        public static TextDecorations TextDecorations = TextDecorations.None;
        public static int CharacterSpacing = 0;
        public static TextAlignment TextAlignment = TextAlignment.Center;
        public static Style? style = null;

        public static void Call()
        {
            OnSubtitleCustomizeRequest?.Invoke();
        }

    }
}

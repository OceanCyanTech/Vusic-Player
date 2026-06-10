using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Vusic_Player.Configuration.Helper.Converters
{
    public class FavoriteToGlyphConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isFavorite = (bool)value;
            // Using standard Segoe Fluent Icons hex codes
            return isFavorite ? "\uEB52" : "\uEB51";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is string glyph)
            {
                return glyph == "\uEB52"; // Returns true if it matches the filled heart glyph
            }
            return false;
        }
    }

}

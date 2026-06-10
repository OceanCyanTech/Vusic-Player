using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;

namespace Vusic_Player.Configuration.Helper.Converters
{
    public class FavoriteToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            // The engine might be passing null or a DependencyProperty.UnsetValue during init
            if (value is bool isFavorite)
            {
                return isFavorite
                    ? new SolidColorBrush(Microsoft.UI.Colors.Red)
                    : new SolidColorBrush(Microsoft.UI.Colors.Gray);
            }

            // Fallback: Return a default brush instead of letting the binding fail
            return new SolidColorBrush(Microsoft.UI.Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value is SolidColorBrush brush)
            {
                // Check if the brush color matches Red
                return brush.Color == Microsoft.UI.Colors.Red;
            }

            return false; // Fallback
        }
    }

}

using System;
using Microsoft.UI.Xaml.Data;

namespace Vusic_Player.Configuration.Helper.Converters
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isFavorite)
            {
                return isFavorite ? 1.0 : 0.0;
            }
            return 0.0;
        }

        // Not needed for one-way binding
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}

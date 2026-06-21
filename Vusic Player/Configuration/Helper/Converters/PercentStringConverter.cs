using Microsoft.UI.Xaml.Data;
using System;

namespace Vusic_Player.Configuration.Helper.Converters
{
    public class PercentStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return $"{value}%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}

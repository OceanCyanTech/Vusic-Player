using Microsoft.UI.Xaml.Data;
using System;

namespace Vusic_Player.Configuration.Helper.Converters
{
    public class RoundedPercentStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && double.TryParse(value.ToString(), out double result))
            {

                return $"{result:0.##}%";
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => throw new NotImplementedException();
    }
}

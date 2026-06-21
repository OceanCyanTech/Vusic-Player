using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.Converters
{
    public class DoubleToFormatConverter : Microsoft.UI.Xaml.Data.IValueConverter
    {
        // From Slider to TextBox
        public object Convert(object value, Type targetType, object parameter, string language)
            => value is double d ? d.ToString("F2") : "0.00";

        // From TextBox to Slider
        public object ConvertBack(object value, Type targetType, object parameter, string language)
            => double.TryParse(value as string, out double result) ? result : 0.0;
    }
}

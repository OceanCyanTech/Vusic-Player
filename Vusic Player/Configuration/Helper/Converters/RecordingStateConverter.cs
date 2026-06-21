using Microsoft.UI.Xaml.Data;
using System;

namespace Vusic_Player.Configuration.Helper.Converters
{
    public class RecordingStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool isRecording = (bool)value;
            string type = parameter as string ?? "";

            if (type == "Icon")
                return isRecording ? "\uE7C8" : "\uE714"; // Stop icon vs Record icon

            return isRecording ? "Stop Recording" : "Start Recording";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) => null ?? string.Empty;
    }
}

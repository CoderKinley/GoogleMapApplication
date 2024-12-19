using System;
using System.Globalization;
using System.Windows.Data;


namespace mapAdvanced
{
    public class LastSegmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string input)
            {
                // Split the string by '>' and return the last segment (trimmed)
                var segments = input.Split(new[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                return segments.Length > 0 ? segments[segments.Length - 1].Trim() : input;
            }

            return value; // Return original value if it's not a string
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
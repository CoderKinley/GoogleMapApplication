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
                var segments = input.Split(new[] { '>' }, StringSplitOptions.RemoveEmptyEntries);
                return segments.Length > 0 ? segments[segments.Length - 1].Trim() : input;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
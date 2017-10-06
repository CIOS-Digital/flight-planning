using System;
using System.Globalization;
using System.Windows.Data;

namespace CIOSDigital.FlightPlanner.Converters
{
    public class StringFormatConverter : IValueConverter
    {
        public string FormatString { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return String.Format(this.FormatString, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

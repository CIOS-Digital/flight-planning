using System;
using System.Globalization;
using System.Windows.Data;

namespace CIOSDigital.FlightPlanner.Converters
{
    public class ProportionConverter : IValueConverter
    {
        public double Scale { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)(this.Scale * (double)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

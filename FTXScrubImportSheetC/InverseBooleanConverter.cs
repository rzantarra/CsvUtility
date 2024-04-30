using System;
using System.Globalization;
using System.Windows.Data;

namespace FTXScrubImportSheetC
{
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return !boolValue; // Return the inverse of the boolean value
            }

            return value; // Return the original value if it's not a boolean
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // ConvertBack is not used in this converter
        }
    }
}

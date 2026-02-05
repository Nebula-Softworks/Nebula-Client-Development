// this was from a open source application that i forgot the name of :sob:

using System;
using System.Globalization;
using System.Windows.Data;

namespace Celestia_IDE.Core.Editor
{
    public class EndsWithFinder : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string header && parameter is string suffix)
            {
                return header.EndsWith(suffix, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
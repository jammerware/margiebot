using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace MargieBot.Infrastructure.UIHelpers.ValueConverters
{
    public class ListOfStringToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Bazam.Modules.Listless.ListToString(value as IEnumerable<string>);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
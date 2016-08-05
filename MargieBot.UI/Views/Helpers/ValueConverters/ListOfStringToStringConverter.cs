using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using Bazam.Extensions;

namespace MargieBot.UI.Views.Helpers.ValueConverters
{
    public class ListOfStringToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as IEnumerable<string>).Concatenate("\n\n");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
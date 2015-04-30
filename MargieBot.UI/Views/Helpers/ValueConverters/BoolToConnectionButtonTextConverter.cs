using System;
using System.Globalization;
using System.Windows.Data;

namespace MargieBot.UI.Views.Helpers.ValueConverters
{
    public class BoolToConnectionButtonTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((bool)value) ? "Disconnect" : "Connect";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

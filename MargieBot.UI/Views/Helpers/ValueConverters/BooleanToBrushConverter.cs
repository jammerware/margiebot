using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MargieBot.UI.Views.Helpers.ValueConverters
{
    public class BooleanToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return App.Current.FindResource((bool)value ? "AccentBrush" : "SubtleBrush") as Brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

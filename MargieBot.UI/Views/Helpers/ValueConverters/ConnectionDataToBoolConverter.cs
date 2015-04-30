using MargieBot.Models;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MargieBot.UI.Views.Helpers.ValueConverters
{
    public class ConnectionDataToBoolConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values != null && values.Length > 2 && values[0] != DependencyProperty.UnsetValue) {
                return (bool)values[0] && !string.IsNullOrEmpty(values[1].ToString()) && (values[2] as SlackChatHub) != null;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
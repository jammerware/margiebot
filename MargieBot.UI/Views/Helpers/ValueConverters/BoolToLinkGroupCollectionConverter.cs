using FirstFloor.ModernUI.Presentation;
using System;
using System.Globalization;
using System.Windows.Data;

namespace MargieBot.UI.Views.Helpers.ValueConverters
{
    public class BoolToLinkGroupCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            LinkGroupCollection groups = new LinkGroupCollection();
            LinkGroup group = new LinkGroup() { DisplayName = "margiebot" };
            groups.Add(group);

            group.Links.Add(new Link() {
                DisplayName = "configure",
                Source = new Uri("/Views/ConfigureView.xaml", UriKind.Relative)
            });

            group.Links.Add(new Link() {
                DisplayName = "debug",
                Source = new Uri("/Views/DebugView.xaml", UriKind.Relative)
            });

            if ((bool)value) {
                group.Links.Add(new Link() {
                    DisplayName = "talk",
                    Source = new Uri("/Views/TalkView.xaml", UriKind.Relative)
                });
            }

            return groups;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
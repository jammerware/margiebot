using FirstFloor.ModernUI.Presentation;
using System.Windows;
using System.Windows.Media;

namespace MargieBot
{
    public partial class App : Application
    {
        private void this_Startup(object sender, StartupEventArgs e)
        {
            // set up MUI theme
            Color myAccentColor = (Color)App.Current.FindResource("MyAccentColor");
            AppearanceManager.Current.AccentColor = myAccentColor;
        }
    }
}

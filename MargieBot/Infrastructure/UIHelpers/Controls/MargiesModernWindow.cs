using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;

namespace MargieBot.Infrastructure.UIHelpers.Controls
{
    public class MargiesModernWindow : ModernWindow
    {
        public UIElement Widget
        {
            get { return (UIElement)GetValue(WidgetProperty); }
            set { SetValue(WidgetProperty, value); }
        }

        public static readonly DependencyProperty WidgetProperty = DependencyProperty.Register(
            "Widget", 
            typeof(UIElement), 
            typeof(MargiesModernWindow), 
            new PropertyMetadata(null)
        );       
    }
}
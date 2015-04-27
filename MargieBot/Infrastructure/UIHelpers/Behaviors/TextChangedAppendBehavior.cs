using System.Windows.Controls;
using System.Windows.Interactivity;

namespace MargieBot.Infrastructure.UIHelpers.Behaviors
{
    public class TextChangedAppendBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            AssociatedObject.TextChanged += (object sender, TextChangedEventArgs args) => {
                AssociatedObject.ScrollToEnd();
            };
        }
    }
}
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace Vusic_Player.UI.UserViews.Controls
{
    public class OceanButton : Button
    {
        private TranslateTransform? _gradientTransform;

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _gradientTransform = GetTemplateChild("GradientTransform") as TranslateTransform;

            if (_gradientTransform != null)
            {
                StartShimmer();
            }
        }

        private void StartShimmer()
        {
            var animation = new DoubleAnimation
            {
                From = -1,
                To = 1,
                Duration = new Duration(System.TimeSpan.FromSeconds(2)),
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(animation, _gradientTransform);
            Storyboard.SetTargetProperty(animation, "X");

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }
    }

}

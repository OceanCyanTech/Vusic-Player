using CommunityToolkit.WinUI.Helpers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Vusic_Player.Configuration.Playback;
using Vusic_Player.Configuration.UserSettings;
using Vusic_Player.MediaProperties.VideoProperties;
using Windows.UI;


namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoGeneral
{
    public sealed partial class View : UserControl
    {
        Color originalGlowColor;
        public static GlowService GlowInstance { get; } = new GlowService();
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;


        public string FormatSpeed(double value) => value.ToString("F1");
        public View()
        {
            InitializeComponent();
            chkGlow.Checked -= chkGlow_Checked;
            chkGlow.Checked += chkGlow_Checked;
            chkGlow.Unchecked -= chkGlow_Checked;
            chkGlow.Unchecked += chkGlow_Checked;
        }


        #region View and Glow Settings Events

        private void sldZoom_ValueChanged(double obj)
        {
            ZoomService.Set(obj);
        }

        private void btnResetZoom_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            sldZoom.Value = 100;
            ZoomService.Set(100);
        }

        private async void chkGlow_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            if (chkGlow.IsChecked == true)
            {
                currentSettings.GlowEffect = true;
                GlowService.Instance.GlowEffectVisibility = Visibility.Visible;
                tglMoreGlowOptions.IsEnabled = true;
                glowColorPicker.IsEnabled = true;
                btnChooseGlowColor.IsEnabled = true;
                switchMoving.IsEnabled = true;
                speedSlider.IsEnabled = true;

            }
            else
            {
                currentSettings.GlowEffect = false;
                tglMoreGlowOptions.IsEnabled = false;
                glowColorPicker.IsEnabled = false;
                btnChooseGlowColor.IsEnabled = false;
                switchMoving.IsEnabled = false;
                speedSlider.IsEnabled = false;
                GlowService.Instance.GlowEffectVisibility = Visibility.Collapsed;
            }
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }



        private void tglMoreGlowOptions_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            MoreGlowOptions();
        }
        private void MoreGlowOptions()
        {
            GlowSettingsPanel.Visibility = (tglMoreGlowOptions.IsChecked == true) ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
            DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () =>
            {
                var options = new BringIntoViewOptions
                {
                    VerticalAlignmentRatio = 0.0, // Force to the top
                    AnimationDesired = true
                };

                GlowSettingsPanel.StartBringIntoView(options);
            });
        }
        private void tglMoreGlowOptions_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            MoreGlowOptions();
        }

        private async void glowColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            GlowService.Instance.GlowColor = glowColorPicker.Color;
            rctColorGlow.Fill = new SolidColorBrush(args.NewColor);
            var currentSettings = await SettingsLoader.LoadSettingsAsync();
            currentSettings.GlowColorHex = glowColorPicker.Color.ToHex();
            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        private void switchRGB_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
        }

        private async void switchMoving_Toggled(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            var currentSettings = await SettingsLoader.LoadSettingsAsync();

            if (switchMoving.IsOn)
            {
                originalGlowColor = GlowService.Instance.GlowColor;
                GlowService.GlowSpeed = speedSlider.Value;
                GlowService.StartNeonGlowAnimation();
                currentSettings.GlowAnimation = true;

            }
            else
            {
                currentSettings.GlowAnimation = false;
                GlowService.StopAnimation(originalGlowColor);
            }

            await SettingsLoader.SaveSettingsAsync(currentSettings);
        }
        #endregion

        private void speedSlider_ValueChanged(double obj)
        {
            GlowService.GlowSpeed = speedSlider.Value;
        }
    }

}

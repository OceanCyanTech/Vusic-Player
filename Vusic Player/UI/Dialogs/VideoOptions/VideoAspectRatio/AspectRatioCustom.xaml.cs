using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Vusic_Player.Configuration;
using Vusic_Player.MediaProperties.VideoProperties;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoAspectRatio
{
    public sealed partial class AspectRatioCustom : UserControl
    {
        public AspectRatioCustom()
        {
            InitializeComponent();
        }
        public Aspect AspectValues => Aspect.Instance;

        #region Custom Aspect Ratio Events
        bool isUpdatingAR = false;

        private void numARWidth_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (PlayerService.Masterplayer == null) return;
            var currentaspectratio = PlayerService.Masterplayer.Config.Video.AspectRatio;
            if (LockRatioCheckBox.IsChecked == true)
            {
                if (isUpdatingAR == false)
                    LockAspectRatioBasedOnWidth();
            }
        }
        private void LockAspectRatioBasedOnWidth()
        {

            if (PlayerService.Masterplayer == null) return;
            if (RoundToIntegersCheckBox == null) return;
            isUpdatingAR = true;
            var currentaspectratio = PlayerService.Masterplayer.Video.AspectRatio;

            var width = currentaspectratio.Num;
            var height = currentaspectratio.Den;

            var lockheight = (height / width) * numARWidth.Value;
            var value = lockheight;
            if (RoundToIntegersCheckBox.IsChecked == true)
            {
                value = Math.Round(lockheight);
            }
            numARHeight.Value = value;
            isUpdatingAR = false;
        }
        private void numARHeight_ValueChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args)
        {
            if (PlayerService.Masterplayer == null) return;
            if (LockRatioCheckBox.IsChecked == true)
            {
                if (isUpdatingAR == false)
                    LockAspectRatioBasedOnHeight();
            }
        }
        private void LockAspectRatioBasedOnHeight()
        {
            if (PlayerService.Masterplayer == null) return;
            isUpdatingAR = true;
            var currentaspectratio = PlayerService.Masterplayer.Video.AspectRatio;

            var width = currentaspectratio.Num;
            var height = currentaspectratio.Den;
            var old = height / width;
            var lockheight = numARHeight.Value / old;
            var value = lockheight;
            if (RoundToIntegersCheckBox.IsChecked == true)
            {
                value = Math.Round(lockheight);
            }
            numARWidth.Value = value;
            isUpdatingAR = false;
        }

        private void LockRatioCheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            LockRatio();
        }
        private void LockRatio()
        {
            var originalwidth = numARWidth.Value;
            var originalheight = numARHeight.Value;


            if (LockRatioCheckBox.IsChecked == true)
            {
                LockAspectRatioBasedOnWidth();
            }
            else
            {
                numARHeight.Value = originalheight;
                numARWidth.Value = originalwidth;
            }
        }
        private void LockRatioCheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            LockRatio();
        }
        private void Roundoff()
        {
            var originalwidth = numARWidth.Value;
            var originalheight = numARHeight.Value;
            if (RoundToIntegersCheckBox.IsChecked == true)
            {
                numARHeight.Value = Math.Round(numARHeight.Value);
                numARWidth.Value = Math.Round(numARWidth.Value);
            }
            else
            {
                numARHeight.Value = originalheight;
                numARWidth.Value = originalwidth;
            }
        }
        private void RoundToIntegersCheckBox_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Roundoff();
        }

        private void RoundToIntegersCheckBox_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Roundoff();
        }

        private void btnSetCustomRatio_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (PlayerService.Masterplayer == null) return;
            var AspectRatioCustom = new FlyleafLib.AspectRatio();
            AspectRatioCustom.Num = numARWidth.Value;
            AspectRatioCustom.Den = numARHeight.Value;
            string ratio = AspectRatioCustom.ValueStr;
            string label = ratio switch
            {
                "1:1" or "4:3" or "16:9" or "21:9" => ratio,
                _ => $"{ratio} (custom)"
            };

            AspectValues.AspectDisplay = label;
            Aspect.SetAspectRatio(AspectRatioCustom);
        }

        #endregion

    }
}

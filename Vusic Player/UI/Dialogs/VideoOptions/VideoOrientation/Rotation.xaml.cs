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
using Orientation = Vusic_Player.MediaProperties.VideoProperties.Orientation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoOrientation
{
    public sealed partial class Rotation : UserControl
    {
        public Rotation()
        {
            InitializeComponent();
        }
        public static readonly DependencyProperty RotationAngleProperty =
DependencyProperty.Register("RotationAngle", typeof(double), typeof(MainWindow), new PropertyMetadata(0.0));

        public double RotationAngle
        {
            get => (double)GetValue(RotationAngleProperty);
            set => SetValue(RotationAngleProperty, value);
        }
        #region Video Rotation Events

        private void Dial_PointerPressed(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            isDraggingRot = true;
            ((FrameworkElement)sender).CapturePointer(e.Pointer);
            UpdateDialAngle(e);
        }

        private void Dial_PointerMoved(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (isDraggingRot)
            {
                UpdateDialAngle(e);
            }
        }
        private void UpdateDialAngle(PointerRoutedEventArgs e)
        {
            var grid = DialControl;
            var pointerPoint = e.GetCurrentPoint(grid);

            double centerX = grid.ActualWidth / 2;
            double centerY = grid.ActualHeight / 2;

            // 2. Calculate the difference between the mouse and the center
            double deltaX = pointerPoint.Position.X - centerX;
            double deltaY = pointerPoint.Position.Y - centerY;

            // 3. Get the angle in Radians and convert to Degrees
            // Math.Atan2 returns 0 at the 3 o'clock position, so we add 90 
            // to make 0 degrees start at the 12 o'clock position.
            double radians = Math.Atan2(deltaY, deltaX);
            double degrees = radians * (180 / Math.PI) + 90;

            // 4. Normalize the value to a 0-360 range
            if (degrees < 0) degrees += 360;
            if (degrees >= 360) degrees -= 360;

            double snapThreshold = 5.0;
            if (degrees % 90 < snapThreshold || degrees % 90 > (90 - snapThreshold))
            {
                degrees = Math.Round(degrees / 90.0) * 90;
                // Correct 360 back to 0 for consistency
                if (degrees == 360) degrees = 0;
            }

            // 6. Update the Dependency Property
            // This triggers the {x:Bind} for the Needle and Slider automatically
            RotationAngle = Math.Round(degrees);
            sldRotation.Value = RotationAngle;
            if (PlayerService.Masterplayer != null)
            {
                if (RotationAngle == 0 || RotationAngle == 90 || RotationAngle == 180 || RotationAngle == 270)
                {
                    Orientation.Instance.AngleRotation = 0;
                    // You must choose ONE specific value to assign here
                    PlayerService.Masterplayer.Config.Video.Rotation = (uint)RotationAngle;
                }
                else
                {
                    PlayerService.Masterplayer.Config.Video.Rotation = 0;
                    Orientation.Instance.AngleRotation = RotationAngle;
                }
            }
            // Manual update to ensure the needle moves immediately
            NeedleRotation.Angle = RotationAngle;
            txtRotationValue.Text = $"{RotationAngle}°";
            sldRotation.Value = RotationAngle;
        }
        bool isDraggingRot = false;
        private void Dial_PointerReleased(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            isDraggingRot = false;
            ((FrameworkElement)sender).ReleasePointerCapture(e.Pointer);
        }

        private void sldRotation_ValueChanged(double obj)
        {
            Orientation.Instance.AngleRotation = obj;
            UpdateManualRotation(obj);
        }
        private void UpdateManualRotation(double angle)
        {
            RotationAngle = angle;
            txtRotationValue.Text = $"{RotationAngle.ToString("0.00")}°";
            if (PlayerService.Masterplayer != null)
            {
                if (RotationAngle == 0 || RotationAngle == 90 || RotationAngle == 180 || RotationAngle == 270)
                {
                    Orientation.Instance.AngleRotation = 0;
                    PlayerService.Masterplayer.Config.Video.Rotation = (uint)RotationAngle;
                }
                else
                {
                    PlayerService.Masterplayer.Config.Video.Rotation = 0;
                    Orientation.Instance.AngleRotation = RotationAngle;
                }
            }
            sldRotation.Value = RotationAngle;
        }
        private void btnResetRotation_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            UpdateManualRotation(0);
        }

        private void BtnRotateMinus_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            double newAngle = RotationAngle - 1;
            if (newAngle < 0) newAngle = 359;

            UpdateManualRotation(newAngle);
        }

        private void BtnRotatePlus_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            double newAngle = RotationAngle + 1;
            if (newAngle >= 360) newAngle = 0;

            UpdateManualRotation(newAngle);
        }

        #endregion
    }

}

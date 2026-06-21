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
using Vusic_Player.MediaProperties.VideoProperties;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Orientation = Vusic_Player.MediaProperties.VideoProperties.Orientation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoOrientation
{
    public sealed partial class Flip : UserControl
    {
        public Flip()
        {
            InitializeComponent();
        }
        #region Mirroring and Flip Events

        private void btnFlipHorizontal_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Orientation.Flip(FlipOrientation.Horizontal, btnFlipHorizontal.IsChecked ?? false);
        }

        private void btnFlipHorizontal_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Orientation.Flip(FlipOrientation.Horizontal, btnFlipHorizontal.IsChecked ?? false);
        }

        private void btnFlipVertical_Checked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Orientation.Flip(FlipOrientation.Vertical, btnFlipVertical.IsChecked ?? false);
        }

        private void btnFlipVertical_Unchecked(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Orientation.Flip(FlipOrientation.Vertical, btnFlipVertical.IsChecked ?? false);
        }

        #endregion
    }
}

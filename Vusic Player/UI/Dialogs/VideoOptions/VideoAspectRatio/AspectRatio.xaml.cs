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

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.Dialogs.VideoOptions.VideoAspectRatio
{
    public sealed partial class AspectRatio : UserControl
    {
        private bool _isUpdating = false;
        public Aspect AspectValues => Aspect.Instance;

        public AspectRatio()
        {
            InitializeComponent();
        }

        private void btnDefaultRatio_Click(object sender, RoutedEventArgs e)
        {
            Aspect.SetDefault();

        }
        #region Aspect Ratio Preset Events

        private void Preset_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Content is string ratio)
            {
                var parts = ratio.Split(':');
                if (parts.Length == 2)
                {
                    _isUpdating = true;
                    Aspect.Instance.Width = double.Parse(parts[0]);
                    Aspect.Instance.Height = double.Parse(parts[1]);
                }

                _isUpdating = false;
                Aspect.SetAspectRatio(ratio);

                AspectValues.AspectDisplay = ratio;
            }

        #endregion
        }
    }

}

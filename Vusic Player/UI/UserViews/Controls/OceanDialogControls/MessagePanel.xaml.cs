using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using Vusic_Player.Configuration.Helper.UI.Customization;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls.OceanDialogControls
{
    public sealed partial class MessagePanel : UserControl
    {
        public MessagePanel()
        {
            InitializeComponent();
        }
        public void UpdateImage(string image)
        {
            imgSeverity.Source = new BitmapImage(new Uri($"ms-appx:///Assets/Themes/{ThemeManager.Instance.GlobalTheme}/{image}.png"));
        }
        public void UpdateMessage(string Message)
        {
            txtMessage.Text = Message;
        }
    }
}

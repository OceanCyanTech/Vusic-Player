using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.Playback;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Controls
{
    public sealed partial class PlaybackMainControls : UserControl
    {
        public MediaPlaybackController mediacontroller => MediaPlaybackController.Instance;
        public Visibility FileNameTextVisibility
        {
            get => (Visibility)GetValue(filenametextvis);
            set => SetValue(filenametextvis, value);
        }
        public static readonly DependencyProperty filenametextvis =
    DependencyProperty.Register(
        nameof(FileNameTextVisibility),
        typeof(Visibility),
        typeof(PlaybackMainControls),
        new PropertyMetadata(Visibility.Visible));

        public PlaybackMainControls()
        {
            InitializeComponent();
        }
        private void sldMain_DragStarted()
        {
            PlayerService.SldMain_DragStarted();
        }

        private void sldMain_DragCompleted()
        {
            PlayerService.SldMain_DragCompleted(sldMain);
        }

    }
}

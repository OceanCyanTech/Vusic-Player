using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids;

public sealed partial class SetAppLock : UserControl
{
    public SetAppLock()
    {
        InitializeComponent();
    }
    int index = 0;
    int pass1 = 0;
    int pass2 = 0;
    int pass3= 0;
    int pass4= 0;
    private void Grid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if(e.Key == Windows.System.VirtualKey.Number1 ||
            e.Key == Windows.System.VirtualKey.Number2 ||
            e.Key == Windows.System.VirtualKey.Number3 ||
            e.Key == Windows.System.VirtualKey.Number4 ||
            e.Key == Windows.System.VirtualKey.Number5 ||
            e.Key == Windows.System.VirtualKey.Number6 ||
            e.Key == Windows.System.VirtualKey.Number7 ||
            e.Key == Windows.System.VirtualKey.Number8 ||
            e.Key == Windows.System.VirtualKey.Number9 ||
            e.Key == Windows.System.VirtualKey.Number0)
        {
            switch (index)
            {
                case 0:
                    pass1 = (e.Key - Windows.System.VirtualKey.Number0);
                    txtNum1.Text = "•";
                    index++;
                    break;

                case 1:
                    pass2 = (e.Key - Windows.System.VirtualKey.Number0);
                    txtNum2.Text = "•";
                    index++;
                    break;


                case 2:
                    pass3 = (e.Key - Windows.System.VirtualKey.Number0);
                    txtNum3.Text = "•";
                    index++;
                    break;

                case 3:
                    pass4 = (e.Key - Windows.System.VirtualKey.Number0);
                    txtNum4.Text = "•"; 
                    break;
            }
        }
        else if(e.Key == Windows.System.VirtualKey.Back)
        {
            switch (index)
            {
                case 0:
                    pass1 = 0;
                    txtNum1.Text = "";
                    break;

                case 1:
                    pass2 = 0;
                    txtNum2.Text = "";
                    index--;
                    break;


                case 2:
                    pass3 = 0;
                    txtNum3.Text = "";
                    index--;
                    break;

                case 3:
                    pass4 = 0;
                    txtNum4.Text = "";
                    index--;
                    break;
            }
        }
    }
}

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

namespace Vusic_Player.UI.UserViews.Controls;

public sealed partial class KeyboardShortcutsGuide : UserControl
{
    public KeyboardShortcutsGuide()
    {
        InitializeComponent();
    }

    private void selBarKeyboardShortcuts_SelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
    
    }

    private void selBarKeyboardShortcuts_SelectionChanged_1(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItemContainer == null) return;
        imgKeyboardShortcut1.Visibility = Visibility.Collapsed;
        imgKeyboardShortcut2.Visibility = Visibility.Collapsed;
        imgKeyboardShortcut3.Visibility = Visibility.Collapsed;
        if (args.SelectedItemContainer == selBar1)
        {
            imgKeyboardShortcut1.Visibility = Visibility.Visible;
        }
        else if (args.SelectedItemContainer == selBar2)
        {
            imgKeyboardShortcut2.Visibility = Visibility.Visible;

        }
        else if (args.SelectedItemContainer == selBar3)
        {
            imgKeyboardShortcut3.Visibility = Visibility.Visible;

        }
    }
}

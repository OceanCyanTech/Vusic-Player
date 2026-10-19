using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.Windows.AppLifecycle;
using System;
using System.Diagnostics;
using Vusic_Player.Configuration;
using Vusic_Player.Configuration.UserSettings;
using Windows.ApplicationModel.Core;
using Windows.System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Authentication
{
    public sealed partial class AppAuthVerify : UserControl
    {
        public AppAuthVerify()
        {
            InitializeComponent();
            password = PinService.RetrievePin();
            if (password != null)
            {
                noofdigits = password.Length;
                if (noofdigits == 6)
                {
                    txtNum5.Visibility = Visibility.Visible;
                    txtNum6.Visibility = Visibility.Visible;
                }
                else
                {
                    txtNum5.Visibility = Visibility.Collapsed;
                    txtNum6.Visibility = Visibility.Collapsed;

                }
            }

        }
        public void SetNoOfDigits()
        {
            password = PinService.RetrievePin();
            if (password != null)
            {
                noofdigits = password.Length;
                if (noofdigits == 6)
                {
                    txtNum5.Visibility = Visibility.Visible;
                    txtNum6.Visibility = Visibility.Visible;
                }
                else
                {
                    txtNum5.Visibility = Visibility.Collapsed;
                    txtNum6.Visibility = Visibility.Collapsed;

                }
            }
        }
        public event Action? AuthenticationSuccess;
        public void BringFocus(string passwordmain)
        {
            txtNum1.Focus(FocusState.Programmatic);

            password = passwordmain;
        }
        private int noofdigits = 4;
        private string? password = "";
        private readonly int[] pass = new int[6];
        private int index = 0;
        private void Grid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            txtIncorrectPassword.Text = "";
            int digit = -1;
            if (e.Key >= VirtualKey.Number0 && e.Key <= VirtualKey.Number9)
            {
                digit = (int)(e.Key - VirtualKey.Number0);
            }
            else if (e.Key >= VirtualKey.NumberPad0 && e.Key <= VirtualKey.NumberPad9)
            {
                digit = (int)(e.Key - VirtualKey.NumberPad0);
            }
            Debug.WriteLine("MAX DIGITIS " + noofdigits);
            Debug.WriteLine("DIGIT " + digit);
            int maxDigits = noofdigits;
            TextBox[] textBoxes = [txtNum1, txtNum2, txtNum3, txtNum4, txtNum5, txtNum6];

            // Handle Number Input
            if (digit != -1)
            {
                e.Handled = true; // Stop event bubbling to eliminate input latency

                if (index < maxDigits)
                {
                    pass[index] = digit;
                    textBoxes[index].Text = "•";
                    index++;

                    if (index == maxDigits)
                    {
                        Debug.WriteLine("YES TRUE: " + pass);
                        if (PinService.VerifyPin(pass, maxDigits))
                        {
                            AuthenticationSuccess?.Invoke();
                            Debug.WriteLine("PASS: " + pass);
                            Debug.WriteLine("Success");
                        }
                        else
                        {
                            ShakeStoryboard.Begin();
                            txtIncorrectPassword.Text = "Incorrect Password. Try again!";
                        }
                    }
                }
            }
            // Handle Backspace
            else if (e.Key == VirtualKey.Back)
            {
                e.Handled = true;

                if (index > 0)
                {
                    index--;
                    pass[index] = 0;
                    textBoxes[index].Text = "";
                }
            }


        }

        private void hypForgotPin_Click(object sender, RoutedEventArgs e)
        {
            grdVerify.Visibility = Visibility.Collapsed;
            grdForgotPin.Visibility = Visibility.Visible;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            grdVerify.Visibility = Visibility.Visible;
            grdForgotPin.Visibility = Visibility.Collapsed;
        }
        public void ClearAll()
        {
            txtNum1.Text = "";
            txtNum2.Text = "";
            txtNum3.Text = "";
            txtNum4.Text = "";
            txtNum5.Text = "";
            txtNum6.Text = "";
        }
        private async void btnConfirmReset_Click(object sender, RoutedEventArgs e)
        {
            SettingsFunctions.ResetAllSettings();
            PinService.RemovePin();
            AppRestartFailureReason result = AppInstance.Restart(string.Empty);

            // If restart succeeds, the current process terminates automatically.
            // If it fails, inspect the failure reason:
            switch (result)
            {
                case AppRestartFailureReason.RestartPending:
                    // A restart request is already processing
                    break;
                case AppRestartFailureReason.InvalidUser:
                    // The user token is invalid
                    break;
                case AppRestartFailureReason.Other:
                    // General failure, handle fallback
                    break;
            }
        }
    }
}

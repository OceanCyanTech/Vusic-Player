using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Diagnostics;
using Vusic_Player.Configuration;
using Vusic_Player.UI.Dialogs.OceanDialogConfig;
using Windows.System;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Vusic_Player.UI.UserViews.Grids;

public sealed partial class SetAppLock : UserControl
{
    public SetAppLock()
    {
        InitializeComponent();
        if (PinService.IsPinEnabled())
        {
            grdMain.Visibility = Visibility.Collapsed;
            grdDashboard.Visibility = Visibility.Visible;
        }
    }
    private readonly int[] pass = new int[6];
    private readonly int[] confirmPass = new int[6];
    private int index = 0;
    private async void Grid_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (grdMain.Visibility == Visibility.Visible)
        {
            int digit = -1;
            if (e.Key >= VirtualKey.Number0 && e.Key <= VirtualKey.Number9)
            {
                digit = (int)(e.Key - VirtualKey.Number0);
            }
            else if (e.Key >= VirtualKey.NumberPad0 && e.Key <= VirtualKey.NumberPad9)
            {
                digit = (int)(e.Key - VirtualKey.NumberPad0);
            }

            int maxDigits = (chkIsSixDigit.IsChecked == true) ? 6 : 4;
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
                        btnNext.IsEnabled = true;

                        // Verification check if in confirmation mode
                        if (chkIsSixDigit.IsEnabled == false)
                        {
                            ValidatePinMatch(maxDigits);
                        }
                    }
                }
            }
            // Handle Backspace
            else if (e.Key == VirtualKey.Back)
            {
                e.Handled = true;
                btnSet.Visibility = Visibility.Collapsed;
                txtConfirmPassword.Text = "Confirm Password";
                btnNext.IsEnabled = false;

                if (index > 0)
                {
                    index--;
                    pass[index] = 0;
                    textBoxes[index].Text = "";
                }
            }
        }
    }
    private void ValidatePinMatch(int length)
    {
        bool isMatch = true;
        for (int i = 0; i < length; i++)
        {
            if (pass[i] != confirmPass[i])
            {
                isMatch = false;
                break;
            }
        }

        if (isMatch)
        {
            txtConfirmPassword.Text = "Confirm Password";

            btnSet.Visibility = Visibility.Visible;
            btnSet.Focus(FocusState.Programmatic);

        }
        else
        {
            btnSet.Visibility = Visibility.Collapsed;

            txtConfirmPassword.Text = "Confirm Password - Passwords Don't Match";
            ShakeStoryboard.Begin();
        }
    }
    private async void btnUseWindowsAuthentication_Click(object sender, RoutedEventArgs e)
    {
        IntPtr hwnd = WindowNative.GetWindowHandle(App.OceanDialogInstance);

        bool verified = await AppLockAuth.VerifyCurrentUserAsync(
           hwnd,
           "Verify your identity to configure App Lock for Vusic Player."
       );

        if (verified)
        {
            Debug.WriteLine("THANK YOU");
            PinService.SaveWindowsAuthBool();
            grdMain.Visibility = Visibility.Collapsed;
            PinService.RemovePin();
            grdWinAuth.Visibility = Visibility.Visible;
        }
        else
        {
            Debug.WriteLine("Verification failed, cancelled, or unavailable.");
        }
    }

    private void btnNext_Click(object sender, RoutedEventArgs e)
    {
        chkIsSixDigit.IsEnabled = false;

        // Copy entered digits into confirmPass
        Array.Copy(pass, confirmPass, pass.Length);
        Array.Clear(pass, 0, pass.Length);

        // Clear all textboxes
        TextBox[] textBoxes = [txtNum1, txtNum2, txtNum3, txtNum4, txtNum5, txtNum6];
        foreach (var tb in textBoxes)
        {
            tb.Text = "";
        }

        index = 0;
        txtConfirmPassword.Visibility = Visibility.Visible;
        btnNext.Visibility = Visibility.Collapsed;
        btnBack.Visibility = Visibility.Visible;
    }
    private void chkIsSixDigit_Checked(object sender, RoutedEventArgs e)
    {
        if (chkIsSixDigit.IsChecked == true)
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

    private void btnBack_Click(object sender, RoutedEventArgs e)
    {
        btnNext.Visibility = Visibility.Visible;
        btnBack.Visibility = Visibility.Collapsed;
        txtConfirmPassword.Visibility = Visibility.Collapsed;
        index = 3;

        if (chkIsSixDigit.IsChecked == true)
        {
            index = 5;
        }
        chkIsSixDigit.IsEnabled = true;

        txtNum1.Text = "•";
        txtNum2.Text = "•";
        txtNum3.Text = "•";
        txtNum4.Text = "•";
        if (chkIsSixDigit.IsChecked == true)
        {
            txtNum5.Text = "•";
            txtNum6.Text = "•";
        }
    }

    private void btnGoBack_Click(object sender, RoutedEventArgs e)
    {
        grdMain.Visibility = Visibility.Visible;
        txtNum1.Focus(FocusState.Programmatic);
        grdConfirmation.Visibility = Visibility.Collapsed;
    }

    private void btnProceed_Click(object sender, RoutedEventArgs e)
    {
        if (chkIsSixDigit.IsChecked == false)
        {
            PinService.SavePin(pass, 4);


        }
        else
        {
            PinService.SavePin(pass, 6);

        }
        grdSuccess.Visibility = Visibility.Visible;
        grdConfirmation.Visibility = Visibility.Collapsed;
    }

    private void btnSet_Click(object sender, RoutedEventArgs e)
    {
        grdMain.Visibility = Visibility.Collapsed;
        grdConfirmation.Visibility = Visibility.Visible;
        btnProceed.Focus(FocusState.Programmatic);

    }

    private void btnRemovePassword_Click(object sender, RoutedEventArgs e)
    {
        grdSuccess.Visibility = Visibility.Collapsed;
        grdRemove.Visibility = Visibility.Visible;
    }

    private void btnChangePassword_Click(object sender, RoutedEventArgs e)
    {
        grdSuccess.Visibility = Visibility.Collapsed;
        grdVerify.Visibility = Visibility.Visible;
        string? password = PinService.RetrievePin();
        AppAuthVer.SetNoOfDigits();

        AppAuthVer.BringFocus(password!);
        AppAuthVer.AuthenticationSuccess += (() =>
        {
            PinService.RemovePin();
            grdVerify.Visibility = Visibility.Collapsed;
            grdMain.Visibility = Visibility.Visible;
            txtNum1.Focus(FocusState.Programmatic);

            btnSet.Visibility = Visibility.Collapsed;
            btnBack.Visibility = Visibility.Collapsed;
            btnNext.Visibility = Visibility.Visible;
            chkIsSixDigit.IsEnabled = true;
            txtNum1.Text = "";
            txtNum2.Text = "";
            txtNum3.Text = "";
            txtNum4.Text = "";
            txtNum5.Text = "";
            txtNum6.Text = "";
            txtConfirmPassword.Text = "";
            btnNext.IsEnabled = true;
        });
    }
    public void ClearAll()
    {
        AppAuthVer.ClearAll();
        txtNum1.Text = "";
        txtNum2.Text = "";
        txtNum3.Text = "";
        txtNum4.Text = "";
        txtNum5.Text = "";
        txtNum6.Text = "";
    }
    private void btnDone_Click(object sender, RoutedEventArgs e)
    {
        OceanContentDialog.HideDlg();
        MainWindow.ShowWindow();
        grdDashboard.Visibility = Visibility.Visible;
        grdSuccess.Visibility = Visibility.Collapsed;
    }
    public async void ShowDashboard()
    {

        grdDashboard.Visibility = Visibility.Collapsed;
        grdSuccess.Visibility = Visibility.Collapsed;
        grdRemove.Visibility = Visibility.Collapsed;
        grdMain.Visibility = Visibility.Visible;
        txtNum1.Focus(FocusState.Programmatic);

        grdConfirmation.Visibility = Visibility.Collapsed;
        grdVerify.Visibility = Visibility.Collapsed;


        if (PinService.IsPinEnabled())
        {
            grdMain.Visibility = Visibility.Collapsed;
            grdDashboard.Visibility = Visibility.Visible;

        }
        else
        {
            if (PinService.IsWindowsAuthEnabled())
            {
                grdDashboard.Visibility = Visibility.Collapsed;
                grdMain.Visibility = Visibility.Collapsed;
                IntPtr hwnd = WindowNative.GetWindowHandle(App.OceanDialogInstance);

                bool verified = await AppLockAuth.VerifyCurrentUserAsync(
                   hwnd,
                   "Verify your identity to configure App Lock for Vusic Player."
               );

                if (verified)
                {
                    grdDashboard.Visibility = Visibility.Collapsed;
                    grdMain.Visibility = Visibility.Collapsed;
                    grdWinAuth.Visibility = Visibility.Visible;
                }
            }
        }
    }

    private void btnNo_Click(object sender, RoutedEventArgs e)
    {
        grdRemove.Visibility = Visibility.Collapsed;
        grdSuccess.Visibility = Visibility.Visible;
    }

    private void btnYes_Click(object sender, RoutedEventArgs e)
    {
        grdRemove.Visibility = Visibility.Collapsed;
        grdVerify.Visibility = Visibility.Visible;
        string? password = PinService.RetrievePin();
        AppAuthVer.SetNoOfDigits();
        AppAuthVer.BringFocus(password!);
        AppAuthVer.AuthenticationSuccess += (() =>
        {
            PinService.RemovePin();
            grdVerify.Visibility = Visibility.Collapsed;
            grdMain.Visibility = Visibility.Visible;
            txtNum1.Focus(FocusState.Programmatic);

            btnSet.Visibility = Visibility.Collapsed;
            btnBack.Visibility = Visibility.Collapsed;
            btnNext.Visibility = Visibility.Visible;
            chkIsSixDigit.IsEnabled = true;
            txtNum1.Text = "";
            txtNum2.Text = "";
            txtNum3.Text = "";
            txtNum4.Text = "";
            txtNum5.Text = "";
            txtNum6.Text = "";
            txtConfirmPassword.Text = "";
            btnNext.IsEnabled = true;
        });
    }

    private void btnChangeCurrentPin_Click(object sender, RoutedEventArgs e)
    {
        grdDashboard.Visibility = Visibility.Collapsed;
        grdVerify.Visibility = Visibility.Visible;
        AppAuthVer.SetNoOfDigits();

        string? password = PinService.RetrievePin();
        AppAuthVer.BringFocus(password!);
        AppAuthVer.AuthenticationSuccess += (() =>
        {
            PinService.RemovePin();
            grdVerify.Visibility = Visibility.Collapsed;
            grdMain.Visibility = Visibility.Visible;
            txtNum1.Focus(FocusState.Programmatic);

            btnSet.Visibility = Visibility.Collapsed;
            btnBack.Visibility = Visibility.Collapsed;
            btnNext.Visibility = Visibility.Visible;
            chkIsSixDigit.IsEnabled = true;
            txtNum1.Text = "";
            txtNum2.Text = "";
            txtNum3.Text = "";
            txtNum4.Text = "";
            txtNum5.Text = "";
            txtNum6.Text = "";
            txtConfirmPassword.Text = "";
            btnNext.IsEnabled = true;
        });
    }

    private void btnRemoveCurrentPin_Click(object sender, RoutedEventArgs e)
    {
        grdDashboard.Visibility = Visibility.Collapsed;
        grdVerify.Visibility = Visibility.Visible;
        AppAuthVer.SetNoOfDigits();

        string? password = PinService.RetrievePin();
        AppAuthVer.BringFocus(password!);
        AppAuthVer.AuthenticationSuccess += (() =>
        {
            PinService.RemovePin();
            grdVerify.Visibility = Visibility.Collapsed;
            grdMain.Visibility = Visibility.Visible;
            txtNum1.Focus(FocusState.Programmatic);

            btnSet.Visibility = Visibility.Collapsed;
            btnBack.Visibility = Visibility.Collapsed;
            btnNext.Visibility = Visibility.Visible;
            chkIsSixDigit.IsEnabled = true;
            txtNum1.Text = "";
            txtNum2.Text = "";
            txtNum3.Text = "";
            txtNum4.Text = "";
            txtNum5.Text = "";
            txtNum6.Text = "";
            txtConfirmPassword.Text = "";
            btnNext.IsEnabled = true;
        });
    }

    private async void btnRemoveAuthentication_Click(object sender, RoutedEventArgs e)
    {
        IntPtr hwnd = WindowNative.GetWindowHandle(App.OceanDialogInstance);

        bool verified = await AppLockAuth.VerifyCurrentUserAsync(
           hwnd,
           "Verify your identity to configure App Lock for Vusic Player."
       );

        if (verified)
        {
            PinService.RemoveWindowsAuthentication();
            grdMain.Visibility = Visibility.Visible;
            grdWinAuth.Visibility = Visibility.Collapsed;
        }
    }
}

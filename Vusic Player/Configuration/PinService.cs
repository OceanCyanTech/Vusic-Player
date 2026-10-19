using System.Text;
using Windows.Security.Credentials;

namespace Vusic_Player.Configuration
{
    public static class PinService
    {
        public static string IntToString(int[] digits, int length)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++)
            {
                sb.Append(digits[i]);
            }
            return sb.ToString();
        }
        public static void SavePin(int[] digits, int length)
        {
            var vault = new PasswordVault();

            string pin = IntToString(digits, length);
            vault.Add(new PasswordCredential("VusicPlayer_AppPin", "OceanCyanMasterPin", pin));
        }
        public static void SaveWindowsAuthBool()
        {
            var vault = new PasswordVault();

            vault.Add(new PasswordCredential("VusicPlayer_AppWindowsAuth", "OceanCyanMasterPin", "1"));
        }
        public static void RemoveWindowsAuthentication()
        {
            var vault = new PasswordVault();

            try
            {
                var credential = vault.Retrieve("VusicPlayer_AppWindowsAuth", "OceanCyanMasterPin");
                vault.Remove(credential);
            }
            catch
            {

            }
        }

        public static void RemovePin()
        {
            var vault = new PasswordVault();

            try
            {
                var credential = vault.Retrieve("VusicPlayer_AppPin", "OceanCyanMasterPin");
                vault.Remove(credential);
            }
            catch
            {

            }
        }
        public static bool VerifyPin(int[] enteredDigits, int length)
        {
            var vault = new PasswordVault();

            try
            {
                var credential = vault.Retrieve("VusicPlayer_AppPin", "OceanCyanMasterPin");
                credential.RetrievePassword(); // Loads password into credential.Password

                string enteredPin = IntToString(enteredDigits, length);
                return credential.Password == enteredPin;
            }
            catch
            {
                // Throws an exception if the credential does not exist
                return false;
            }
        }
        public static string? RetrievePin()
        {
            var vault = new PasswordVault();
            try
            {
                var cred = vault.Retrieve("VusicPlayer_AppPin", "OceanCyanMasterPin");
                cred.RetrievePassword();
                return cred.Password;
            }
            catch
            {
                return null;
            }
        }
        public static bool IsPinEnabled()
        {
            var vault = new PasswordVault();
            try
            {
                var cred = vault.Retrieve("VusicPlayer_AppPin", "OceanCyanMasterPin");
                return cred != null;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsWindowsAuthEnabled()
        {
            var vault = new PasswordVault();
            try
            {
                var cred = vault.Retrieve("VusicPlayer_AppWindowsAuth", "OceanCyanMasterPin");
                return cred != null;
            }
            catch
            {
                return false;
            }
        }

    }
}

using System;
using System.Threading.Tasks;
using Windows.Security.Credentials.UI;

namespace Vusic_Player.Configuration
{


    public static class AppLockAuth
    {
        public static async Task<bool> VerifyCurrentUserAsync(IntPtr hwnd, string promptMessage)
        {
            // 1. Check if the device has Windows Hello or credentials set up
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();
            if (availability != UserConsentVerifierAvailability.Available)
            {
                return false;
            }

            // 2. Call the built-in CsWinRT interop helper directly
            UserConsentVerificationResult result =
                await UserConsentVerifierInterop.RequestVerificationForWindowAsync(hwnd, promptMessage);

            return result == UserConsentVerificationResult.Verified;
        }
    }
}

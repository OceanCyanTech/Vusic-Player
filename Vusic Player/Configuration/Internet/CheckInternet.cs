using System;
using Windows.Networking.Connectivity;

namespace Vusic_Player.Configuration.Internet
{
    public class CheckInternet
    {
        public static event Action? SetImage;
        public static void CallImageSet()
        {
            SetImage?.Invoke();
        }
        public static string UrlToDownload = "";
        public static bool IsInternetAvailable()
        {
            var profile = NetworkInformation.GetInternetConnectionProfile();
            return profile != null &&
                   profile.GetNetworkConnectivityLevel() ==
                   NetworkConnectivityLevel.InternetAccess;
        }
    }

}

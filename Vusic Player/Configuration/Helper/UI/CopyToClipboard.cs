using Windows.ApplicationModel.DataTransfer;

namespace Vusic_Player.Configuration.Helper.UI
{
    public class CopyToClipboard
    {
        public static void CopyStringToClipboard(string data)
        {
            var package = new DataPackage();
            package.SetText(data);
            Clipboard.SetContent(package);
        }
    }
}

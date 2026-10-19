using System.Diagnostics;
using System.IO;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class OpenFileLocation
    {
        public static void PathSelect(string path)
        {
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
        }
    }
}

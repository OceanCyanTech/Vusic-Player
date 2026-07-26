using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Helper.FileSystem
{
    public class FileSystemWatch
    {
        private readonly List<FileSystemWatcher> _watchers = new();
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _debounceTokens = new();
        public void WatchFolders(List<string> FolderPaths)
        {
            foreach (var path in FolderPaths)
            {
                if (!Directory.Exists(path)) continue;

                var watcher = new FileSystemWatcher(path)
                {
                    Filter = "*.*",
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    IncludeSubdirectories = true // Automatically watches all nested subfolders!
                };

                // Attach the exact same event handlers to all watchers
                watcher.Changed += Watcher_Changed;
                watcher.Renamed += Watcher_Renamed ;
                watcher.EnableRaisingEvents = true;

                _watchers.Add(watcher);
            }
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
        }

    

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
        }
    }
}

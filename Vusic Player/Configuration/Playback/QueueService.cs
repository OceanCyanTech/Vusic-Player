using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.Playback
{
    public class QueueService
    {
        public static ObservableCollection<string> VusicQueue { get; } = new();
        public static ObservableCollection<string> OriginalVusicQueue { get; } = new();
        public static ObservableCollection<string> VusicQueueNext { get; } = new();
    }
}

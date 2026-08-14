using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class GroupedCollectionModelShow
    {
        public Show Data { get; set; } = new Show();// Can be SongModel, VideoModel, etc.
        public string Letter { get; set; } = "";
        public bool IsGroupStart { get; set; }

    }
}

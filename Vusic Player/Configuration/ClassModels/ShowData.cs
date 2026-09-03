using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class ShowData
    {
        public string ShowName { get; set; } = "";
        public string ShowID { get; set; } = "";
        public List<EpisodeModel> episodes { get; set; } = new List<EpisodeModel>();
        public List<PlaylistItem> seasons { get; set; } = new List<PlaylistItem>();
    }
}

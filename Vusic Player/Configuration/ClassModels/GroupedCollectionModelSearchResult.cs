using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class GroupedCollectionModelSearchResult
    {
        public MasterSearchModel Data { get; set; } = new MasterSearchModel();
        public string Letter { get; set; } = "";
        public bool IsGroupStart { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vusic_Player.Configuration.ClassModels
{
    public class SearchResultGroupHeader : List<MasterSearchModel>
    {
        public string GroupName { get; set; }
        public SearchResultGroupHeader(string groupName, IEnumerable<MasterSearchModel> members) : base(members)
        {
            GroupName = groupName;
        }
    }
}

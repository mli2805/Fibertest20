using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public partial class GraphReadModel
    {
        public List<string> GraphVisibilityLevels { get; set; }
        public string SelectedGraphVisibilityLevel { get; set; }


        private void InitilizeVisibility()
        {
            GraphVisibilityLevels = new List<string>() {Resources.SID_Rtu, Resources.SID_Lines, Resources.SID_Equip, Resources.SID_Nodes, Resources.SID_All};
            SelectedGraphVisibilityLevel = GraphVisibilityLevels.Last();
        }
    }

}

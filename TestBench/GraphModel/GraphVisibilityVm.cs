using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.TestBench
{
    public partial class GraphReadModel
    {
        public List<string> GraphVisibilityLevels { get; set; }
        public string SelectedGraphVisibilityLevel { get; set; }


        private void InitilizeVisibility()
        {
            GraphVisibilityLevels = new List<string>() {"RTU", "Line", "Equip", "Node", "All"};
            SelectedGraphVisibilityLevel = GraphVisibilityLevels.First();
        }
    }

}

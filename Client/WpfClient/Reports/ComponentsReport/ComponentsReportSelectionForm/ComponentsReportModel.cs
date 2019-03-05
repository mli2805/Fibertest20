using System.Collections.Generic;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class ComponentsReportModel
    {
        public bool IsZoneSelectionEnabled { get; set; }

        public List<Zone> Zones { get; set; }
        public Zone SelectedZone { get; set; }

    }
}

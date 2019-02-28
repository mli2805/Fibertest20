using System.Collections.Generic;
using System.Windows;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class ComponentsReportModel
    {
        public Visibility ZoneSelectionVisibility { get; set; }
        public List<Zone> Zones { get; set; }
        public Zone SelectedZone { get; set; }

    }
}

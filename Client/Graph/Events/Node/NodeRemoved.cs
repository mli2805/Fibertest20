using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class NodeRemoved
    {
        public Guid NodeId { get; set; }
        public EquipmentType Type { get; set; }

        public List<NodeDetour> DetoursForGraph { get; set; } // mapper copies dictionary and list successfully
        public Guid FiberIdToDetourAdjustmentPoint { get; set; } // if there are no traces passing through this point
    }
}
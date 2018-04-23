using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class NodeDetour
    {
        public Guid FiberId { get; set; }
        public Guid NodeId1 { get; set; }
        public Guid NodeId2 { get; set; }
        public FiberState TraceState { get; set; }
        public Guid TraceId { get; set; }
    }

    public class RemoveNode
    {
        public Guid NodeId { get; set; }
        public EquipmentType Type { get; set; }

        public List<NodeDetour> DetoursForGraph { get; set; }
        public Guid FiberIdToDetourAdjustmentPoint { get; set; } // if there are no traces passing through this point
    }
}
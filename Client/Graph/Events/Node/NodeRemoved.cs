﻿using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class NodeRemoved
    {
        public Guid Id { get; set; }
        public EquipmentType Type { get; set; }
        public Dictionary<Guid,Guid> TraceWithNewFiberForDetourRemovedNode { get; set; }
        public Guid FiberIdToDetourAdjustmentPoint { get; set; } // if there are no traces passing through this point
    }
}
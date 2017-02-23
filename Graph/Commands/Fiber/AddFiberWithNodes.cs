using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph
{
    public class AddFiberWithNodes
    {
        public Guid Node1 { get; set; }
        public Guid Node2 { get; set; }

        public List<AddNode> AddNodes { get; set; }
        public List<AddEquipmentAtGpsLocation> AddEquipments { get; set; }
        public List<AddFiber> AddFibers { get; set; }

    }
}

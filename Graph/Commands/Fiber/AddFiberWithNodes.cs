using System;

namespace Iit.Fibertest.Graph.Commands
{
    public class AddFiberWithNodes
    {
        public Guid Id { get; set; }
        public Guid Node1 { get; set; }
        public Guid Node2 { get; set; }

        public int IntermediateNodesCount { get; set; }
        public EquipmentType EquipmentInIntermediateNodesType { get; set; } = EquipmentType.None;

    }
}

using System;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class EquipmentVm
    {
        public Guid Id { get; set; }
        public NodeVm Node { get; set; }
        public EquipmentType Type { get; set; }
    }
}
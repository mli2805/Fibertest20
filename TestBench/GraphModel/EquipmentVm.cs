using System;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EquipmentVm
    {
        public Guid Id { get; set; }
        public NodeVm Node { get; set; }

        public string Title { get; set; }
        public EquipmentType Type { get; set; }
        public int CableReserveLeft { get; set; }
        public int CableReserveRight { get; set; }
        public string Comment { get; set; }
    }
}
using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Graph.Commands
{
    public class AddNodeIntoFiber
    {
        public Guid Id { get; set; }
        public Guid FiberId { get; set; }
        public Guid NewFiberId1 { get; set; }
        public Guid NewFiberId2 { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public EquipmentType EqType { get; set; }
        public Guid EquipmentId { get; set; }
        public List<Guid> TracesConsumingEquipment { get; set; }
    }
}

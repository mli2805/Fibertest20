using System;
using GMap.NET;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class NodeIntoFiberAdded
    {
        public Guid Id { get; set; }
        public PointLatLng Position { get; set; }

        public Guid EquipmentId { get; set; }
        public bool IsAdjustmentPoint { get; set; }

        public Guid FiberId { get; set; }
        public Guid NewFiberId1 { get; set; }
        public Guid NewFiberId2 { get; set; }


    }
}

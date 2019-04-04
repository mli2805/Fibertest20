using System;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Node
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public EquipmentType TypeOfLastAddedEquipment { get; set; }
        public FiberState State { get; set; }
        public PointLatLng Position { get; set; }
        public string Comment { get; set; }

        public Guid AccidentOnTraceId { get; set; }
    }
}
using System;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace TreeVision.Vms
{
    public class NodeVm
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public EquipmentType Type { get; set; }
        public FiberState State { get; set; }

        public PointLatLng Position { get; set; }
        public string Comment { get; set; }
    }
}
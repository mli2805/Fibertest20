using System;
using System.ComponentModel;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class NodeVm
    {
        public Guid Id { get; set; }
        [Localizable(false)]
        public string Title { get; set; }
        public EquipmentType Type { get; set; }
        public FiberState State { get; set; }

        public PointLatLng Position { get; set; }
        public string Comment { get; set; }
    }
}
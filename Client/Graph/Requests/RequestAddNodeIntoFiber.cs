using System;
using GMap.NET;

namespace Iit.Fibertest.Graph.Requests
{
    public class RequestAddNodeIntoFiber
    {
        public Guid FiberId { get; set; }

        public EquipmentType InjectionType { get; set; } // adjustment point - empty node - cable reserve
        public PointLatLng Position { get; set; }
    }
}

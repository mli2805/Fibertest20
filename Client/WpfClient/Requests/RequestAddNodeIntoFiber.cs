using System;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RequestAddNodeIntoFiber
    {
        public Guid FiberId { get; set; }

        public EquipmentType InjectionType { get; set; } // adjustment point - empty node - cable reserve
        public PointLatLng Position { get; set; }
    }
}

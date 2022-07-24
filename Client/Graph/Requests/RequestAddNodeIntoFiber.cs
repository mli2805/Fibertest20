using System;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class RequestAddNodeIntoFiber
    {
        public Guid FiberId { get; set; }

        public EquipmentType InjectionType { get; set; } // adjustment point - empty node - cable reserve
        public PointLatLng Position { get; set; }
    }
}

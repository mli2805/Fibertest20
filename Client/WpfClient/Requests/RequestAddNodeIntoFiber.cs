using System;
using GMap.NET;

namespace Iit.Fibertest.Client
{
    public class RequestAddNodeIntoFiber
    {
        public Guid FiberId { get; set; }

        public bool IsAdjustmentPoint { get; set; }
        public PointLatLng Position { get; set; }
    }
}

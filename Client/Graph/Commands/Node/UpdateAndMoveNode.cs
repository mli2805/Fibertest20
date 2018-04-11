using System;
using GMap.NET;

namespace Iit.Fibertest.Graph
{
    public class UpdateAndMoveNode
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public PointLatLng GpsCoors { get; set; }
        public string Comment { get; set; }
    }
}
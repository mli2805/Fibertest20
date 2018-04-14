using System;
using GMap.NET;

namespace Iit.Fibertest.Graph
{
    public class UpdateAndMoveNode
    {
        public Guid NodeId { get; set; }
        public string Title { get; set; }
        public PointLatLng Position { get; set; }
        public string Comment { get; set; }
    }
}
using System;
using GMap.NET;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class RtuUpdated
    {
        public Guid RtuId { get; set; }
        public string Title { get; set; }

        public PointLatLng Position { get; set; }
        public string Comment { get; set; }
    }
}

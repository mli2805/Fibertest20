using System;

namespace Iit.Fibertest.Graph
{
    public class ZoneAdded
    {
        public Guid ZoneId { get; set; }
        public bool IsDefaultZone { get; set; }

        public string Title { get; set; }
        public string Comment { get; set; }
    }
}
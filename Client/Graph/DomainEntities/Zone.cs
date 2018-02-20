using System;

namespace Iit.Fibertest.Graph
{
    public class Zone
    {
        public Guid ZoneId { get; set; }
        public bool IsDefaultZone { get; set; }

        public string Title { get; set; }
        public string Comment { get; set; }

    }
}
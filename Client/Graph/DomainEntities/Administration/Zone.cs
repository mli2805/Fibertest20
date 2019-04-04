using System;

namespace Iit.Fibertest.Graph
{
    [Serializable]
    public class Zone
    {
        public Guid ZoneId { get; set; }
        public bool IsDefaultZone { get; set; }

        public string Title { get; set; }
        public string Comment { get; set; }

        public override string ToString()
        {
            return Title;
        }
    }
}
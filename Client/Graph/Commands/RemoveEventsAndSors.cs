using System;

namespace Iit.Fibertest.Graph
{
    public class RemoveEventsAndSors
    {
        public DateTime UpTo { get; set; }
        public bool IsMeasurementsNotEvents { get; set; }
        public bool IsOpticalEvents { get; set; }
        public bool IsNetworkEvents { get; set; }
        public bool IsRtuStatusEvents { get; set; }
    }
}
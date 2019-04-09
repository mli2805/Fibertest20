using System;

namespace Iit.Fibertest.Graph
{
    public class StartDbOptimazation
    {
        public bool IsRemoveElementsMode { get; set; }
        public DateTime UpTo { get; set; }
        public bool IsMeasurementsNotEvents { get; set; }
        public bool IsOpticalEvents { get; set; }
        public bool IsNetworkEvents { get; set; }
    }
}
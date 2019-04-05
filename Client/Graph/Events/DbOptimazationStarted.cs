using System;

namespace Iit.Fibertest.Graph
{
    public class DbOptimazationStarted
    {
        public DateTime UpTo { get; set; }
        public bool MeasurementsNotEvents { get; set; }
        public bool OpticalEvents { get; set; }
        public bool NetworkEvents { get; set; }
    }
}
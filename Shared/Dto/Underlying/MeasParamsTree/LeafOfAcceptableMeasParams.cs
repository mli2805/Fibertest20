using System;

namespace Iit.Fibertest.Dto
{
    [Serializable]
    public class LeafOfAcceptableMeasParams
    {
        public string[] Resolutions { get; set; }
        public string[] PulseDurations { get; set; }
        public string[] PeriodsToAverage { get; set; }
        public string[] MeasCountsToAverage { get; set; }
    }
}
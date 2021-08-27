// ReSharper disable InconsistentNaming

using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class TestsRelation
    {
        public string id { get; set; }
        public string testAId { get; set; }
        public string testBId { get; set; }
        public string type { get; set; } = "fibertest_fast_precise";
    }

    public class RelationItems
    {
        public List<TestsRelation> items { get; set; }
    }

    public class Test
    {
        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string otdrId { get; set; }
        public VeexOtauPort otauPort { get; set; }
        public int? period { get; set; }
        public int? failedPeriod { get; set; }

        public LinkObject analysis_parameters { get; set; }
        public LinkObject thresholds { get; set; }
        // public LinkObject relations { get; set; }
        public RelationItems relations { get; set; }
        public LinkObject reference { get; set; }
        public LinkObject lastFailed { get; set; }
        public LinkObject lastPassed { get; set; }
    }

}
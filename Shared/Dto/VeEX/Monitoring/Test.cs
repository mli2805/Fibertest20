namespace Iit.Fibertest.Dto
{
    public class Test
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string OtdrId { get; set; }
        public VeexOtauPort OtauPort { get; set; }
        public int? Period { get; set; }

        public LinkObject AnalysisParameters { get; set; }
        public LinkObject Thresholds { get; set; }
        public LinkObject Reference { get; set; }
        public LinkObject LastFailed { get; set; }
        public LinkObject LastPassed { get; set; }
    }

    public class CreateTestCmd
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string State { get; set; }
        public string OtdrId { get; set; } // must be GUID, but no matter which
        public VeexOtauPort VeexOtauPort { get; set; } // could null, but if set otauId should be GUID, but no matter which
        public int Period { get; set; }
    }
}
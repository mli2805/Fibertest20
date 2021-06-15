namespace Iit.Fibertest.Dto
{
    public class Test
    {
        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string otdrId { get; set; }
        public VeexOtauPort OtauPort { get; set; }
        public int? period { get; set; }

        public LinkObject analysis_parameters { get; set; }
        public LinkObject thresholds { get; set; }
        public LinkObject reference { get; set; }
        public LinkObject lastFailed { get; set; }
        public LinkObject lastPassed { get; set; }
    }

    public class CreateTestCmd
    {
        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string otdrId { get; set; } // must be GUID, but no matter which
        public VeexOtauPort VeexOtauPort { get; set; } // could null, but if set otauId should be GUID, but no matter which
        public int period { get; set; }
    }
}
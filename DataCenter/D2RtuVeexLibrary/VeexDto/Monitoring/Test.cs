namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class Test
    {
        public string id { get; set; }
        public string name { get; set; }
        public string state { get; set; }
        public string otdrId { get; set; }
        public OtauPort otauPort { get; set; }
        public int period { get; set; }

        public Item analysis_parameters { get; set; }
        public Item thresholds { get; set; }
        public Item reference { get; set; }
        public Item lastFailed { get; set; }
        public Item lastPassed { get; set; }
    }
}
using System;

namespace Iit.Fibertest.Dto
{
    public class Linkmap
    {
        public string Self { get; set; }
    }

    public class Report
    {
        public string Self { get; set; }
    }

    public class Traces
    {
        public string Self { get; set; }
    }

    public class CompletedTest
    {
        public Linkmap Linkmap { get; set; }
        public Report Report { get; set; }
        public string Result { get; set; }
        public string ExtendedResult { get; set; }
        public DateTime Started { get; set; }
        public Traces Traces { get; set; }
        public string Type { get; set; }
    }
}

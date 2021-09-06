using System;

// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
{
    public class Linkmap
    {
        public string self { get; set; }
    }

    public class Report
    {
        public string self { get; set; }
    }

    public class Traces
    {
        public string self { get; set; }
    }

    public class CompletedTest
    {
        public Linkmap linkmap { get; set; }
        public Report report { get; set; }
        public int[] indicesOfReferenceTraces { get; set; }
        public string result { get; set; }
        public string extendedResult { get; set; }
        public DateTime started { get; set; }
        public Traces traces { get; set; }
        public string type { get; set; }
    }
}

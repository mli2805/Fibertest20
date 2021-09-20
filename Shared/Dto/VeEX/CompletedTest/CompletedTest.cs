using System;
using System.Collections.Generic;

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
        public int id { get; set; } // completed test ID
        public string testId { get; set; } 
        public string reason { get; set; }
        public Linkmap linkmap { get; set; }
        public Report report { get; set; }
        public int[] indicesOfReferenceTraces { get; set; }
        public string result { get; set; }
        public string extendedResult { get; set; }
        public DateTime started { get; set; }
        public Traces traces { get; set; }
        public string type { get; set; }
    }

    public class CompletedTestPortion
    {
        public List<CompletedTest> items { get; set; }
        public int total { get; set; }
    }
}

using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    public class TestsLinks
    {
        public List<LinkObject> items { get; set; }
        public int offset { get; set; }
        public int total { get; set; }
    }
}
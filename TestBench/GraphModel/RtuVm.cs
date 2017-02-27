using System;

namespace Iit.Fibertest.TestBench
{
    public class RtuVm
    {
        public Guid Id { get; set; }
        public NodeVm Node { get; set; }

        public string Title { get; set; }
        public string Comment { get; set; }
    }
}
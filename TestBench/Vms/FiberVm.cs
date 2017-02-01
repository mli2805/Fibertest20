using System;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class FiberVm
    {
        public Guid Id { get; set; }
        public NodeVm NodeA { get; set; }
        public NodeVm NodeB { get; set; }

        public FiberState State { get; set; }

        public int UserInputedLength { get; set; }
    }
}
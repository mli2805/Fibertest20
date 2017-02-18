using System;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class FiberVm
    {
        public Guid Id { get; set; }
        public NodeVm Node1 { get; set; }
        public NodeVm Node2 { get; set; }

        public FiberState State { get; set; }

        public int UserInputedLength { get; set; }
    }
}
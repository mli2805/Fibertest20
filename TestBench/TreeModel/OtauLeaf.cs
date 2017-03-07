using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public class OtauLeaf : Leaf
    {
        public RtuPartState State { get; set; }
        public int PortCount { get; set; }
        public int FirstPortNumber { get; set; }

        public OtauLeaf(ReadModel readModel, IWindowManager windowManager, Bus bus) : base(readModel, windowManager, bus)
        {
        }
    }
}
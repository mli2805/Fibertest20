using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class OtauLeaf : Leaf
    {
        public RtuPartState State { get; set; }
        public int PortCount { get; set; }
        public int FirstPortNumber { get; set; }
        public int MasterPort { get; set; }
        public RtuPartState OtauState { get; set; }
        public ImageSource OtauStatePictogram => OtauState.GetPictogram();

        public override string Name
        {
            get { return string.Format(Resources.SID_port_trace, MasterPort, Title); }
            set {}
        }

        public OtauLeaf(ReadModel readModel, IWindowManager windowManager, Bus bus) : base(readModel, windowManager, bus)
        {
        }
    }
}
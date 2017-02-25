using System.Collections.ObjectModel;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class TreeReadModel
    {
        private readonly IWindowManager _windowManager;
        private readonly ReadModel _readModel;
        private readonly Bus _bus;
        public ObservableCollection<Leaf> Tree { get; set; } = new ObservableCollection<Leaf>();

        public TreeReadModel(IWindowManager windowManager, ReadModel readModel, Bus bus)
        {
            _windowManager = windowManager;
            _readModel = readModel;
            _bus = bus;
        }

        #region Rtu
        public void Apply(RtuAtGpsLocationAdded e)
        {
            var rtuLeaf = new RtuLeaf(_readModel,_windowManager, _bus)
            {
                Id = e.Id, Title = Resources.SID_noname_RTU, Color = Brushes.DarkGray,
            };

            // TODO this should be set by rtu initialization
            rtuLeaf.PortCount = 8;
            rtuLeaf.MonitoringState = MonitoringState.On;
            rtuLeaf.MainChannelState = RtuPartState.Normal;
            rtuLeaf.Color = Brushes.Black;
            for (int i=1; i <= rtuLeaf.PortCount; i++)
            {
                var port = new PortLeaf(_readModel, _windowManager, _bus)
                    { PortNumber = i, Title = string.Format(Resources.SID_Port_N, i), Color = Brushes.Blue, };
                rtuLeaf.Children.Add(port);
                port.Parent = rtuLeaf;
            }

            rtuLeaf.IsExpanded = true;
            Tree.Add(rtuLeaf);
        }

        public void Apply(RtuUpdated e)
        {
            var rtu = Tree.GetById(e.Id);
            rtu.Title = e.Title;
        }
        public void Apply(RtuRemoved e)
        {
            var rtu = Tree.GetById(e.Id);
            RemoveWithChildren(rtu);
        }

        private void RemoveWithChildren(Leaf leaf)
        {
            foreach (var child in leaf.Children)
            {
                RemoveWithChildren(child);
            }
            Tree.Remove(leaf);
        }
        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            var trace = new TraceLeaf(_readModel,_windowManager, _bus)
            {
                Id = e.Id, Title = e.Title, TraceState = FiberState.NotJoined, Color = Brushes.Blue,
            };
            var rtu = Tree.GetById(e.RtuId);
            rtu.Children.Add(trace);
            trace.Parent = rtu;
            rtu.IsExpanded = true;
        }

        public void Apply(TraceAttached e)
        {
            TraceLeaf trace = Tree.GetById(e.TraceId) as TraceLeaf;
            if (trace == null)
                return;
            trace.Title = string.Format(Resources.SID_port_trace, e.Port, trace.Title);
            trace.Color = Brushes.Black;
            trace.PortNumber = e.Port;
        }

        public void Apply(TraceDetached e)
        {
            var trace = Tree.GetById(e.TraceId);
            trace.Color = Brushes.Blue;

        }
        #endregion
    }
}

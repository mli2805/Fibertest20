using System.Collections.ObjectModel;
using System.Windows.Media;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Events;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public class TreeReadModel
    {
        public ObservableCollection<Leaf> Tree { get; set; } = new ObservableCollection<Leaf>();

        #region Rtu
        public void Apply(RtuAtGpsLocationAdded e)
        {
            var leaf = new RtuLeaf()
            {
                Id = e.Id, Title = Resources.SID_noname_RTU, Color = Brushes.DarkGray,

            };
            Tree.Add(leaf);
        }

        public void Apply(RtuUpdated e)
        {
            var rtu = Tree.GetById(e.Id);
            rtu.Title = e.Title;
        }
        public void Apply(RtuRemoved e)
        {
            var rtu = Tree.GetById(e.Id);
            Tree.Remove(rtu);
        }
        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            var trace = new TraceLeaf()
            {
                Id = e.Id, Title = e.Title, TraceState = FiberState.NotJoined, Color = Brushes.Blue,
            };
            var rtu = Tree.GetById(e.RtuId);
            rtu.Children.Add(trace);
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

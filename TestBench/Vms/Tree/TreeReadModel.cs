using System;
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
            var rtuLeaf = new RtuLeaf(_readModel, _windowManager, _bus)
            {
                Id = e.Id,
                Title = Resources.SID_noname_RTU,
                Color = Brushes.DarkGray,
            };

            // TODO this should be set by rtu initialization
            rtuLeaf.PortCount = 8;
            rtuLeaf.MonitoringState = MonitoringState.On;
            rtuLeaf.MainChannelState = RtuPartState.Normal;
            rtuLeaf.Color = Brushes.Black;
            for (int i = 1; i <= rtuLeaf.PortCount; i++)
            {
                var port = new PortLeaf(_readModel, _windowManager, _bus, i);
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
            var trace = new TraceLeaf(_readModel, _windowManager, _bus)
            {
                Id = e.Id,
                Title = e.Title,
                TraceState = FiberState.NotJoined,
                Color = Brushes.Blue,
            };
            var rtu = Tree.GetById(e.RtuId);
            rtu.Children.Add(trace);
            trace.Parent = rtu;
            rtu.IsExpanded = true;
        }

        public void Apply(TraceAttached e)
        {
            TraceLeaf traceLeaf = Tree.GetById(e.TraceId) as TraceLeaf;
            if (traceLeaf == null)
                return;

            RtuLeaf rtu = (RtuLeaf)Tree.GetById(traceLeaf.Parent.Id);

            rtu.Children.RemoveAt(e.Port - 1);
            rtu.Children.Insert(e.Port - 1,
                new TraceLeaf(_readModel, _windowManager, _bus)
                {
                    Id = e.TraceId,
                    Title = string.Format(Resources.SID_port_trace, e.Port, traceLeaf.Title),
                    TraceState = FiberState.Ok,
                    Color = Brushes.Black,
                    PortNumber = e.Port,
                    Parent = rtu,
                });

            rtu.Children.Remove(traceLeaf);
        }

        public void Apply(TraceDetached e)
        {
            TraceLeaf traceLeaf = Tree.GetById(e.TraceId) as TraceLeaf;
            if (traceLeaf == null)
                return;

            RtuLeaf rtu = (RtuLeaf)Tree.GetById(traceLeaf.Parent.Id);
            int port = traceLeaf.PortNumber;
            var detachedTraceLeaf = new TraceLeaf(_readModel, _windowManager, _bus)
            {
                PortNumber = 0,
                TraceState = traceLeaf.TraceState,
                Color = Brushes.Blue,
                Title = traceLeaf.Title.Split(':')[1].Trim(),
            };

            rtu.Children.RemoveAt(port - 1);
            rtu.Children.Insert(port - 1, new PortLeaf(_readModel, _windowManager, _bus, port));

            rtu.Children.Add(detachedTraceLeaf);
        }
        #endregion
    }
}

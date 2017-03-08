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
            Tree.Add(new RtuLeaf(_readModel, _windowManager, _bus)
            {
                Id = e.Id,
                Title = Resources.SID_noname_RTU,
                Color = Brushes.DarkGray,
                IsExpanded = true,
            });
        }

        public void Apply(RtuInitialized e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.Id);

            rtuLeaf.OwnPortCount = e.OwnPortCount;
            rtuLeaf.FullPortCount = e.FullPortCount;
            rtuLeaf.MainChannelState = e.MainChannelState;
            rtuLeaf.ReserveChannelState = e.ReserveChannelState;
            rtuLeaf.MonitoringState = MonitoringState.Off;

            rtuLeaf.Color = Brushes.Black;
            for (int i = 1; i <= rtuLeaf.FullPortCount; i++)
            {
                var port = new PortLeaf(_readModel, _windowManager, _bus, rtuLeaf, i);
                rtuLeaf.Children.Insert(i - 1, port);
                port.Parent = rtuLeaf;
            }
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

        public void Apply(OtauAttached e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            var otauLeaf = new OtauLeaf(_readModel, _windowManager, _bus)
            {
                Id = e.Id,
                Parent = rtuLeaf,
                Title = string.Format(Resources.SID_Optical_switch_with_Address, e.NetAddress),
                Color = Brushes.Black,
                MasterPort = e.MasterPort,
                FirstPortNumber = rtuLeaf.FullPortCount + 1,
                PortCount = e.PortCount,
                OtauState = RtuPartState.Normal,
                IsExpanded = true,
            };
            for (int i = 0; i < otauLeaf.PortCount; i++)
                otauLeaf.Children.Add(new PortLeaf(_readModel, _windowManager, _bus, otauLeaf, i + 1));
            rtuLeaf.Children.Remove(rtuLeaf.Children[e.MasterPort - 1]);
            rtuLeaf.Children.Insert(e.MasterPort - 1, otauLeaf);
            rtuLeaf.FullPortCount += otauLeaf.PortCount;
        }

        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            var rtu = Tree.GetById(e.RtuId);
            var trace = new TraceLeaf(_readModel, _windowManager, _bus, rtu)
            {
                Id = e.Id,
                Title = e.Title,
                TraceState = FiberState.NotJoined,
                Color = Brushes.Blue,
            };
            rtu.Children.Add(trace);
            rtu.IsExpanded = true;
        }

        public void Apply(TraceUpdated e)
        {
            var traceLeaf = Tree.GetById(e.Id);
            traceLeaf.Title = e.Title;
        }

        public void Apply(TraceCleaned e)
        {
            var traceLeaf = Tree.GetById(e.Id);
            var rtuLeaf = traceLeaf.Parent;
            rtuLeaf.Children.Remove(traceLeaf);
        }

        public void Apply(TraceRemoved e)
        {
            var traceLeaf = Tree.GetById(e.Id);
            var rtuLeaf = traceLeaf.Parent;
            rtuLeaf.Children.Remove(traceLeaf);
        }

        public void Apply(TraceAttached e)
        {
            TraceLeaf traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            RtuLeaf rtuLeaf = (RtuLeaf)Tree.GetById(traceLeaf.Parent.Id);
            Leaf portOwner = rtuLeaf.GetOwnerOfExtendedPort(e.Port);

            portOwner.Children.RemoveAt(e.Port - 1);
            portOwner.Children.Insert(e.Port - 1,
                new TraceLeaf(_readModel, _windowManager, _bus, portOwner)
                {
                    Id = e.TraceId,
                    TraceState = FiberState.NotChecked,
                    Title = traceLeaf.Title,
                    Color = Brushes.Black,
                    PortNumber = e.Port,
                });
            portOwner.Children.Remove(traceLeaf);
        }

        public void Apply(TraceDetached e)
        {
            TraceLeaf traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            RtuLeaf rtu = (RtuLeaf)Tree.GetById(traceLeaf.Parent.Id);
            int port = traceLeaf.PortNumber;
            var detachedTraceLeaf = new TraceLeaf(_readModel, _windowManager, _bus, rtu)
            {
                PortNumber = 0,
                Title = traceLeaf.Title,
                TraceState = FiberState.NotJoined,
                Color = Brushes.Blue,
            };

            rtu.Children.RemoveAt(port - 1);
            rtu.Children.Insert(port - 1, new PortLeaf(_readModel, _windowManager, _bus, rtu, port));

            rtu.Children.Add(detachedTraceLeaf);
        }
        #endregion
    }
}

using System.Collections.ObjectModel;
using System.Linq;
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
        public FreePortsToggleButton FreePortsToggleButton { get; } = new FreePortsToggleButton();
        public string Statistics
        {
            get
            {
                var portCount = Tree.PortCount();
                var traceCount = Tree.TraceCount();
                return string.Format(Resources.SID_Tree_statistics, Tree.Count,
                    Tree.Sum(r => ((RtuLeaf)r).ChildrenProvider.Children.Count(c => c is OtauLeaf)),
                    portCount, traceCount, (double)traceCount / portCount * 100);
            }
        }


        public TreeReadModel(IWindowManager windowManager, ReadModel readModel, Bus bus)
        {
            _windowManager = windowManager;
            _readModel = readModel;
            _bus = bus;
        }

        #region Rtu
        public void Apply(RtuAtGpsLocationAdded e)
        {
            Tree.Add(new RtuLeaf(_readModel, _windowManager, _bus, FreePortsToggleButton)
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
            for (int i = 1; i <= rtuLeaf.OwnPortCount; i++)
            {
                var port = new PortLeaf(_readModel, _windowManager, _bus, rtuLeaf, i);
                rtuLeaf.ChildrenProvider.Children.Insert(i - 1, port);
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
            IPortOwner owner = leaf as IPortOwner;
            if (owner != null)
                foreach (var child in owner.ChildrenProvider.Children)
                {
                    RemoveWithChildren(child);
                }
            Tree.Remove(leaf);
        }

        public void Apply(OtauAttached e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            var otauLeaf = new OtauLeaf(_readModel, _windowManager, _bus, FreePortsToggleButton)
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
                otauLeaf.ChildrenProvider.Children.Add(new PortLeaf(_readModel, _windowManager, _bus, otauLeaf, i + 1));
            rtuLeaf.ChildrenProvider.Children.Remove(rtuLeaf.ChildrenProvider.Children[e.MasterPort - 1]);
            rtuLeaf.ChildrenProvider.Children.Insert(e.MasterPort - 1, otauLeaf);
            rtuLeaf.FullPortCount += otauLeaf.PortCount;
        }

        public void Apply(OtauDetached e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            var otauLeaf = (OtauLeaf)Tree.GetById(e.Id);
            var port = otauLeaf.MasterPort;
            rtuLeaf.FullPortCount -= otauLeaf.PortCount;
            rtuLeaf.ChildrenProvider.Children.Remove(otauLeaf);

            var portLeaf = new PortLeaf(_readModel, _windowManager, _bus, rtuLeaf, port);
            rtuLeaf.ChildrenProvider.Children.Insert(port - 1, portLeaf);
            portLeaf.Parent = rtuLeaf;
        }

        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            var rtu = (RtuLeaf)Tree.GetById(e.RtuId);
            var trace = new TraceLeaf(_readModel, _windowManager, _bus, rtu)
            {
                Id = e.Id,
                Title = e.Title,
                TraceState = FiberState.NotJoined,
                Color = Brushes.Blue,
            };
            rtu.ChildrenProvider.Children.Add(trace);
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
            var rtuLeaf = traceLeaf.Parent is RtuLeaf ? (RtuLeaf)traceLeaf.Parent : (RtuLeaf)traceLeaf.Parent.Parent;
            rtuLeaf.ChildrenProvider.Children.Remove(traceLeaf);
        }

        public void Apply(TraceRemoved e)
        {
            var traceLeaf = Tree.GetById(e.Id);
            var rtuLeaf = traceLeaf.Parent is RtuLeaf ? (RtuLeaf)traceLeaf.Parent : (RtuLeaf)traceLeaf.Parent.Parent;
            rtuLeaf.ChildrenProvider.Children.Remove(traceLeaf);
        }

        public void Apply(TraceAttached e)
        {
            TraceLeaf traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            RtuLeaf rtuLeaf = (RtuLeaf)Tree.GetById(traceLeaf.Parent.Id);
            var portOwner = rtuLeaf.GetOwnerOfExtendedPort(e.Port);
            var port = portOwner is RtuLeaf ? e.Port : e.Port - ((OtauLeaf)portOwner).FirstPortNumber + 1;

            portOwner.ChildrenProvider.Children[port - 1] =
                new TraceLeaf(_readModel, _windowManager, _bus, portOwner)
                {
                    Id = e.TraceId,
                    TraceState = FiberState.NotChecked,
                    Title = traceLeaf.Title,
                    Color = Brushes.Black,
                    PortNumber = port,
                };
            rtuLeaf.ChildrenProvider.Children.Remove(traceLeaf);
        }

        public void Apply(TraceDetached e)
        {
            TraceLeaf traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            var owner = Tree.GetById(traceLeaf.Parent.Id);
            RtuLeaf rtu = owner is RtuLeaf ? (RtuLeaf)owner : (RtuLeaf)(owner.Parent);
            int port = traceLeaf.PortNumber;
            var detachedTraceLeaf = new TraceLeaf(_readModel, _windowManager, _bus, rtu)
            {
                Id = traceLeaf.Id,
                PortNumber = 0,
                Title = traceLeaf.Title,
                TraceState = FiberState.NotJoined,
                Color = Brushes.Blue,
            };

            ((IPortOwner)owner).ChildrenProvider.Children.RemoveAt(port - 1);
            ((IPortOwner)owner).ChildrenProvider.Children.Insert(port - 1, new PortLeaf(_readModel, _windowManager, _bus, owner, port));

            rtu.ChildrenProvider.Children.Add(detachedTraceLeaf);
        }
        #endregion
    }
}

using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfConnections;
using Iit.Fibertest.WcfServiceForClientInterface;
using Serilog;

namespace Iit.Fibertest.Client
{
    public class TreeOfRtuModel : PropertyChangedBase
    {
        private readonly IWindowManager _windowManager;
        private readonly ReadModel _readModel;

        private readonly IWcfServiceForClient _c2DWcfManager;

        private readonly IniFile _iniFile35;
        private readonly ILogger _log;
        private readonly IMyLog _logFile;

        public C2DWcfManager C2DWcfManager { get; set; }
        public ObservableCollection<Leaf> Tree { get; set; } = new ObservableCollection<Leaf>();
        public FreePorts FreePorts { get; } = new FreePorts(true);

        private PostOffice _postOffice;
        public PostOffice PostOffice
        {
            get { return _postOffice; }
            set
            {
                if (Equals(value, _postOffice)) return;
                _postOffice = value;
                NotifyOfPropertyChange();
            }
        }

        public string Statistics
        {
            get
            {
                var portCount = Tree.PortCount();
                var traceCount = Tree.TraceCount();
                return string.Format(Resources.SID_Tree_statistics, Tree.Count,
                    Tree.Sum(r => ((RtuLeaf)r).ChildrenImpresario.Children.Count(c => c is OtauLeaf)),
                    portCount, traceCount, (double)traceCount / portCount * 100);
            }
        }

        public TreeOfRtuModel(IWindowManager windowManager, ReadModel readModel, IWcfServiceForClient c2DWcfManager, IniFile iniFile35, ILogger log, IMyLog logFile)
        {
            _windowManager = windowManager;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _iniFile35 = iniFile35;
            _log = log;
            _logFile = logFile;

            PostOffice = new PostOffice();
        }

        #region Rtu
        public void Apply(RtuAtGpsLocationAdded e)
        {
            Tree.Add(new RtuLeaf(_readModel, _windowManager, _c2DWcfManager, _iniFile35, _log, _logFile, PostOffice, FreePorts)
            {
                Id = e.Id,
                Title = Resources.SID_noname_RTU,
                Color = Brushes.DarkGray,
                IsExpanded = true,
            });
            NotifyOfPropertyChange(nameof(Statistics));
        }

        public void Apply(RtuInitialized e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.Id);

            if (rtuLeaf.Serial == null)
            {
                InitializeRtuFirstTime(e, rtuLeaf);
                return;
            }

            if (rtuLeaf.Serial == e.Serial)
            {
                if (rtuLeaf.OwnPortCount != e.OwnPortCount)
                {
                    // main otdr problem ?
                    // TODO
                    return;
                }

                if (rtuLeaf.FullPortCount != e.FullPortCount)
                {
                    // bop changes
                    // TODO
                    return;
                }

                if (rtuLeaf.FullPortCount == e.FullPortCount)
                {
                    // just re-initialization, nothing should be done?
                }
            }

            if (rtuLeaf.Serial != e.Serial)
            {
                //TODO discuss and implement rtu replacement scenario
            }
        }

        private void InitializeRtuFirstTime(RtuInitialized e, RtuLeaf rtuLeaf)
        {
            rtuLeaf.OwnPortCount = e.OwnPortCount;
            rtuLeaf.FullPortCount = e.OwnPortCount; // otauAttached then will increase 
            rtuLeaf.Serial = e.Serial;
            rtuLeaf.MainChannelState = e.MainChannelState;
            rtuLeaf.ReserveChannelState = e.ReserveChannelState;
            rtuLeaf.MonitoringState = MonitoringState.Off;

            rtuLeaf.Color = Brushes.Black;
            for (int i = 1; i <= rtuLeaf.OwnPortCount; i++)
            {
                var port = new PortLeaf(_readModel, _windowManager, _c2DWcfManager, _iniFile35, _logFile, PostOffice, rtuLeaf, i);
                rtuLeaf.ChildrenImpresario.Children.Insert(i - 1, port);
                port.Parent = rtuLeaf;
            }
            if (e.Otaus != null)
                foreach (var otauAttached in e.Otaus)
                    Apply(otauAttached);
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
                foreach (var child in owner.ChildrenImpresario.Children)
                {
                    RemoveWithChildren(child);
                }
            Tree.Remove(leaf);
        }

        public void Apply(OtauAttached e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            var otauLeaf = new OtauLeaf(_readModel, _windowManager, _c2DWcfManager, PostOffice, FreePorts)
            {
                Id = e.Id,
                Parent = rtuLeaf,
                Title = string.Format(Resources.SID_Optical_switch_with_Address, e.NetAddress.ToStringB()),
                Color = Brushes.Black,
                MasterPort = e.MasterPort,
                FirstPortNumber = rtuLeaf.FullPortCount + 1,
                PortCount = e.PortCount,
                OtauState = RtuPartState.Normal,
                IsExpanded = true,
            };
            for (int i = 0; i < otauLeaf.PortCount; i++)
                otauLeaf.ChildrenImpresario.Children.Add(new PortLeaf(_readModel, _windowManager, _c2DWcfManager, _iniFile35, _logFile, PostOffice, otauLeaf, i + 1));
            rtuLeaf.ChildrenImpresario.Children.Remove(rtuLeaf.ChildrenImpresario.Children[e.MasterPort - 1]);
            rtuLeaf.ChildrenImpresario.Children.Insert(e.MasterPort - 1, otauLeaf);
            rtuLeaf.FullPortCount += otauLeaf.PortCount;
        }

        public void Apply(OtauDetached e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            var otauLeaf = (OtauLeaf)Tree.GetById(e.Id);
            var port = otauLeaf.MasterPort;
            rtuLeaf.FullPortCount -= otauLeaf.PortCount;
            rtuLeaf.ChildrenImpresario.Children.Remove(otauLeaf);

            var portLeaf = new PortLeaf(_readModel, _windowManager, _c2DWcfManager, _iniFile35, _logFile, PostOffice, rtuLeaf, port);
            rtuLeaf.ChildrenImpresario.Children.Insert(port - 1, portLeaf);
            portLeaf.Parent = rtuLeaf;
        }

        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            var rtu = (RtuLeaf)Tree.GetById(e.RtuId);
            var trace = new TraceLeaf(_readModel, _windowManager, _c2DWcfManager, PostOffice, rtu)
            {
                Id = e.Id,
                Title = e.Title,
                TraceState = FiberState.NotJoined,
                Color = Brushes.Blue,
            };
            rtu.ChildrenImpresario.Children.Add(trace);
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
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void Apply(TraceRemoved e)
        {
            var traceLeaf = Tree.GetById(e.Id);
            var rtuLeaf = traceLeaf.Parent is RtuLeaf ? (RtuLeaf)traceLeaf.Parent : (RtuLeaf)traceLeaf.Parent.Parent;
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void Apply(TraceAttached e)
        {
            TraceLeaf traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            RtuLeaf rtuLeaf = (RtuLeaf)Tree.GetById(traceLeaf.Parent.Id);
            var portOwner = rtuLeaf.GetOwnerOfExtendedPort(e.Port);

            if (portOwner == null) return;


            var port = portOwner is RtuLeaf ? e.Port : e.Port - ((OtauLeaf)portOwner).FirstPortNumber + 1;

            portOwner.ChildrenImpresario.Children[port - 1] =
                new TraceLeaf(_readModel, _windowManager, _c2DWcfManager, PostOffice, portOwner)
                {
                    Id = e.TraceId,
                    TraceState = FiberState.NotChecked,
                    Title = traceLeaf.Title,
                    Color = Brushes.Black,
                    PortNumber = port,
                };
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void Apply(TraceDetached e)
        {
            TraceLeaf traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            var owner = Tree.GetById(traceLeaf.Parent.Id);
            RtuLeaf rtu = owner is RtuLeaf ? (RtuLeaf)owner : (RtuLeaf)(owner.Parent);
            int port = traceLeaf.PortNumber;
            var detachedTraceLeaf = new TraceLeaf(_readModel, _windowManager, _c2DWcfManager, PostOffice, rtu)
            {
                Id = traceLeaf.Id,
                PortNumber = 0,
                Title = traceLeaf.Title,
                TraceState = FiberState.NotJoined,
                Color = Brushes.Blue,
            };

            ((IPortOwner)owner).ChildrenImpresario.Children.RemoveAt(port - 1);
            ((IPortOwner)owner).ChildrenImpresario.Children.
                Insert(port - 1, new PortLeaf(_readModel, _windowManager, _c2DWcfManager, _iniFile35, _logFile, PostOffice, owner, port));

            rtu.ChildrenImpresario.Children.Add(detachedTraceLeaf);
        }
        #endregion
    }
}

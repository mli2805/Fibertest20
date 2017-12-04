using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class TreeOfRtuModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;
        private readonly RtuLeafContextMenuProvider _rtuLeafContextMenuProvider;
        private readonly TraceLeafContextMenuProvider _traceLeafContextMenuProvider;
        private readonly IMyWindowManager _windowManager;
        private readonly ReadModel _readModel;

        private readonly IWcfServiceForClient _c2DWcfManager;

        private readonly IniFile _iniFile35;
        private readonly IMyLog _logFile;

        public ObservableCollection<Leaf> Tree { get; set; } = new ObservableCollection<Leaf>();
        public FreePorts FreePorts { get; }

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
                return string.Format(Resources.SID_Tree_statistics, Tree.Count,
                    Tree.Sum(r => ((RtuLeaf)r).ChildrenImpresario.Children.Count(c => c is OtauLeaf)),
                    Tree.PortCount(), Tree.TraceCount(), (double)Tree.TraceCount() / Tree.PortCount() * 100);
            }
        }

        public TreeOfRtuModel(IMyWindowManager windowManager, ReadModel readModel,
            IWcfServiceForClient c2DWcfManager, IniFile iniFile35, IMyLog logFile,
            // only for pass to it's leaves
            ILifetimeScope globalScope, PostOffice postOffice, FreePorts freePorts, 
            RtuLeafContextMenuProvider rtuLeafContextMenuProvider,
            TraceLeafContextMenuProvider traceLeafContextMenuProvider)
        {
            _globalScope = globalScope;
            _rtuLeafContextMenuProvider = rtuLeafContextMenuProvider;
            _traceLeafContextMenuProvider = traceLeafContextMenuProvider;
            _windowManager = windowManager;
            _readModel = readModel;
            _c2DWcfManager = c2DWcfManager;
            _iniFile35 = iniFile35;
            _logFile = logFile;

            PostOffice = postOffice;
            FreePorts = freePorts;
            FreePorts.AreVisible = true;
        }

        #region Rtu
        public void Apply(RtuAtGpsLocationAdded e)
        {
            var newRtuLeaf = new RtuLeaf(_globalScope, _logFile, _readModel, _windowManager,
                _c2DWcfManager, _rtuLeafContextMenuProvider, PostOffice, FreePorts);
            newRtuLeaf.Id = e.Id;
            Tree.Add(newRtuLeaf);
            NotifyOfPropertyChange(nameof(Statistics));
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
                OwnPortCount = e.PortCount,
                OtauNetAddress = e.NetAddress,
                OtauState = RtuPartState.Normal,
                IsExpanded = true,
            };
            for (int i = 0; i < otauLeaf.OwnPortCount; i++)
                otauLeaf.ChildrenImpresario.Children.Add(new PortLeaf(_readModel, _windowManager, _c2DWcfManager, _iniFile35, _logFile, PostOffice, otauLeaf, i + 1));
            rtuLeaf.ChildrenImpresario.Children.Remove(rtuLeaf.ChildrenImpresario.Children[e.MasterPort - 1]);
            rtuLeaf.ChildrenImpresario.Children.Insert(e.MasterPort - 1, otauLeaf);
            rtuLeaf.FullPortCount += otauLeaf.OwnPortCount;
        }

        public void Apply(OtauDetached e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            var otauLeaf = (OtauLeaf)Tree.GetById(e.Id);
            var port = otauLeaf.MasterPort;
            rtuLeaf.FullPortCount -= otauLeaf.OwnPortCount;
            rtuLeaf.ChildrenImpresario.Children.Remove(otauLeaf);

            var portLeaf = new PortLeaf(_readModel, _windowManager, _c2DWcfManager, _iniFile35, _logFile, PostOffice, rtuLeaf, port);
            rtuLeaf.ChildrenImpresario.Children.Insert(port - 1, portLeaf);
            portLeaf.Parent = rtuLeaf;
        }

        public void Apply(ListOfRtuWithChangedAvailabilityDto dto)
        {
            foreach (var rtuWithChannelChanges in dto.List)
            {
                var rtuLeaf = (RtuLeaf)Tree.GetById(rtuWithChannelChanges.RtuId);
                if (rtuLeaf == null)
                    continue;
                if (rtuWithChannelChanges.MainChannel == ChannelStateChanges.Recovered)
                    rtuLeaf.MainChannelState = RtuPartState.Normal;
                else if (rtuWithChannelChanges.MainChannel == ChannelStateChanges.Broken)
                    rtuLeaf.MainChannelState = RtuPartState.Broken;
            }
        }

        public void Apply(NetworkEvent networkEvent)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(networkEvent.RtuId);
            if (rtuLeaf == null)
                return;

            if (networkEvent.MainChannelState != ChannelStateChanges.TheSame)
                rtuLeaf.MainChannelState = (RtuPartState)(int)networkEvent.MainChannelState;
            if (networkEvent.ReserveChannelState != ChannelStateChanges.TheSame)
                rtuLeaf.ReserveChannelState = (RtuPartState)(int)networkEvent.ReserveChannelState;
        }

        public void Apply(OpticalEvent opticalEvent)
        {
            var traceLeaf = (TraceLeaf)Tree.GetById(opticalEvent.TraceId);
            if (traceLeaf == null)
                return;

            traceLeaf.TraceState = opticalEvent.TraceState;
        }
        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            var rtu = (RtuLeaf)Tree.GetById(e.RtuId);
            var trace = new TraceLeaf(_readModel, _windowManager, _c2DWcfManager, PostOffice, rtu, _traceLeafContextMenuProvider)
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
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == e.TraceId);
            if (trace == null)
                return;

            TraceLeaf traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            RtuLeaf rtuLeaf = (RtuLeaf)Tree.GetById(traceLeaf.Parent.Id);
            var portOwner = rtuLeaf.GetOwnerOfExtendedPort(e.Port);

            if (portOwner == null) return;


            var port = portOwner is RtuLeaf ? e.Port : e.Port - ((OtauLeaf)portOwner).FirstPortNumber + 1;

            portOwner.ChildrenImpresario.Children[port - 1] =
                new TraceLeaf(_readModel, _windowManager, _c2DWcfManager, PostOffice, portOwner, _traceLeafContextMenuProvider)
                {
                    Id = e.TraceId,
                    TraceState = FiberState.NotChecked,
                    Title = traceLeaf.Title,
                    Color = Brushes.Black,
                    PortNumber = port,
                    HasEnoughBaseRefsToPerformMonitoring = trace.HasEnoughBaseRefsToPerformMonitoring,
                };
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void Apply(TraceDetached e)
        {
            TraceLeaf traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            var owner = Tree.GetById(traceLeaf.Parent.Id);
            RtuLeaf rtu = owner is RtuLeaf ? (RtuLeaf)owner : (RtuLeaf)(owner.Parent);
            int port = traceLeaf.PortNumber;
            var detachedTraceLeaf = new TraceLeaf(_readModel, _windowManager, _c2DWcfManager, PostOffice, rtu, _traceLeafContextMenuProvider)
            {
                Id = traceLeaf.Id,
                PortNumber = 0,
                Title = traceLeaf.Title,
                TraceState = FiberState.NotJoined,
                Color = Brushes.Blue,
                IsInMonitoringCycle = false,
            };

            ((IPortOwner)owner).ChildrenImpresario.Children.RemoveAt(port - 1);
            ((IPortOwner)owner).ChildrenImpresario.Children.
                Insert(port - 1, new PortLeaf(_readModel, _windowManager, _c2DWcfManager, _iniFile35, _logFile, PostOffice, owner, port));

            rtu.ChildrenImpresario.Children.Add(detachedTraceLeaf);
        }

        #endregion

        #region JustEchosOfCmdsSentToRtu
        public void Apply(BaseRefAssigned e)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == e.TraceId);
            if (trace == null) return;
            var traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            traceLeaf.HasEnoughBaseRefsToPerformMonitoring 
                = trace.HasEnoughBaseRefsToPerformMonitoring;
            if (!traceLeaf.HasEnoughBaseRefsToPerformMonitoring)
                traceLeaf.IsInMonitoringCycle =  false;
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
            rtuLeaf.OtauNetAddress = e.OtauNetAddress;

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


        public void Apply(MonitoringSettingsChanged e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            rtuLeaf.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
            ApplyMonitoringSettingsRecursively(rtuLeaf, e);
        }

        private void ApplyMonitoringSettingsRecursively(IPortOwner portOwner, MonitoringSettingsChanged e)
        {
            foreach (var leaf in portOwner.ChildrenImpresario.Children)
            {
                var traceLeaf = leaf as TraceLeaf;
                if (traceLeaf != null)
                {
                    traceLeaf.IsInMonitoringCycle = e.TracesInMonitoringCycle.Contains(traceLeaf.Id);
                    traceLeaf.RtuMonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                }

                var otauLeaf = leaf as OtauLeaf;
                if (otauLeaf != null)
                    ApplyMonitoringSettingsRecursively(otauLeaf, e);
            }

        }

        public void Apply(MonitoringStarted e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            rtuLeaf.MonitoringState = MonitoringState.On;
            ApplyRecursively(rtuLeaf, MonitoringState.On);
        }

        public void Apply(MonitoringStopped e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            rtuLeaf.MonitoringState = MonitoringState.Off;
            ApplyRecursively(rtuLeaf, MonitoringState.Off);
        }
        private void ApplyRecursively(IPortOwner portOwner, MonitoringState rtuMonitoringState)
        {
            foreach (var leaf in portOwner.ChildrenImpresario.Children)
            {
                var traceLeaf = leaf as TraceLeaf;
                if (traceLeaf != null)
                    traceLeaf.RtuMonitoringState = rtuMonitoringState;

                var otauLeaf = leaf as OtauLeaf;
                if (otauLeaf != null)
                    ApplyRecursively(otauLeaf, rtuMonitoringState);
            }
        }
        #endregion
    }
}

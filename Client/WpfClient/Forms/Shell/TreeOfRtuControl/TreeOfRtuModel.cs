using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class TreeOfRtuModel : PropertyChangedBase
    {
        private readonly ILifetimeScope _globalScope;

        public ObservableCollection<Leaf> Tree { get; set; } = new ObservableCollection<Leaf>();
        public FreePorts FreePorts { get; }

        public string Statistics =>
            string.Format(Resources.SID_Tree_statistics, Tree.Count,
                Tree.Sum(r => ((RtuLeaf)r).ChildrenImpresario.Children.Count(c => c is OtauLeaf)),
                Tree.PortCount(), Tree.TraceCount(), (double)Tree.TraceCount() / Tree.PortCount() * 100);

        public TreeOfRtuModel(ILifetimeScope globalScope, FreePorts freePorts)
        {
            _globalScope = globalScope;

            FreePorts = freePorts;
            FreePorts.AreVisible = true;
        }

        #region RTU
        public void Apply(RtuAtGpsLocationAdded e)
        {
            var newRtuLeaf = _globalScope.Resolve<RtuLeaf>();
            newRtuLeaf.Id = e.Id;
            newRtuLeaf.Title = e.Title;
            Tree.Add(newRtuLeaf);
            NotifyOfPropertyChange(nameof(Statistics));
        }

        public void Apply(RtuUpdated e)
        {
            var rtu = Tree.GetById(e.RtuId);
            rtu.Title = e.Title;
        }
        public void Apply(RtuRemoved e)
        {
            var rtu = Tree.GetById(e.RtuId);
            RemoveWithAdditionalOtaus(rtu);
        }

        private void RemoveWithAdditionalOtaus(Leaf leaf)
        {
            if (leaf is IPortOwner owner)
                foreach (var child in owner.ChildrenImpresario.Children)
                    RemoveWithAdditionalOtaus(child);
            Tree.Remove(leaf);
        }

        public void Apply(OtauAttached e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            var otauLeaf = _globalScope.Resolve<OtauLeaf>();

            otauLeaf.Id = e.Id;
            otauLeaf.Parent = rtuLeaf;
            otauLeaf.Title = string.Format(Resources.SID_Optical_switch_with_Address, e.NetAddress.ToStringB());
            otauLeaf.Color = Brushes.Black;
            otauLeaf.MasterPort = e.MasterPort;
            otauLeaf.OwnPortCount = e.PortCount;
            otauLeaf.OtauNetAddress = e.NetAddress;
            otauLeaf.OtauState = RtuPartState.Ok;
            otauLeaf.IsExpanded = true;

            for (int i = 0; i < otauLeaf.OwnPortCount; i++)
                otauLeaf.ChildrenImpresario.Children.Add(
                    _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", otauLeaf), new NamedParameter(@"portNumber", i + 1)));
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

            var portLeaf = _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", rtuLeaf), new NamedParameter(@"portNumber", port));
            rtuLeaf.ChildrenImpresario.Children.Insert(port - 1, portLeaf);
            portLeaf.Parent = rtuLeaf;
        }

        public void Apply(NetworkEventAdded e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            if (rtuLeaf == null)
                return;

            rtuLeaf.MainChannelState = e.MainChannelState;
            rtuLeaf.ReserveChannelState = e.ReserveChannelState;
        }

       
        public void Apply(MeasurementAdded e)
        {
            var traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            if (traceLeaf == null || traceLeaf.TraceState == FiberState.NotJoined)
                return;

            traceLeaf.TraceState = e.TraceState != FiberState.Ok && e.BaseRefType == BaseRefType.Fast
                ? FiberState.Suspicion : e.TraceState;
        }
        #endregion

        #region Trace
        public void Apply(TraceAdded e)
        {
            var rtuLeaf = (RtuLeaf)Tree.GetById(e.RtuId);
            var traceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", rtuLeaf));

            traceLeaf.Id = e.Id;
            traceLeaf.Title = e.Title;
            traceLeaf.TraceState = FiberState.NotJoined;
            traceLeaf.Color = Brushes.Blue;

            rtuLeaf.ChildrenImpresario.Children.Add(traceLeaf);
            rtuLeaf.IsExpanded = true;
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
            var portOwner = rtuLeaf.GetPortOwner(new NetAddress(e.OtauPortDto.OtauIp, e.OtauPortDto.OtauTcpPort));
            if (portOwner == null) return;

            var port = e.OtauPortDto.OpticalPort;

            var newTraceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", portOwner));
            newTraceLeaf.Id = e.TraceId;
            newTraceLeaf.TraceState = e.PreviousTraceState;
            newTraceLeaf.Title = traceLeaf.Title;
            newTraceLeaf.Color = Brushes.Black;
            newTraceLeaf.PortNumber = port;

            portOwner.ChildrenImpresario.Children[port - 1] = newTraceLeaf;
            newTraceLeaf.BaseRefsSet = traceLeaf.BaseRefsSet;
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void Apply(TraceDetached e)
        {
            var traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);
            var owner = Tree.GetById(traceLeaf.Parent.Id);
            var rtuLeaf = owner is RtuLeaf ? (RtuLeaf)owner : (RtuLeaf)(owner.Parent);
            int port = traceLeaf.PortNumber;
            if (port <= 0)
                return; // some error

            var detachedTraceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", rtuLeaf));
            detachedTraceLeaf.Id = traceLeaf.Id;
            detachedTraceLeaf.PortNumber = 0;
            detachedTraceLeaf.Title = traceLeaf.Title;
            detachedTraceLeaf.TraceState = FiberState.NotJoined;
            detachedTraceLeaf.Color = Brushes.Blue;
            detachedTraceLeaf.BaseRefsSet = traceLeaf.BaseRefsSet;
            detachedTraceLeaf.BaseRefsSet.IsInMonitoringCycle = false;

            ((IPortOwner)owner).ChildrenImpresario.Children.RemoveAt(port - 1);
            ((IPortOwner)owner).ChildrenImpresario.Children.Insert(port - 1,
                _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", owner), new NamedParameter(@"portNumber", port)));

            rtuLeaf.ChildrenImpresario.Children.Add(detachedTraceLeaf);
        }

        #endregion

        #region JustEchosOfCmdsSentToRtu
        public void Apply(BaseRefAssigned e)
        {
            var traceLeaf = (TraceLeaf)Tree.GetById(e.TraceId);

            var preciseBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Precise);
            if (preciseBaseRef != null)
            {
                traceLeaf.BaseRefsSet.PreciseId = preciseBaseRef.Id;
                traceLeaf.BaseRefsSet.PreciseDuration = preciseBaseRef.Duration;
            }
            var fastBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Fast);
            if (fastBaseRef != null)
            {
                traceLeaf.BaseRefsSet.FastId = fastBaseRef.Id;
                traceLeaf.BaseRefsSet.FastDuration = fastBaseRef.Duration;
            }
            var additionalBaseRef = e.BaseRefs.FirstOrDefault(b => b.BaseRefType == BaseRefType.Additional);
            if (additionalBaseRef != null)
            {
                traceLeaf.BaseRefsSet.AdditionalId = additionalBaseRef.Id;
                traceLeaf.BaseRefsSet.AdditionalDuration = additionalBaseRef.Duration;
            }

            if (!traceLeaf.BaseRefsSet.HasEnoughBaseRefsToPerformMonitoring)
                traceLeaf.BaseRefsSet.IsInMonitoringCycle = false;
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
                    // main OTDR problem ?
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
                //TODO discuss and implement RTU replacement scenario
            }

            rtuLeaf.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
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
                var port = _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", rtuLeaf), new NamedParameter(@"portNumber", i));
                rtuLeaf.ChildrenImpresario.Children.Insert(i - 1, port);
                port.Parent = rtuLeaf;
            }
            if (e.Otaus != null)
                foreach (var otauAttached in e.Otaus)
                    Apply(otauAttached);

            rtuLeaf.TreeOfAcceptableMeasParams = e.AcceptableMeasParams;
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
                if (leaf is TraceLeaf traceLeaf)
                {
                    traceLeaf.BaseRefsSet.IsInMonitoringCycle = e.TracesInMonitoringCycle.Contains(traceLeaf.Id);
                    traceLeaf.BaseRefsSet.RtuMonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                }

                if (leaf is OtauLeaf otauLeaf)
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
                if (leaf is TraceLeaf traceLeaf)
                    traceLeaf.BaseRefsSet.RtuMonitoringState = rtuMonitoringState;

                if (leaf is OtauLeaf otauLeaf)
                    ApplyRecursively(otauLeaf, rtuMonitoringState);
            }
        }
        #endregion


    }
}

using System;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EchoEventsOnTreeExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly CurrentUser _currentUser;
        private readonly ReadModel _readModel;
        private readonly RtuEventsOnTreeExecutor _rtuEventsOnTreeExecutor;

        public EchoEventsOnTreeExecutor(ILifetimeScope globalScope, TreeOfRtuModel treeOfRtuModel, 
            CurrentUser currentUser, ReadModel readModel, RtuEventsOnTreeExecutor rtuEventsOnTreeExecutor)
        {
            _globalScope = globalScope;
            _treeOfRtuModel = treeOfRtuModel;
            _currentUser = currentUser;
            _readModel = readModel;
            _rtuEventsOnTreeExecutor = rtuEventsOnTreeExecutor;
        }

        public void AssignBaseRef(BaseRefAssigned e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == e.TraceId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var traceLeaf = (TraceLeaf)_treeOfRtuModel.GetById(e.TraceId);

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

        public void InitializeRtu(RtuInitialized e)
        {
            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.Id);

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
                    _rtuEventsOnTreeExecutor.AttachOtau(otauAttached);

            rtuLeaf.TreeOfAcceptableMeasParams = e.AcceptableMeasParams;
        }


        public void ChangeMonitoringSettings(MonitoringSettingsChanged e)
        {
            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.RtuId);
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

        public void StartMonitoring(MonitoringStarted e)
        {
            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.RtuId);
            rtuLeaf.MonitoringState = MonitoringState.On;
            ApplyRecursively(rtuLeaf, MonitoringState.On);
        }

        public void StopMonitoring(MonitoringStopped e)
        {
            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.RtuId);
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
    }
}
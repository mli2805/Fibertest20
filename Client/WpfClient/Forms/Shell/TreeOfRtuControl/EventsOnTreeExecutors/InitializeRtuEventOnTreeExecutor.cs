using System;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class InitializeRtuEventOnTreeExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly IMyLog _logFile;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly TreeOfRtuModel _treeOfRtuModel;

        public InitializeRtuEventOnTreeExecutor(ILifetimeScope globalScope, IMyLog logFile, CurrentUser currentUser,
            Model readModel, TreeOfRtuModel treeOfRtuModel)
        {
            _globalScope = globalScope;
            _logFile = logFile;
            _currentUser = currentUser;
            _readModel = readModel;
            _treeOfRtuModel = treeOfRtuModel;
        }

        public void InitializeRtu(RtuInitialized e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.Id).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.Id);

            if (rtuLeaf.Serial == null)
                InitializeFirstTime(rtuLeaf, e);
            else
                ReInitialize(rtuLeaf, e);

            rtuLeaf.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
            foreach (var child in rtuLeaf.ChildrenImpresario.Children)
            {
                if (child is TraceLeaf traceLeaf)
                    traceLeaf.BaseRefsSet.RtuMonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                else if (child is OtauLeaf otauLeaf)
                    foreach (var leaf in otauLeaf.ChildrenImpresario.Children)
                    {
                        if (leaf is TraceLeaf traceLeaf1)
                            traceLeaf1.BaseRefsSet.RtuMonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
                    }
            }
        }

        private void ReInitialize(RtuLeaf rtuLeaf, RtuInitialized e)
        {

            if (rtuLeaf.OwnPortCount < e.OwnPortCount)
            {
                for (int i = rtuLeaf.OwnPortCount+1; i <= e.OwnPortCount; i++)
                {
                    var port = _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", rtuLeaf), new NamedParameter(@"portNumber", i));
                    rtuLeaf.ChildrenImpresario.Children.Insert(i - 1, port);
                    port.Parent = rtuLeaf;
                }
            }

            if (rtuLeaf.OwnPortCount > e.OwnPortCount)
            {
                for (int i = rtuLeaf.OwnPortCount - 1; i >= e.OwnPortCount; i--)
                {
                    rtuLeaf.ChildrenImpresario.Children.RemoveAt(i);
                }
            }

            if (e.Children != null)
                foreach (var childPair in e.Children)
                {
                    var otau = rtuLeaf.ChildrenImpresario.Children.Select(child => child as OtauLeaf)
                        .FirstOrDefault(o => o?.OtauNetAddress.Equals(childPair.Value.NetAddress) == true);
                    if (otau == null)
                    {
                        _logFile.AppendLine(@"RTU cannot return child OTAU which does not exist yet! It's a business rule");
                        _logFile.AppendLine(@"Client sends existing OTAU list -> ");
                        _logFile.AppendLine(@" RTU MUST detach any OTAU which are not in client's list");
                        _logFile.AppendLine(@" and attach all OTAU from this list");
                    }
                    else
                        rtuLeaf.SetOtauState(otau.Id, childPair.Value.IsOk);
                }

            SetRtuProperties(rtuLeaf, e);
        }

        private void InitializeFirstTime(RtuLeaf rtuLeaf, RtuInitialized e)
        {
            SetRtuProperties(rtuLeaf, e);

            for (int i = 1; i <= e.OwnPortCount; i++)
            {
                var port = _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", rtuLeaf), new NamedParameter(@"portNumber", i));
                rtuLeaf.ChildrenImpresario.Children.Insert(i - 1, port);
                port.Parent = rtuLeaf;
            }

            if (e.Children != null && e.Children.Count > 0)
            {
                _logFile.AppendLine(@"While first initialization RTU cannot return children! It's a business rule");
                _logFile.AppendLine(@"RTU MUST detach all OTAUs if has any");
            }

        }

        private static void SetRtuProperties(RtuLeaf rtuLeaf, RtuInitialized e)
        {
            rtuLeaf.RtuMaker = e.Maker;
            rtuLeaf.OwnPortCount = e.OwnPortCount;
            rtuLeaf.FullPortCount = e.FullPortCount;
            rtuLeaf.Serial = e.Serial;
            rtuLeaf.MainChannelState = e.MainChannelState;
            rtuLeaf.ReserveChannelState = e.ReserveChannelState;
            rtuLeaf.OtauNetAddress = e.OtauNetAddress;
            rtuLeaf.Color = Brushes.Black;
            rtuLeaf.TreeOfAcceptableMeasParams = e.AcceptableMeasParams;
        }
    }
}
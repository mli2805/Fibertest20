using System;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class InitializeRtuEventOnTreeExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly RtuEventsOnTreeExecutor _rtuEventsOnTreeExecutor;

        public InitializeRtuEventOnTreeExecutor(ILifetimeScope globalScope, CurrentUser currentUser,
            Model readModel, TreeOfRtuModel treeOfRtuModel, RtuEventsOnTreeExecutor rtuEventsOnTreeExecutor)
        {
            _globalScope = globalScope;
            _currentUser = currentUser;
            _readModel = readModel;
            _treeOfRtuModel = treeOfRtuModel;
            _rtuEventsOnTreeExecutor = rtuEventsOnTreeExecutor;
        }

        public void InitializeRtu(RtuInitialized e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.Id).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.Id);

            SetRtuProperties(rtuLeaf, e);
            rtuLeaf.MonitoringState = e.IsMonitoringOn ? MonitoringState.On : MonitoringState.Off;
        }

        private void SetRtuProperties(RtuLeaf rtuLeaf, RtuInitialized e)
        {
            rtuLeaf.OwnPortCount = e.OwnPortCount;
            rtuLeaf.FullPortCount = e.FullPortCount;
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


    }
}
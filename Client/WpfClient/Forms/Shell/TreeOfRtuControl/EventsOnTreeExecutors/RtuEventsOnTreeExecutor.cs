using System;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuEventsOnTreeExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly TraceEventsOnTreeExecutor _traceEventsOnTreeExecutor;
        private readonly CurrentUser _currentUser;
        private readonly Model _readModel;

        public RtuEventsOnTreeExecutor(ILifetimeScope globalScope, Model readModel, CurrentUser currentUser,
            TreeOfRtuModel treeOfRtuModel, TraceEventsOnTreeExecutor traceEventsOnTreeExecutor)
        {
            _globalScope = globalScope;
            _treeOfRtuModel = treeOfRtuModel;
            _traceEventsOnTreeExecutor = traceEventsOnTreeExecutor;
            _currentUser = currentUser;
            _readModel = readModel;
        }

        public void AddRtuAtGpsLocation(RtuAtGpsLocationAdded e)
        {
            if (_currentUser.ZoneId != Guid.Empty) return;

            var newRtuLeaf = _globalScope.Resolve<RtuLeaf>();
            newRtuLeaf.Id = e.Id;
            newRtuLeaf.Title = e.Title;
            _treeOfRtuModel.Tree.Add(newRtuLeaf);
        }

        public void UpdateRtu(RtuUpdated e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtu = _treeOfRtuModel.GetById(e.RtuId);
            rtu.Title = e.Title;
        }

        public void RemoveRtu(RtuRemoved e)
        {
            var rtu = _treeOfRtuModel.GetById(e.RtuId);
            if (rtu == null) return;

            RemoveWithAdditionalOtaus(rtu);
        }

        private void RemoveWithAdditionalOtaus(Leaf leaf)
        {
            if (leaf is IPortOwner owner)
                foreach (var child in owner.ChildrenImpresario.Children)
                    RemoveWithAdditionalOtaus(child);
            _treeOfRtuModel.Tree.Remove(leaf);
        }

        public void AttachOtau(OtauAttached e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.RtuId);
            var otauLeaf = _globalScope.Resolve<OtauLeaf>();

            otauLeaf.Id = e.Id;
            otauLeaf.Parent = rtuLeaf;
            otauLeaf.Title = string.Format(Resources.SID_Optical_switch_with_Address, e.NetAddress.ToStringA());
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
            rtuLeaf.SetOtauState(otauLeaf.Id, otauLeaf.OtauState == RtuPartState.Ok);
        }

        public void DetachOtau(OtauDetached e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.RtuId);
            var otauLeaf = (OtauLeaf)_treeOfRtuModel.GetById(e.Id);
            var port = otauLeaf.MasterPort;

            foreach (var traceId in e.TracesOnOtau)
                _traceEventsOnTreeExecutor.DetachTrace(traceId);

            rtuLeaf.FullPortCount -= otauLeaf.OwnPortCount;
            rtuLeaf.ChildrenImpresario.Children.Remove(otauLeaf);

            var portLeaf = _globalScope.Resolve<PortLeaf>(new NamedParameter(@"parent", rtuLeaf), new NamedParameter(@"portNumber", port));
            rtuLeaf.ChildrenImpresario.Children.Insert(port - 1, portLeaf);
            portLeaf.Parent = rtuLeaf;
            rtuLeaf.RemoveOtauState(otauLeaf.Id);
        }

        public void AddNetworkEvent(NetworkEventAdded e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.RtuId);
            if (rtuLeaf == null)
                return;

            rtuLeaf.MainChannelState = e.MainChannelState;
            rtuLeaf.ReserveChannelState = e.ReserveChannelState;
        }

        public void AddBopNetworkEvent(BopNetworkEventAdded e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.RtuId);
            if (rtuLeaf == null)
                return;

            foreach (var child in rtuLeaf.ChildrenImpresario.Children)
            {
                if (child is OtauLeaf otauLeaf && otauLeaf.OtauNetAddress.Ip4Address == e.OtauIp && otauLeaf.OtauNetAddress.Port == e.TcpPort)
                {
                    otauLeaf.OtauState = e.IsOk ? RtuPartState.Ok : RtuPartState.Broken;
                    rtuLeaf.SetOtauState(child.Id, e.IsOk);
                }
            }

        }
    }
}
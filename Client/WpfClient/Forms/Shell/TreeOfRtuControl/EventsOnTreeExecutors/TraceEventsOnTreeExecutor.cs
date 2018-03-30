using System;
using System.Linq;
using System.Windows.Media;
using Autofac;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceEventsOnTreeExecutor
    {
        private readonly ILifetimeScope _globalScope;
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly CurrentUser _currentUser;
        private readonly ReadModel _readModel;

        public TraceEventsOnTreeExecutor(ILifetimeScope globalScope, TreeOfRtuModel treeOfRtuModel, CurrentUser currentUser, ReadModel readModel)
        {
            _globalScope = globalScope;
            _treeOfRtuModel = treeOfRtuModel;
            _currentUser = currentUser;
            _readModel = readModel;
        }

        public void AddTrace(TraceAdded e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Rtus.First(r => r.Id == e.RtuId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(e.RtuId);
            var traceLeaf = _globalScope.Resolve<TraceLeaf>(new NamedParameter(@"parent", rtuLeaf));

            traceLeaf.Id = e.TraceId;
            traceLeaf.Title = e.Title;
            traceLeaf.TraceState = FiberState.NotJoined;
            traceLeaf.Color = Brushes.Blue;

            rtuLeaf.ChildrenImpresario.Children.Add(traceLeaf);
            rtuLeaf.IsExpanded = true;
        }

        public void UpdateTrace(TraceUpdated e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == e.Id).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var traceLeaf = _treeOfRtuModel.GetById(e.Id);
            traceLeaf.Title = e.Title;
        }

        public void CleanTrace(TraceCleaned e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == e.TraceId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var traceLeaf = _treeOfRtuModel.GetById(e.TraceId);
            var rtuLeaf = traceLeaf.Parent is RtuLeaf ? (RtuLeaf)traceLeaf.Parent : (RtuLeaf)traceLeaf.Parent.Parent;
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void RemoveTrace(TraceRemoved e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == e.TraceId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var traceLeaf = _treeOfRtuModel.GetById(e.TraceId);
            var rtuLeaf = traceLeaf.Parent is RtuLeaf ? (RtuLeaf)traceLeaf.Parent : (RtuLeaf)traceLeaf.Parent.Parent;
            rtuLeaf.ChildrenImpresario.Children.Remove(traceLeaf);
        }

        public void AttaceTrace(TraceAttached e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == e.TraceId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            TraceLeaf traceLeaf = (TraceLeaf)_treeOfRtuModel.GetById(e.TraceId);
            RtuLeaf rtuLeaf = (RtuLeaf)_treeOfRtuModel.GetById(traceLeaf.Parent.Id);
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

        public void DetachTrace(TraceDetached e)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == e.TraceId).ZoneIds.Contains(_currentUser.ZoneId)) return;

            var traceLeaf = (TraceLeaf)_treeOfRtuModel.GetById(e.TraceId);
            var owner = _treeOfRtuModel.GetById(traceLeaf.Parent.Id);
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

    }
}
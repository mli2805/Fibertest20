using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly ReadModel _readModel;

        public TraceEventsOnGraphExecutor(GraphReadModel model, ReadModel readModel)
        {
            _model = model;
            _readModel = readModel;
        }

        public void AddTrace(TraceAdded evnt)
        {
            var fiberIds = _readModel.GetFibersByNodes(evnt.Nodes).ToList();
            _model.ChangeFutureTraceColor(evnt.Id, fiberIds, FiberState.NotJoined);
        }

        private IEnumerable<FiberVm> GetTraceFibersByNodes(List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberBetweenNodes(nodes[i - 1], nodes[i]);
        }

        private FiberVm GetFiberBetweenNodes(Guid node1, Guid node2)
        {
            return _model.Data.Fibers.First(
                f => f.Node1.Id == node1 && f.Node2.Id == node2 ||
                     f.Node1.Id == node2 && f.Node2.Id == node1);
        }

        public void CleanTrace(TraceCleaned evnt)
        {
            var trace = _readModel.Traces.First(t => t.Id == evnt.Id);
            GetTraceFibersByNodes(trace.Nodes).ToList().ForEach(f => f.RemoveState(evnt.Id));
        }

        public void RemoveTrace(TraceRemoved evnt)
        {
            var trace = _readModel.Traces.First(t => t.Id == evnt.Id);
            var traceFibers = GetTraceFibersByNodes(trace.Nodes).ToList();
            foreach (var fiberVm in traceFibers)
            {
                fiberVm.RemoveState(evnt.Id);
                if (fiberVm.State == FiberState.NotInTrace)
                    _model.Data.Fibers.Remove(fiberVm);
            }
            foreach (var nodeId in trace.Nodes)
            {
//                if (_model.Data.Rtus.Any(r => r.Node.Id == nodeId) ||
                   if ( _model.Data.Fibers.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId))
                    continue;
                var nodeVm = _model.Data.Nodes.First(n => n.Id == nodeId);
                _model.Data.Nodes.Remove(nodeVm);
            }
        }

        public void AttachTrace(TraceAttached evnt)
        {
            var trace = _readModel.Traces.First(t => t.Id == evnt.TraceId);
            ApplyTraceStateToFibers(trace, FiberState.Ok);
        }

        public void DetachTrace(TraceDetached evnt)
        {
            var trace = _readModel.Traces.First(t => t.Id == evnt.TraceId);
            ApplyTraceStateToFibers(trace, FiberState.NotJoined);
            _model.CleanAccidentPlacesOnTrace(evnt.TraceId);
        }

        // events are applied to Graph BEFORE applying to ReadModel, so trace doesn't have new state
        private void ApplyTraceStateToFibers(Trace trace, FiberState state)
        {
            foreach (var fiberVm in GetTraceFibersByNodes(trace.Nodes))
                fiberVm.SetState(trace.Id, state);
        }
       
    }
}
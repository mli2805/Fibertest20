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
        private readonly AccidentEventsOnGraphExecutor _accidentEventsOnGraphExecutor;

        public TraceEventsOnGraphExecutor(GraphReadModel model, ReadModel readModel, AccidentEventsOnGraphExecutor accidentEventsOnGraphExecutor)
        {
            _model = model;
            _readModel = readModel;
            _accidentEventsOnGraphExecutor = accidentEventsOnGraphExecutor;
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
            foreach (var fiberId in evnt.FiberIds)
            {
                var fiberVm = _model.Data.Fibers.First(f => f.Id == fiberId);
                fiberVm.RemoveState(evnt.TraceId);
            }
        }

        public void RemoveTrace(TraceRemoved evnt)
        {
            foreach (var fiberId in evnt.FiberIds)
            {
                var fiberVm = _model.Data.Fibers.First(f => f.Id == fiberId);
                fiberVm.RemoveState(evnt.TraceId);
                if (fiberVm.State == FiberState.NotInTrace)
                    _model.Data.Fibers.Remove(fiberVm);
            }
            foreach (var nodeId in evnt.NodeIds)
            {
                if (_model.Data.Fibers.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId))
                    continue;
                var nodeVm = _model.Data.Nodes.First(n => n.Id == nodeId);
                if (nodeVm.Type != EquipmentType.Rtu)
                    _model.Data.Nodes.Remove(nodeVm);
            }
        }

        public void AttachTrace(TraceAttached evnt)
        {
            _accidentEventsOnGraphExecutor.ShowMonitoringResult(new MeasurementAdded()
            {
                TraceId = evnt.TraceId,
                TraceState = evnt.PreviousTraceState,
                Accidents = evnt.AccidentsInLastMeasurement,
            });
        }

        public void DetachTrace(TraceDetached evnt)
        {
            var trace = _readModel.Traces.First(t => t.Id == evnt.TraceId);
            foreach (var fiberVm in GetTraceFibersByNodes(trace.Nodes))
                fiberVm.SetState(trace.Id, trace.State);
            _model.CleanAccidentPlacesOnTrace(evnt.TraceId);
        }

    }
}
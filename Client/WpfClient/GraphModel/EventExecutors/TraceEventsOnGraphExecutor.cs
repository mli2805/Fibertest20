﻿using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;
        private readonly AccidentEventsOnGraphExecutor _accidentEventsOnGraphExecutor;

        public TraceEventsOnGraphExecutor(GraphReadModel model, Model readModel,
            CurrentUser currentUser, CurrentlyHiddenRtu currentlyHiddenRtu, AccidentEventsOnGraphExecutor accidentEventsOnGraphExecutor)
        {
            _model = model;
            _readModel = readModel;
            _currentUser = currentUser;
            _currentlyHiddenRtu = currentlyHiddenRtu;
            _accidentEventsOnGraphExecutor = accidentEventsOnGraphExecutor;
        }

        public void AddTrace(TraceAdded evnt)
        {
            if (_currentUser.ZoneId != Guid.Empty) return;

            var fiberIds = _readModel.GetFibersByNodes(evnt.NodeIds).ToList();
            _model.ChangeFutureTraceColor(evnt.TraceId, fiberIds, FiberState.NotJoined);
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

        // event applied to ReadModel firstly and at this moment trace could be cleaned/removed, so fibers list should be prepared beforehand
        // but in case trace was hidden check fiberVm/nodeVm on null before operations
        public void CleanTrace(TraceCleaned evnt)
        {
            foreach (var fiberId in evnt.FiberIds)
            {
                var fiberVm = _model.Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                fiberVm?.RemoveState(evnt.TraceId);
            }
        }

        // event applied to ReadModel firstly and at this moment trace could be cleaned/removed, so fibers list should be prepared beforehand
        // but in case trace was hidden check fiberVm/nodeVm on null before operations
        public void RemoveTrace(TraceRemoved evnt)
        {
            foreach (var fiberId in evnt.FiberIds)
            {
                var fiberVm = _model.Data.Fibers.FirstOrDefault(f => f.Id == fiberId);
                if (fiberVm == null) continue;
                fiberVm.RemoveState(evnt.TraceId);
                if (fiberVm.State == FiberState.NotInTrace)
                    _model.Data.Fibers.Remove(fiberVm);
            }
            foreach (var nodeId in evnt.NodeIds)
            {
                if (_model.Data.Fibers.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId))
                    continue;
                var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (nodeVm?.Type != EquipmentType.Rtu)
                    _model.Data.Nodes.Remove(nodeVm);
            }
        }

        public void AttachTrace(TraceAttached evnt)
        {
            if (!ShouldAcceptEventForTrace(evnt.TraceId)) return;

            _accidentEventsOnGraphExecutor.ShowMonitoringResult(new MeasurementAdded()
            {
                TraceId = evnt.TraceId,
                TraceState = evnt.PreviousTraceState,
                Accidents = evnt.AccidentsInLastMeasurement,
            });
        }

        public void DetachTrace(TraceDetached evnt)
        {
            DetachTrace(evnt.TraceId);
        }

        public void DetachTrace(Guid traceId)
        {
            if (!ShouldAcceptEventForTrace(traceId)) return;

            var trace = _readModel.Traces.First(t => t.TraceId == traceId);
            foreach (var fiberVm in GetTraceFibersByNodes(trace.NodeIds))
                fiberVm.SetState(trace.TraceId, trace.State);
            _model.CleanAccidentPlacesOnTrace(traceId);
        }

        private bool ShouldAcceptEventForTrace(Guid traceId)
        {
            if (_currentUser.ZoneId != Guid.Empty &&
                !_readModel.Traces.First(t => t.TraceId == traceId).ZoneIds.Contains(_currentUser.ZoneId)) return false;

            var trace = _readModel.Traces.First(t => t.TraceId == traceId);
            var rtu = _readModel.Rtus.First(r => r.Id == trace.RtuId);
            return !_currentlyHiddenRtu.Collection.Contains(rtu.Id);
        }
    }
}
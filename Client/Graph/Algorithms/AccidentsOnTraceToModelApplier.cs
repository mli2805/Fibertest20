using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph.Algorithms
{
    public  class AccidentsOnTraceToModelApplier
    {
        private readonly Model _model;
        private readonly AccidentPlaceLocator _accidentPlaceLocator;

        public AccidentsOnTraceToModelApplier(Model model, AccidentPlaceLocator accidentPlaceLocator)
        {
            _model = model;
            _accidentPlaceLocator = accidentPlaceLocator;
        }

        public  void CleanAccidentPlacesOnTrace(Guid traceId)
        {
            var nodes = _model.Nodes.Where(n => n.AccidentOnTraceId == traceId).ToList();
            foreach (var node in nodes)
            {
                _model.Nodes.Remove(node);
            }

            foreach (var fiber in _model.Fibers)
            {
                fiber.RemoveBadSegment(traceId);
            }
        }

        public void ShowMonitoringResult(MeasurementAdded e)
        {
            var trace = _model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null) return;

            trace.State = e.TraceState;
            foreach (var fiber in _model.GetTraceFibers(trace))
                fiber.SetState(trace.TraceId, trace.State);

            CleanAccidentPlacesOnTrace(trace.TraceId);

            if (e.TraceState != FiberState.Ok && e.TraceState != FiberState.NoFiber)
                e.Accidents.ForEach(a => ShowAccidentPlaceOnTrace(a, e.TraceId));
        }

        private void ShowAccidentPlaceOnTrace(AccidentOnTrace accident, Guid traceId)
        {
            switch (accident)
            {
                case AccidentAsNewEvent accidentAsNewEvent:
                    ShowBetweenNodes(accidentAsNewEvent, traceId);
                    return;
                case AccidentInOldEvent accidentInOldEvent:
                    if (accidentInOldEvent.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                        ShowBadSegment(accidentInOldEvent, traceId);
                    else
                        ShowInNode(accidentInOldEvent, traceId);
                    return;
                default: return;
            }
        }

        private void ShowBetweenNodes(AccidentAsNewEvent accidentAsNewEvent, Guid traceId)
        {
            var accidentGps = _accidentPlaceLocator.GetAccidentGps(accidentAsNewEvent);
            if (accidentGps == null) return;

            AddAccidentNode((PointLatLng)accidentGps, traceId, accidentAsNewEvent.AccidentSeriousness);
        }

        private void ShowInNode(AccidentInOldEvent accidentInOldEvent, Guid traceId)
        {
            var node = GetNodeByLandmarkIndex(traceId, accidentInOldEvent.BrokenLandmarkIndex);

            if (node == null) return;

            AddAccidentNode(node.Position, traceId, accidentInOldEvent.AccidentSeriousness);
        }

        private void AddAccidentNode(PointLatLng accidentGps, Guid traceId, FiberState state)
        {
            var accidentNode = new Node()
            {
                NodeId = Guid.NewGuid(),
                Position = accidentGps,
                TypeOfLastAddedEquipment = EquipmentType.AccidentPlace,
                AccidentOnTraceId = traceId,
                State = state,
            };
            _model.Nodes.Add(accidentNode);
        }

        private void ShowBadSegment(AccidentInOldEvent accidentInOldEvent, Guid traceId)
        {
            var fibers = GetTraceFibersBetweenLandmarks(traceId, accidentInOldEvent.BrokenLandmarkIndex - 1,
                accidentInOldEvent.BrokenLandmarkIndex);

            foreach (var fiber in fibers)
            {
                fiber.SetBadSegment(traceId, accidentInOldEvent.AccidentSeriousness);
            }
        }


        private Node GetNodeByLandmarkIndex(Guid traceId, int lmIndex)
        {
            var nodesWithoutAdjustmentPoints = _model.GetTraceNodesExcludingAdjustmentPoints(traceId).ToList();
            var nodeId = nodesWithoutAdjustmentPoints[lmIndex];
            return _model.Nodes.First(n => n.NodeId == nodeId);
        }

        // on graph could be more than one fiber between landmarks
        // so we should exclude AdjustmentPoints to find nodes corresponding to landmarks
        // and return all fibers between those nodes
        private IEnumerable<Fiber> GetTraceFibersBetweenLandmarks(Guid traceId, int leftLmIndex, int rightLmIndex)
        {
            var trace = _model.Traces.First(t => t.TraceId == traceId);
            var nodesWithoutAdjustmentPoints = _model.GetTraceNodesExcludingAdjustmentPoints(traceId).ToList();
            var leftNodeId = nodesWithoutAdjustmentPoints[leftLmIndex];
            var rightNodeId = nodesWithoutAdjustmentPoints[rightLmIndex];

            var leftNodeIndexInFull = trace.NodeIds.IndexOf(leftNodeId);
            var rightNodeIndexInFull = trace.NodeIds.IndexOf(rightNodeId);

            for (int i = leftNodeIndexInFull; i < rightNodeIndexInFull; i++)
            {
                yield return _model.Fibers.First(
                    f => f.NodeId1 == trace.NodeIds[i] && f.NodeId2 == trace.NodeIds[i + 1] ||
                         f.NodeId1 == trace.NodeIds[i + 1] && f.NodeId2 == trace.NodeIds[i]);
            }

        }
    }
}
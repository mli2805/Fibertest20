using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public  class AccidentsOnTraceToModelApplier
    {
        private readonly Model _model;

        public AccidentsOnTraceToModelApplier(Model model)
        {
            _model = model;
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

        private void ShowAccidentPlaceOnTrace(AccidentOnTraceV2 accident, Guid traceId)
        {
            if (accident.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                ShowBadSegment(accident, traceId);
            else
                ShowPoint(accident, traceId);
        }

       
        private void ShowPoint(AccidentOnTraceV2 accidentInPoint, Guid traceId)
        {
            AddAccidentNode(accidentInPoint.AccidentCoors, traceId, accidentInPoint.AccidentSeriousness);
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

        private void ShowBadSegment(AccidentOnTraceV2 accidentInOldEvent, Guid traceId)
        {
            var fibers = GetTraceFibersBetweenLandmarks(traceId, accidentInOldEvent.AccidentLandmarkIndex - 1,
                accidentInOldEvent.AccidentLandmarkIndex);

            foreach (var fiber in fibers)
            {
                fiber.SetBadSegment(traceId, accidentInOldEvent.AccidentSeriousness);
            }
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
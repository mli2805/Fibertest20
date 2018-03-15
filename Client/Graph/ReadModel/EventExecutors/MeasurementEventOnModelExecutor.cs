﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.Graph
{
    public class MeasurementEventOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly IModel _model;
        private readonly AccidentPlaceLocator _accidentPlaceLocator;

        public MeasurementEventOnModelExecutor(ReadModel model, AccidentPlaceLocator accidentPlaceLocator)
        {
            _model = model;
            _accidentPlaceLocator = accidentPlaceLocator;
        }

        public string ShowMonitoringResult(MeasurementAdded e)
        {
            _model.Measurements.Add(_mapper.Map<Measurement>(e));

            var trace = _model.Traces.FirstOrDefault(t => t.Id == e.TraceId);
            if (trace == null) return null;

            trace.State = e.TraceState;
            foreach (var fiber in _model.GetTraceFibers(trace))
                fiber.SetState(trace.Id, trace.State);

            CleanAccidentPlacesOnTrace(trace.Id);

            if (e.TraceState != FiberState.Ok && e.TraceState != FiberState.NoFiber)
                e.Accidents.ForEach(a => ShowAccidentPlaceOnTrace(a, e.TraceId));
            return null;
        }

        public string UpdateMeasurement(MeasurementUpdated e)
        {
            var destination = _model.Measurements.First(f => f.SorFileId == e.SorFileId);
            _mapper.Map(e, destination);
            return null;
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

            AddAccidentNode((PointLatLng)accidentGps, traceId);
        }

        private void ShowInNode(AccidentInOldEvent accidentInOldEvent, Guid traceId)
        {
            var node = GetNodeByLandmarkIndex(traceId, accidentInOldEvent.BrokenLandmarkIndex);

            if (node == null) return;

            AddAccidentNode(node.Position, traceId);
        }

        private void AddAccidentNode(PointLatLng accidentGps, Guid traceId)
        {
            var accidentNode = new Node()
            {
                Id = Guid.NewGuid(),
                Position = accidentGps,
                TypeOfLastAddedEquipment = EquipmentType.AccidentPlace,
                AccidentOnTraceId = traceId,
            };
            _model.Nodes.Add(accidentNode);
        }

        private void ShowBadSegment(AccidentInOldEvent accidentInOldEvent, Guid traceId)
        {
            var fibers = GetTraceFibersBetweenLandmarks(traceId, accidentInOldEvent.BrokenLandmarkIndex - 1,
                accidentInOldEvent.BrokenLandmarkIndex);

            foreach (var fiber in fibers)
            {
                fiber.AddBadSegment(traceId);
            }
        }

        private void CleanAccidentPlacesOnTrace(Guid traceId)
        {
            var nodes = _model.Nodes.Where(n => n.AccidentOnTraceId == traceId).ToList();
            foreach (var node in nodes)
            {
                _model.Nodes.Remove(node);
            }

            foreach (var fiber in _model.Fibers)
            {
                fiber.CleanBadSegment(traceId);
            }
        }


        private Node GetNodeByLandmarkIndex(Guid traceId, int lmIndex)
        {
            var nodesWithoutAdjustmentPoints = _model.GetTraceNodesExcludingAdjustmentPoints(traceId).ToList();
            var nodeId = nodesWithoutAdjustmentPoints[lmIndex];
            return _model.Nodes.First(n=>n.Id == nodeId);
        }

        // on graph could be more than one fiber between landmarks
        // so we should exclude AdjustmentPoints to find nodes corresponding to landmarks
        // and return all fibers between those nodes
        private IEnumerable<Fiber> GetTraceFibersBetweenLandmarks(Guid traceId, int leftLmIndex, int rightLmIndex)
        {
            var trace = _model.Traces.First(t => t.Id == traceId);
            var nodesWithoutAdjustmentPoints = _model.GetTraceNodesExcludingAdjustmentPoints(traceId).ToList();
            var leftNodeId = nodesWithoutAdjustmentPoints[leftLmIndex];
            var rightNodeId = nodesWithoutAdjustmentPoints[rightLmIndex];

            var leftNodeIndexInFull = trace.Nodes.IndexOf(leftNodeId);
            var rightNodeIndexInFull = trace.Nodes.IndexOf(rightNodeId);

            for (int i = leftNodeIndexInFull; i < rightNodeIndexInFull; i++)
            {
                yield return _model.Fibers.First(
                    f => f.Node1 == trace.Nodes[i] && f.Node2 == trace.Nodes[i + 1] ||
                         f.Node1 == trace.Nodes[i + 1] && f.Node2 == trace.Nodes[i]);
            }

        }
    }
}
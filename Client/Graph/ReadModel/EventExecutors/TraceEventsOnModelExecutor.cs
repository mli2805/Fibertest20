﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.Algorithms;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class TraceEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly IModel _model;
        private readonly IMyLog _logFile;
        private readonly AccidentsOnTraceApplierToReadModel _accidentsOnTraceApplierToReadModel;

        public TraceEventsOnModelExecutor(ReadModel model, IMyLog logFile,
            AccidentsOnTraceApplierToReadModel accidentsOnTraceApplierToReadModel)
        {
            _model = model;
            _logFile = logFile;
            _accidentsOnTraceApplierToReadModel = accidentsOnTraceApplierToReadModel;
        }
        public string AddTrace(TraceAdded e)
        {
            Trace trace = _mapper.Map<Trace>(e);
            trace.ZoneIds.Add(_model.Zones.First(z => z.IsDefaultZone).ZoneId);
            _model.Traces.Add(trace);
            for (int i = 1; i < trace.Nodes.Count; i++)
                GetFiberBetweenNodes(trace.Nodes[i - 1], trace.Nodes[i]).SetState(trace.Id, FiberState.NotJoined);
            return null;
        }

        public string UpdateTrace(TraceUpdated source)
        {
            var destination = _model.Traces.First(t => t.Id == source.Id);
            _mapper.Map(source, destination);
            return null;
        }

        public string CleanTrace(TraceCleaned e)
        {
            var trace = _model.Traces.FirstOrDefault(t => t.Id == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceCleaned: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }
            var traceFibers = GetTraceFibersByNodes(trace.Nodes).ToList();
            foreach (var fiber in traceFibers)
            {
                fiber.TracesWithExceededLossCoeff.Remove(trace.Id);
                if (fiber.States.ContainsKey(trace.Id))
                    fiber.States.Remove(trace.Id);
            }

            _model.Traces.Remove(trace);
            return null;
        }

        private IEnumerable<Fiber> GetTraceFibersByNodes(List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberBetweenNodes(nodes[i - 1], nodes[i]);
        }

        private Fiber GetFiberBetweenNodes(Guid node1, Guid node2)
        {
            return _model.Fibers.First(
                f => f.Node1 == node1 && f.Node2 == node2 ||
                     f.Node1 == node2 && f.Node2 == node1);
        }

        public string RemoveTrace(TraceRemoved e)
        {
            var trace = _model.Traces.FirstOrDefault(t => t.Id == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceRemoved: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }

            var traceFibers = GetTraceFibersByNodes(trace.Nodes).ToList();
            foreach (var fiber in traceFibers)
            {
                if (_model.Traces.Where(t => t.Id != e.TraceId).All(t => Topo.GetFiberIndexInTrace(t, fiber) == -1))
                    _model.Fibers.Remove(fiber);
            }

            foreach (var traceNodeId in trace.Nodes)
            {
                if (_model.Fibers.Any(f => f.Node1 == traceNodeId || f.Node2 == traceNodeId))
                    continue;

                var node = _model.Nodes.First(n => n.Id == traceNodeId);
                if (node.TypeOfLastAddedEquipment != EquipmentType.Rtu)
                    _model.Nodes.Remove(node);
            }

            _model.Traces.Remove(trace);
            return null;
        }

        public string AttachTrace(TraceAttached e)
        {
            var trace = _model.Traces.First(t => t.Id == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceAttached: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }

            trace.Port = e.OtauPortDto.OpticalPort;
            trace.OtauPort = e.OtauPortDto;

            _accidentsOnTraceApplierToReadModel.ShowMonitoringResult(new MeasurementAdded()
            {
                TraceId = e.TraceId,
                TraceState = e.PreviousTraceState,
                Accidents = e.AccidentsInLastMeasurement,
            });
            return null;
        }

        public string DetachTrace(TraceDetached e)
        {
            var trace = _model.Traces.First(t => t.Id == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceDetached: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }
            trace.Port = -1;
            trace.OtauPort = null;
            trace.IsIncludedInMonitoringCycle = false;
            trace.State = FiberState.NotJoined;
            foreach (var fiber in GetTraceFibersByNodes(trace.Nodes))
            {
                fiber.SetState(trace.Id, FiberState.NotJoined);
            }

            _accidentsOnTraceApplierToReadModel.CleanAccidentPlacesOnTrace(trace.Id);
            return null;
        }
    }
}
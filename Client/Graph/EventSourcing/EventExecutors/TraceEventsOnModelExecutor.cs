using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class TraceEventsOnModelExecutor
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly Model _model;
        private readonly IMyLog _logFile;
        private readonly AccidentsOnTraceToModelApplier _accidentsOnTraceToModelApplier;

        public TraceEventsOnModelExecutor(Model model, IMyLog logFile,
            AccidentsOnTraceToModelApplier accidentsOnTraceToModelApplier)
        {
            _model = model;
            _logFile = logFile;
            _accidentsOnTraceToModelApplier = accidentsOnTraceToModelApplier;
        }
        public string AddTrace(TraceAdded e)
        {
            Trace trace = Mapper.Map<Trace>(e);
            trace.ZoneIds.Add(_model.Zones.First(z => z.IsDefaultZone).ZoneId);
            _model.Traces.Add(trace);

            foreach (var fiberId in trace.FiberIds)
                _model.Fibers.First(f=>f.FiberId == fiberId).SetState(trace.TraceId, FiberState.NotJoined);
            return null;
        }

        public string UpdateTrace(TraceUpdated source)
        {
            var destination = _model.Traces.First(t => t.TraceId == source.Id);
            Mapper.Map(source, destination);
            return null;
        }

        public string CleanTrace(TraceCleaned e)
        {
           var trace = _model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceCleaned: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }
            var traceFibers = GetTraceFibersByNodes(trace.NodeIds).ToList();
            foreach (var fiber in traceFibers)
            {
                fiber.TracesWithExceededLossCoeff.Remove(trace.TraceId);
                if (fiber.States.ContainsKey(trace.TraceId))
                    fiber.States.Remove(trace.TraceId);
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
                f => f.NodeId1 == node1 && f.NodeId2 == node2 ||
                     f.NodeId1 == node2 && f.NodeId2 == node1);
        }

        public string RemoveTrace(TraceRemoved e)
        {
            var trace = _model.Traces.FirstOrDefault(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceRemoved: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }

            var traceFibers = GetTraceFibersByNodes(trace.NodeIds).ToList();
            foreach (var fiber in traceFibers)
            {
                if (_model.Traces.Where(t => t.TraceId != e.TraceId).All(t => t.FiberIds.IndexOf(fiber.FiberId) == -1))
                    _model.Fibers.Remove(fiber);
                else
                {
                    fiber.TracesWithExceededLossCoeff.Remove(trace.TraceId);
                    if (fiber.States.ContainsKey(trace.TraceId))
                        fiber.States.Remove(trace.TraceId);
                }
            }

            foreach (var traceNodeId in trace.NodeIds)
            {
                if (_model.Fibers.Any(f => f.NodeId1 == traceNodeId || f.NodeId2 == traceNodeId))
                    continue;

                var node = _model.Nodes.FirstOrDefault(n => n.NodeId == traceNodeId); // FirstOrDefault because of possible repetitions in trace
                if (node?.TypeOfLastAddedEquipment != EquipmentType.Rtu)
                    _model.Nodes.Remove(node);
            }

            _model.Traces.Remove(trace);
            return null;
        }

        public string AttachTrace(TraceAttached e)
        {
            var trace = _model.Traces.First(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceAttached: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }

            trace.Port = e.OtauPortDto.OpticalPort;
            trace.OtauPort = e.OtauPortDto;

            _accidentsOnTraceToModelApplier.ShowMonitoringResult(new MeasurementAdded()
            {
                TraceId = e.TraceId,
                TraceState = e.PreviousTraceState,
                Accidents = e.AccidentsInLastMeasurement,
            });
            return null;
        }

        public string DetachTrace(TraceDetached e)
        {
            var trace = _model.Traces.First(t => t.TraceId == e.TraceId);
            if (trace == null)
            {
                var message = $@"TraceDetached: Trace {e.TraceId} not found";
                _logFile.AppendLine(message);
                return message;
            }

            DetachTrace(trace);
            return null;
        }

        public void DetachTrace(Trace trace)
        {
            trace.Port = -1;
            trace.OtauPort = null;
            trace.IsIncludedInMonitoringCycle = false;
            trace.State = FiberState.NotJoined;
            foreach (var fiber in GetTraceFibersByNodes(trace.NodeIds))
                fiber.SetState(trace.TraceId, FiberState.NotJoined);

            _accidentsOnTraceToModelApplier.CleanAccidentPlacesOnTrace(trace.TraceId);
        }
    }
}
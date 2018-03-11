using System;
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

        public TraceEventsOnModelExecutor(ReadModel model, IMyLog logFile)
        {
            _model = model;
            _logFile = logFile;
        }
        public string AddTrace(TraceAdded e)
        {
            Trace trace = _mapper.Map<Trace>(e);
            _model.Traces.Add(trace);
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
            var trace = _model.Traces.FirstOrDefault(t => t.Id == e.Id);
            if (trace != null)
            {
                _model.Traces.Remove(trace);
                return null;
            }
            var message = $@"TraceCleaned: Trace {e.Id} not found";
            _logFile.AppendLine(message);
            return message;
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
            var trace = _model.Traces.FirstOrDefault(t => t.Id == e.Id);
            if (trace == null)
            {
                var message = $@"TraceRemoved: Trace {e.Id} not found";
                _logFile.AppendLine(message);
                return message;
            }

            var traceFibers = GetTraceFibersByNodes(trace.Nodes).ToList();
            foreach (var fiber in traceFibers)
            {
                if (_model.Traces.All(t => Topo.GetFiberIndexInTrace(t, fiber) == -1))
                    _model.Fibers.Remove(fiber);
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
            trace.State = FiberState.Ok;
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
            return null;
        }
    }
}
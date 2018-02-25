using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.Graph
{
    public class TraceEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly IModel _model;

        public TraceEventsOnModelExecutor(ReadModel model)
        {
            _model = model;
        }
        public void AddTrace(TraceAdded e)
        {
            Trace trace = _mapper.Map<Trace>(e);
            _model.Traces.Add(trace);
        }

        public void UpdateTrace(TraceUpdated source)
        {
            var destination = _model.Traces.First(t => t.Id == source.Id);
            _mapper.Map(source, destination);
        }

        public void CleanTrace(TraceCleaned e)
        {
            var trace = _model.Traces.First(t => t.Id == e.Id);
            _model.Traces.Remove(trace);
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

        public void RemoveTrace(TraceRemoved e)
        {
            var traceVm = _model.Traces.First(t => t.Id == e.Id);
            var traceFibers = GetTraceFibersByNodes(traceVm.Nodes).ToList();
            foreach (var fiber in traceFibers)
            {
                if (_model.Traces.All(trace => Topo.GetFiberIndexInTrace(trace, fiber) == -1))
                    _model.Fibers.Remove(fiber);
            }
            _model.Traces.Remove(traceVm);
        }

        public void AttachTrace(TraceAttached e)
        {
            var trace = _model.Traces.First(t => t.Id == e.TraceId);
            trace.Port = e.OtauPortDto.OpticalPort;
            trace.OtauPort = e.OtauPortDto;
        }

        public void DetachTrace(TraceDetached e)
        {
            var trace = _model.Traces.First(t => t.Id == e.TraceId);
            trace.Port = -1;
            trace.OtauPort = null;
            trace.IsIncludedInMonitoringCycle = false;
        }
    }
}
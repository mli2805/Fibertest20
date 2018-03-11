﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;

        public TraceEventsOnGraphExecutor(GraphReadModel model)
        {
            _model = model;
        }

        public void AddTrace(TraceAdded evnt)
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingEventToVm>()).CreateMapper();
            var traceVm = mapper.Map<TraceVm>(evnt);
            _model.Data.Traces.Add(traceVm);

            ApplyTraceStateToFibers(traceVm);
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
            var traceVm = _model.Data.Traces.First(t => t.Id == evnt.Id);
            GetTraceFibersByNodes(traceVm.Nodes).ToList().ForEach(f => f.RemoveState(evnt.Id));
            _model.Data.Traces.Remove(traceVm);
        }

        public void RemoveTrace(TraceRemoved evnt)
        {
            var traceVm = _model.Data.Traces.First(t => t.Id == evnt.Id);
            var traceFibers = GetTraceFibersByNodes(traceVm.Nodes).ToList();
            foreach (var fiberVm in traceFibers)
            {
                fiberVm.RemoveState(evnt.Id);
                if (fiberVm.State == FiberState.NotInTrace)
                    _model.Data.Fibers.Remove(fiberVm);
            }
            foreach (var nodeId in traceVm.Nodes)
            {
//                if (_model.Data.Rtus.Any(r => r.Node.Id == nodeId) ||
                   if ( _model.Data.Fibers.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId))
                    continue;
                var nodeVm = _model.Data.Nodes.First(n => n.Id == nodeId);
                _model.Data.Nodes.Remove(nodeVm);
            }
            _model.Data.Traces.Remove(traceVm);
        }

        public void AttachTrace(TraceAttached evnt)
        {
            var traceVm = _model.Data.Traces.First(t => t.Id == evnt.TraceId);
            traceVm.State = FiberState.Unknown;
            traceVm.Port = evnt.OtauPortDto.OpticalPort;
            ApplyTraceStateToFibers(traceVm);
        }

        public void DetachTrace(TraceDetached evnt)
        {
            var traceVm = _model.Data.Traces.First(t => t.Id == evnt.TraceId);
            traceVm.State = FiberState.NotJoined;
            traceVm.Port = 0;
            ApplyTraceStateToFibers(traceVm);
            _model.CleanAccidentPlacesOnTrace(traceVm);
        }

        private void ApplyTraceStateToFibers(TraceVm traceVm)
        {
            foreach (var fiberVm in GetTraceFibersByNodes(traceVm.Nodes))
                fiberVm.SetState(traceVm.Id, traceVm.State);
        }
       
    }
}
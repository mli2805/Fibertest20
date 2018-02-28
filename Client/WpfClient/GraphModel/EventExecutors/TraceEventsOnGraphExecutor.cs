using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly AccidentPlaceLocator _accidentPlaceLocator;

        public TraceEventsOnGraphExecutor(GraphReadModel model, AccidentPlaceLocator accidentPlaceLocator)
        {
            _model = model;
            _accidentPlaceLocator = accidentPlaceLocator;
        }

        public void AddTrace(TraceAdded evnt)
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingEventToVm>()).CreateMapper();
            var traceVm = mapper.Map<TraceVm>(evnt);
            _model.Traces.Add(traceVm);

            ApplyTraceStateToFibers(traceVm);
        }

        private IEnumerable<FiberVm> GetTraceFibersByNodes(List<Guid> nodes)
        {
            for (int i = 1; i < nodes.Count; i++)
                yield return GetFiberBetweenNodes(nodes[i - 1], nodes[i]);
        }

        private FiberVm GetFiberBetweenNodes(Guid node1, Guid node2)
        {
            return _model.Fibers.First(
                f => f.Node1.Id == node1 && f.Node2.Id == node2 ||
                     f.Node1.Id == node2 && f.Node2.Id == node1);
        }

        public void CleanTrace(TraceCleaned evnt)
        {
            var traceVm = _model.Traces.First(t => t.Id == evnt.Id);
            GetTraceFibersByNodes(traceVm.Nodes).ToList().ForEach(f => f.RemoveState(evnt.Id));
            _model.Traces.Remove(traceVm);
        }

        public void RemoveTrace(TraceRemoved evnt)
        {
            var traceVm = _model.Traces.First(t => t.Id == evnt.Id);
            var traceFibers = GetTraceFibersByNodes(traceVm.Nodes).ToList();
            foreach (var fiberVm in traceFibers)
            {
                fiberVm.RemoveState(evnt.Id);
                if (fiberVm.State == FiberState.NotInTrace)
                    _model.Fibers.Remove(fiberVm);
            }
            foreach (var nodeId in traceVm.Nodes)
            {
                if (_model.Rtus.Any(r => r.Node.Id == nodeId) ||
                    _model.Fibers.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId))
                    continue;
                var nodeVm = _model.Nodes.First(n => n.Id == nodeId);
                _model.Nodes.Remove(nodeVm);
            }
            _model.Traces.Remove(traceVm);
        }

        public void AttachTrace(TraceAttached evnt)
        {
            var traceVm = _model.Traces.First(t => t.Id == evnt.TraceId);
            traceVm.State = FiberState.Unknown;
            traceVm.Port = evnt.OtauPortDto.OpticalPort;
            ApplyTraceStateToFibers(traceVm);
        }

        public void DetachTrace(TraceDetached evnt)
        {
            var traceVm = _model.Traces.First(t => t.Id == evnt.TraceId);
            traceVm.State = FiberState.NotJoined;
            traceVm.Port = 0;
            ApplyTraceStateToFibers(traceVm);
            CleanAccidentPlacesOnTrace(traceVm);
        }

        private void ApplyTraceStateToFibers(TraceVm traceVm)
        {
            foreach (var fiberVm in GetTraceFibersByNodes(traceVm.Nodes))
                fiberVm.SetState(traceVm.Id, traceVm.State);
        }

        public void ShowMonitoringResult(MonitoringResultShown evnt)
        {
            var traceVm = _model.Traces.First(t => t.Id == evnt.TraceId);
            traceVm.State = evnt.TraceState;
            _model.ChangeTraceColor(evnt.TraceId, traceVm.Nodes, traceVm.State);
            if (traceVm.State == FiberState.Ok)
                CleanAccidentPlacesOnTrace(traceVm);
            else
                ShowAccidentPlacesOnTrace(evnt, traceVm);
        }

        private void ShowAccidentPlacesOnTrace(MonitoringResultShown evnt, TraceVm traceVm)
        {
            foreach (var accident in evnt.Accidents)
            {
                var accidentGps = _accidentPlaceLocator.GetAccidentGps(accident, traceVm);
                if (accidentGps == null)
                    continue;

                var accidentNode = new NodeVm()
                {
                    Id = Guid.NewGuid(),
                    Position = (PointLatLng)accidentGps,
                    Type = EquipmentType.AccidentPlace,
                    AccidentOnTraceVmId = traceVm.Id,
                };
                _model.Equipments.Add(new EquipmentVm()
                {
                    Id = Guid.NewGuid(),
                    Node = accidentNode,
                    Type = EquipmentType.AccidentPlace
                });
                _model.Nodes.Add(accidentNode);
            }
        }

        private void CleanAccidentPlacesOnTrace(TraceVm traceVm)
        {
            var nodeVms = _model.Nodes.Where(n => n.AccidentOnTraceVmId == traceVm.Id).ToList();
            foreach (var nodeVm in nodeVms)
            {
                var equipmentVm = _model.Equipments.FirstOrDefault(e => e.Node == nodeVm);
                if (equipmentVm != null)
                    _model.Equipments.Remove(equipmentVm);
                _model.Nodes.Remove(nodeVm);
            }
        }
    }
}
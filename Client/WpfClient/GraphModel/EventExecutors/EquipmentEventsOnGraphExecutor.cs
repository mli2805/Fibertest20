using System;
using System.Linq;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class EquipmentEventsOnGraphExecutor
    {
        readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingEventToVm>()).CreateMapper();
        private readonly GraphReadModel _model;
        private readonly IMyLog _logFile;

        public EquipmentEventsOnGraphExecutor(GraphReadModel model, IMyLog logFile)
        {
            _model = model;
            _logFile = logFile;
        }

        public void AddEquipmentAtGpsLocation(EquipmentAtGpsLocationAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = evnt.Type,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude)
            };
            _model.Data.Nodes.Add(nodeVm);

            if (evnt.RequestedEquipmentId != Guid.Empty)
                _model.Data.Equipments.Add(new EquipmentVm() { Id = evnt.RequestedEquipmentId, Node = nodeVm, Type = evnt.Type });
            if (evnt.EmptyNodeEquipmentId != Guid.Empty)
                _model.Data.Equipments.Add(new EquipmentVm() { Id = evnt.EmptyNodeEquipmentId, Node = nodeVm, Type = EquipmentType.EmptyNode });
        }

        public void AddEquipmentAtGpsLocationWithNodeTitle(EquipmentAtGpsLocationWithNodeTitleAdded evnt)
        {
            var nodeVm = new NodeVm()
            {
                Id = evnt.NodeId,
                State = FiberState.Ok,
                Type = evnt.Type,
                Position = new PointLatLng(evnt.Latitude, evnt.Longitude),
                Title = evnt.Title,
            };
            _model.Data.Nodes.Add(nodeVm);

            if (evnt.RequestedEquipmentId != Guid.Empty)
                _model.Data.Equipments.Add(new EquipmentVm() { Id = evnt.RequestedEquipmentId, Node = nodeVm, Type = evnt.Type });
            if (evnt.EmptyNodeEquipmentId != Guid.Empty)
                _model.Data.Equipments.Add(new EquipmentVm() { Id = evnt.EmptyNodeEquipmentId, Node = nodeVm, Type = EquipmentType.EmptyNode });
        }

        public void AddEquipmentIntoNode(EquipmentIntoNodeAdded evnt)
        {
            EquipmentVm equipmentVm = _mapper.Map<EquipmentVm>(evnt);
            var nodeVm = _model.Data.Nodes.First(n => n.Id == evnt.NodeId);
            equipmentVm.Node = nodeVm;
            _model.Data.Equipments.Add(equipmentVm);
            nodeVm.Type = equipmentVm.Type;

            foreach (var traceId in evnt.TracesForInsertion)
            {
                var traceVm = _model.Data.Traces.FirstOrDefault(t => t.Id == traceId);
                if (traceVm == null)
                {
                    var message = $@"EquipmentIntoNodeAdded: Trace {traceId.First6()} not found";
                    _logFile.AppendLine(message);
                    return;
                }
                var idx = traceVm.Nodes.IndexOf(evnt.NodeId);
                traceVm.Equipments[idx] = evnt.Id;
            }
        }

        public void UpdateEquipment(EquipmentUpdated evnt)
        {
            var equipmentVm = _model.Data.Equipments.First(e => e.Id == evnt.Id);
            _mapper.Map(evnt, equipmentVm);
            var nodeVm = equipmentVm.Node;
            nodeVm.Type = evnt.Type;
        }

        public void RemoveEquipment(EquipmentRemoved evnt)
        {
            var equipmentVm = _model.Data.Equipments.FirstOrDefault(e => e.Id == evnt.Id);
            if (equipmentVm == null)
            {
                var message = $@"EquipmentRemoved: Equipment {evnt.Id.First6()} not found";
                _logFile.AppendLine(message);
                return;
            }

            #region replace equipment in trace with emptyEquipment
            var emptyEquipment = _model.Data.Equipments.FirstOrDefault(eq => eq.Node.Id == equipmentVm.Node.Id && eq.Type == EquipmentType.EmptyNode);
            if (emptyEquipment == null)
            {
                var message = $@"EquipmentRemoved: There is no empty equipment in node {equipmentVm.Node.Id.First6()}";
                _logFile.AppendLine(message);
                return;
            }

            var traces = _model.Data.Traces.Where(t => t.Equipments.Contains(evnt.Id)).ToList();
            foreach (var traceVm in traces)
            {
                var idx = traceVm.Equipments.IndexOf(evnt.Id);
                traceVm.Equipments[idx] = emptyEquipment.Id;
            }
            #endregion

            var nodeVm = equipmentVm.Node;

            _model.Data.Equipments.Remove(equipmentVm);

            var majorEquipmentInNode = _model.Data.Equipments.Where(e => e.Node.Id == nodeVm.Id).Max(e => e.Type);
            nodeVm.Type = majorEquipmentInNode;
        }

    }
}
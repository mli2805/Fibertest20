using System;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph
{
    public class EquipmentEventsOnModelExecutor
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly IModel _model;
        private readonly IMyLog _logFile;


        public EquipmentEventsOnModelExecutor(ReadModel model, IMyLog logFile)
        {
            _logFile = logFile;
            _model = model;
        }
        public void AddEquipmentIntoNode(EquipmentIntoNodeAdded e)
        {
            Equipment equipment = _mapper.Map<Equipment>(e);
            _model.Equipments.Add(equipment);
            foreach (var traceId in e.TracesForInsertion)
            {
                var trace = _model.Traces.Single(t => t.Id == traceId);
                var idx = trace.Nodes.IndexOf(e.NodeId);
                trace.Equipments[idx] = e.Id;
            }
        }
        public void AddEquipmentAtGpsLocation(EquipmentAtGpsLocationAdded e)
        {
            Node node = new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude };
            _model.Nodes.Add(node);
            Equipment equipment = _mapper.Map<Equipment>(e);
            equipment.Id = e.RequestedEquipmentId;
            _model.Equipments.Add(equipment);
            if (e.EmptyNodeEquipmentId != Guid.Empty)
            {
                Equipment emptyEquipment = _mapper.Map<Equipment>(e);
                emptyEquipment.Id = e.EmptyNodeEquipmentId;
                emptyEquipment.Type = EquipmentType.EmptyNode;
                _model.Equipments.Add(emptyEquipment);
            }
        }

        public void UpdateEquipment(EquipmentUpdated e)
        {
            var equipment = _model.Equipments.FirstOrDefault(eq => eq.Id == e.Id);
            _mapper.Map(e, equipment);
        }

        public void RemoveEquipment(EquipmentRemoved e)
        {
            var equipment = _model.Equipments.FirstOrDefault(eq => eq.Id == e.Id);
            if (equipment == null)
            {
                var message = $@"EquipmentRemoved: Equipment {e.Id.First6()} not found";
                _logFile.AppendLine(message);
                return;
            }

            var emptyEquipment = _model.Equipments.FirstOrDefault(eq => eq.NodeId == equipment.NodeId && eq.Type == EquipmentType.EmptyNode);
            if (emptyEquipment == null)
            {
                var message = $@"EquipmentRemoved: There is no empty equipment in node {equipment.NodeId.First6()}";
                _logFile.AppendLine(message);
                return;
            }

            var traces = _model.Traces.Where(t => t.Equipments.Contains(e.Id)).ToList();
            foreach (var trace in traces)
            {
                var idx = trace.Equipments.IndexOf(e.Id);
                trace.Equipments[idx] = emptyEquipment.Id;
            }
            _model.Equipments.Remove(_model.Equipments.First(eq => eq.Id == e.Id));
        }
    }
}
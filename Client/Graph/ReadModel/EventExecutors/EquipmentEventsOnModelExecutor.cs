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
        public string AddEquipmentIntoNode(EquipmentIntoNodeAdded e)
        {
            Equipment equipment = _mapper.Map<Equipment>(e);
            _model.Equipments.Add(equipment);
            foreach (var traceId in e.TracesForInsertion)
            {
                var trace = _model.Traces.FirstOrDefault(t => t.Id == traceId);
                if (trace == null)
                {
                    var message = $@"EquipmentIntoNodeAdded: Trace {traceId.First6()} not found";
                    _logFile.AppendLine(message);
                    return message;
                }
                var idx = trace.Nodes.IndexOf(e.NodeId);
                trace.Equipments[idx] = e.Id;
            }
            return null;
        }

        public string AddEquipmentAtGpsLocation(EquipmentAtGpsLocationAdded e)
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
            return null;
        }

        public string AddEquipmentAtGpsLocationWithNodeTitle(EquipmentAtGpsLocationWithNodeTitleAdded e)
        {
            _model.Nodes.Add(new Node() { Id = e.NodeId, Latitude = e.Latitude, Longitude = e.Longitude, Title = e.Title, Comment = e.Comment });

            if (e.RequestedEquipmentId != Guid.Empty)
                _model.Equipments.Add(new Equipment() { Id = e.RequestedEquipmentId, Type = e.Type, NodeId = e.NodeId });

            if (e.EmptyNodeEquipmentId != Guid.Empty)
                _model.Equipments.Add(new Equipment() { Id = e.EmptyNodeEquipmentId, Type = EquipmentType.EmptyNode, NodeId = e.NodeId });

            return null;
        }

        public string UpdateEquipment(EquipmentUpdated e)
        {
            var equipment = _model.Equipments.FirstOrDefault(eq => eq.Id == e.Id);
            if (equipment == null)
            {
                var message = $@"EquipmentUpdated: Equipment {e.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            _mapper.Map(e, equipment);
            return null;
        }

        public string RemoveEquipment(EquipmentRemoved e)
        {
            var equipment = _model.Equipments.FirstOrDefault(eq => eq.Id == e.Id);
            if (equipment == null)
            {
                var message = $@"EquipmentRemoved: Equipment {e.Id.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }

            var emptyEquipment = _model.Equipments.FirstOrDefault(eq => eq.NodeId == equipment.NodeId && eq.Type == EquipmentType.EmptyNode);
            if (emptyEquipment == null)
            {
                var message = $@"EquipmentRemoved: There is no empty equipment in node {equipment.NodeId.First6()}";
                _logFile.AppendLine(message);
                return message;
            }

            var traces = _model.Traces.Where(t => t.Equipments.Contains(e.Id)).ToList();
            foreach (var trace in traces)
            {
                var idx = trace.Equipments.IndexOf(e.Id);
                trace.Equipments[idx] = emptyEquipment.Id;
            }
            _model.Equipments.Remove(_model.Equipments.First(eq => eq.Id == e.Id));
            return null;
        }
    }
}
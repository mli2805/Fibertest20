using System;
using System.Linq;
using AutoMapper;
using GMap.NET;
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


        public EquipmentEventsOnModelExecutor(IModel model, IMyLog logFile)
        {
            _logFile = logFile;
            _model = model;
        }
        public string AddEquipmentIntoNode(EquipmentIntoNodeAdded e)
        {
            var node = _model.Nodes.First(n => n.NodeId == e.NodeId);
            node.TypeOfLastAddedEquipment = e.Type;
            Equipment equipment = _mapper.Map<Equipment>(e);
            _model.Equipments.Add(equipment);
            foreach (var traceId in e.TracesForInsertion)
            {
                var trace = _model.Traces.FirstOrDefault(t => t.TraceId == traceId);
                if (trace == null)
                {
                    var message = $@"EquipmentIntoNodeAdded: Trace {traceId.First6()} not found";
                    _logFile.AppendLine(message);
                    return message;
                }
                var idx = trace.NodeIds.IndexOf(e.NodeId);
                trace.EquipmentIds[idx] = e.EquipmentId;
            }
            return null;
        }

        public string AddEquipmentAtGpsLocation(EquipmentAtGpsLocationAdded e)
        {
            Node node = new Node() { NodeId = e.NodeId, Position = new PointLatLng(e.Latitude, e.Longitude), TypeOfLastAddedEquipment = e.Type };
            _model.Nodes.Add(node);
            Equipment equipment = _mapper.Map<Equipment>(e);
            equipment.EquipmentId = e.RequestedEquipmentId;
            _model.Equipments.Add(equipment);
            if (e.EmptyNodeEquipmentId != Guid.Empty)
            {
                Equipment emptyEquipment = _mapper.Map<Equipment>(e);
                emptyEquipment.EquipmentId = e.EmptyNodeEquipmentId;
                emptyEquipment.Type = EquipmentType.EmptyNode;
                _model.Equipments.Add(emptyEquipment);
            }
            return null;
        }

        public string AddEquipmentAtGpsLocationWithNodeTitle(EquipmentAtGpsLocationWithNodeTitleAdded e)
        {
            _model.Nodes.Add(new Node() { NodeId = e.NodeId, Position = new PointLatLng(e.Latitude, e.Longitude),
                TypeOfLastAddedEquipment = e.Type, Title = e.Title, Comment = e.Comment });

            if (e.RequestedEquipmentId != Guid.Empty)
                _model.Equipments.Add(new Equipment() { EquipmentId = e.RequestedEquipmentId, Type = e.Type, NodeId = e.NodeId });

            if (e.EmptyNodeEquipmentId != Guid.Empty)
                _model.Equipments.Add(new Equipment() { EquipmentId = e.EmptyNodeEquipmentId, Type = EquipmentType.EmptyNode, NodeId = e.NodeId });

            return null;
        }

        public string UpdateEquipment(EquipmentUpdated e)
        {
            var equipment = _model.Equipments.FirstOrDefault(eq => eq.EquipmentId == e.EquipmentId);
            if (equipment == null)
            {
                var message = $@"EquipmentUpdated: Equipment {e.EquipmentId.First6()} not found";
                _logFile.AppendLine(message);
                return message;
            }
            var node = _model.Nodes.First(n => n.NodeId == equipment.NodeId);
            node.TypeOfLastAddedEquipment = e.Type;
            _mapper.Map(e, equipment);
            return null;
        }

        public string RemoveEquipment(EquipmentRemoved e)
        {
            var equipment = _model.Equipments.FirstOrDefault(eq => eq.EquipmentId == e.EquipmentId);
            if (equipment == null)
            {
                var message = $@"EquipmentRemoved: Equipment {e.EquipmentId.First6()} not found";
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

            var traces = _model.Traces.Where(t => t.EquipmentIds.Contains(e.EquipmentId)).ToList();
            foreach (var trace in traces)
            {
                var idx = trace.EquipmentIds.IndexOf(e.EquipmentId);
                trace.EquipmentIds[idx] = emptyEquipment.EquipmentId;
            }

            var node = _model.Nodes.First(n => n.NodeId == equipment.NodeId);

//            _model.Equipments.Remove(_model.Equipments.First(eq => eq.Id == e.Id));
            _model.Equipments.Remove(equipment);

            node.TypeOfLastAddedEquipment = _model.Equipments.Where(p => p.NodeId == node.NodeId).Max(q => q.Type);

            return null;
        }
    }
}
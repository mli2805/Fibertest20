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
            _model.Nodes.Add(nodeVm);

            _model.Equipments.Add(new EquipmentVm() { Id = evnt.RequestedEquipmentId, Node = nodeVm, Type = evnt.Type });
            if (evnt.EmptyNodeEquipmentId != Guid.Empty)
                _model.Equipments.Add(new EquipmentVm() { Id = evnt.EmptyNodeEquipmentId, Node = nodeVm, Type = EquipmentType.EmptyNode });
        }

        public void AddEquipmentIntoNode(EquipmentIntoNodeAdded evnt)
        {
            var nodeVm = _model.Nodes.First(n => n.Id == evnt.NodeId);
            _model.Equipments.Add(new EquipmentVm() { Id = evnt.Id, Node = nodeVm, Type = evnt.Type });
            nodeVm.Type = evnt.Type;
        }

        public void UpdateEquipment(EquipmentUpdated evnt)
        {
            var equipmentVm = _model.Equipments.First(e => e.Id == evnt.Id);
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingEventToVm>()).CreateMapper();
            mapper.Map(evnt, equipmentVm);
            var nodeVm = equipmentVm.Node;
            nodeVm.Type = evnt.Type;
        }

        public void RemoveEquipment(EquipmentRemoved evnt)
        {
            var equipmentVm = _model.Equipments.FirstOrDefault(e => e.Id == evnt.Id);
            if (equipmentVm == null)
            {
                var message = $@"EquipmentRemoved: Equipment {evnt.Id.First6()} not found";
                _logFile.AppendLine(message);
                return;
            }
            var nodeVm = equipmentVm.Node;

            _model.Equipments.Remove(equipmentVm);

            var majorEquipmentInNode = _model.Equipments.Where(e => e.Node.Id == nodeVm.Id).Max(e => e.Type);
            nodeVm.Type = majorEquipmentInNode;
        }

    }
}
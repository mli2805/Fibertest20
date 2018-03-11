using System.Linq;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EquipmentEventsOnGraphExecutor
    {
        readonly IMapper _mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingEventToVm>()).CreateMapper();
        private readonly GraphReadModel _model;
        private readonly ReadModel _readModel;

        public EquipmentEventsOnGraphExecutor(GraphReadModel model, ReadModel readModel)
        {
            _model = model;
            _readModel = readModel;
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
        }

        public void AddEquipmentIntoNode(EquipmentIntoNodeAdded evnt)
        {
            var nodeVm = _model.Data.Nodes.First(n => n.Id == evnt.NodeId);
            nodeVm.Type = evnt.Type;
        }

        public void UpdateEquipment(EquipmentUpdated evnt)
        {
            var equipment = _readModel.Equipments.First(e => e.Id == evnt.Id);
            var nodeVm = _model.Data.Nodes.First(n => n.Id == equipment.NodeId);
            nodeVm.Type = evnt.Type;
        }

        public void RemoveEquipment(EquipmentRemoved evnt)
        {
            var equipment = _readModel.Equipments.First(e => e.Id == evnt.Id);
            var nodeVm = _model.Data.Nodes.First(n => n.Id == equipment.NodeId);

            var majorEquipmentInNode = _readModel.Equipments.Where(e => e.NodeId == nodeVm.Id && e.Id != equipment.Id).Max(e => e.Type);
            nodeVm.Type = majorEquipmentInNode;
        }

    }
}
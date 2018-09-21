using System;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class EquipmentEventsOnGraphExecutor
    {
        private readonly GraphReadModel _model;
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        public EquipmentEventsOnGraphExecutor(GraphReadModel model, Model readModel, CurrentUser currentUser)
        {
            _model = model;
            _readModel = readModel;
            _currentUser = currentUser;
        }

        public void AddEquipmentAtGpsLocation(EquipmentAtGpsLocationAdded evnt)
        {
            //  if (_currentUser.ZoneId != Guid.Empty) return;
            if (_currentUser.Role > Role.Root) return;

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
            //if (_currentUser.ZoneId != Guid.Empty) return;
            if (_currentUser.Role > Role.Root) return;

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
            //            if (_currentUser.ZoneId != Guid.Empty
            //                && _model.Data.Nodes.All(f => f.Id != evnt.NodeId)) return;

            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm != null)
                nodeVm.Type = evnt.Type;
        }

        public void UpdateEquipment(EquipmentUpdated evnt)
        {
            var equipment = _readModel.Equipments.First(e => e.EquipmentId == evnt.EquipmentId);

            //            if (_currentUser.ZoneId != Guid.Empty
            //                && _model.Data.Nodes.All(f => f.Id != equipment.NodeId)) return;

            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == equipment.NodeId);
            if (nodeVm != null)
                nodeVm.Type = evnt.Type;
        }

        public void RemoveEquipment(EquipmentRemoved evnt)
        {
            if (_currentUser.ZoneId != Guid.Empty
                && _model.Data.Nodes.All(f => f.Id != evnt.NodeId)) return;

            var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == evnt.NodeId);
            if (nodeVm == null) return;
            var majorEquipmentInNode = _readModel.Equipments.Last(e => e.NodeId == nodeVm.Id).Type;
            nodeVm.Type = majorEquipmentInNode;
        }

    }
}
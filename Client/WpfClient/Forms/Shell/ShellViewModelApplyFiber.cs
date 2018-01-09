using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel
    {
        private AddFiberWithNodes EndFiberCreationMany(RequestAddFiberWithNodes request, int count, EquipmentType type)
        {
            var result = new AddFiberWithNodes()
            {
                Node1 = request.Node1, Node2 = request.Node2, AddNodes = new List<AddNode>(), AddEquipments = new List<AddEquipmentAtGpsLocation>()
            };
            List<Guid> nodeIds = new List<Guid>();

            foreach (var o in CreateMidNodes(request.Node1, request.Node2, count, type))
            {
                if (type == EquipmentType.Well || type == EquipmentType.Invisible)
                {
                    result.AddNodes.Add((AddNode)o);
                    nodeIds.Add(((AddNode)o).Id);
                }
                else
                {
                    result.AddEquipments.Add((AddEquipmentAtGpsLocation)o);
                    nodeIds.Add(((AddEquipmentAtGpsLocation)o).NodeId);
                }
            }

            nodeIds.Insert(0, request.Node1);
            nodeIds.Add(request.Node2);
            result.AddFibers = CreateMidFibers(nodeIds, count).ToList();
            return result;
        }

        private IEnumerable<object> CreateMidNodes(Guid startId, Guid finishId, int count, EquipmentType type)
        {
            var startNode = ReadModel.Nodes.First(n => n.Id == startId);
            var finishNode = ReadModel.Nodes.First(n => n.Id == finishId);

            double deltaLat = (finishNode.Latitude  - startNode.Latitude ) / (count + 1);
            double deltaLng = (finishNode.Longitude - startNode.Longitude) / (count + 1);

            for (int i = 0; i < count; i++)
            {
                double lat = startNode.Latitude  + deltaLat * (i + 1);
                double lng = startNode.Longitude + deltaLng * (i + 1);

                if (type == EquipmentType.Well || type == EquipmentType.Invisible)
                    yield return new AddNode() { Id = Guid.NewGuid(), Latitude = lat, Longitude = lng, IsJustForCurvature = type == EquipmentType.Invisible};
                else
                    yield return new AddEquipmentAtGpsLocation() { Id = Guid.NewGuid(), NodeId = Guid.NewGuid(), Latitude = lat, Longitude = lng, Type = type };
            }

        }

        private IEnumerable<AddFiber> CreateMidFibers(List<Guid> nodes, int count)
        {
            for (int i = 0; i <= count; i++)
                yield return new AddFiber() {Id = Guid.NewGuid(), Node1 = nodes[i], Node2 = nodes[i+1]};
        }

        private AddFiberWithNodes PrepareCommand(RequestAddFiberWithNodes request)
        {
            if (!Validate(request))
                return null;

            var vm = new FiberWithNodesAddViewModel();
            _windowManager.ShowDialogWithAssignedOwner(vm);
            if (!vm.Result)
                return null;

            return EndFiberCreationMany(request, vm.Count, vm.GetSelectedType());
        }

        private bool Validate(RequestAddFiberWithNodes request)
        {
            if (request.Node1 == request.Node2)
                return false;
            var fiber =
                GraphReadModel.Fibers.FirstOrDefault(
                    f =>
                        f.Node1.Id == request.Node1 && f.Node2.Id == request.Node2 ||
                        f.Node1.Id == request.Node2 && f.Node2.Id == request.Node1);
            if (fiber == null)
                return true;
            _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Section_already_exists));
            return false;
        }

        private AddFiber PrepareCommand(AddFiber request)
        {
            if (!Validate(request))
                return null;
            request.Id = Guid.NewGuid();
            return request;
        }

        private bool Validate(AddFiber cmd)
        {
            if (cmd.Node1 == cmd.Node2)
                return false;
            var fiber =
                GraphReadModel.Fibers.FirstOrDefault(
                    f =>
                        f.Node1.Id == cmd.Node1 && f.Node2.Id == cmd.Node2 ||
                        f.Node1.Id == cmd.Node2 && f.Node2.Id == cmd.Node1);
            if (fiber == null)
                return true;
            _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_Section_already_exists));
            return false;
        }

        private UpdateFiber PrepareCommand(RequestUpdateFiber request)
        {
            var vm = new FiberUpdateViewModel(request.Id, GraphReadModel);
            _windowManager.ShowDialogWithAssignedOwner(vm);

            return vm.Command;
        }

    }
}

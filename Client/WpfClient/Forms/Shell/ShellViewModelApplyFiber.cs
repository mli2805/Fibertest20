using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel
    {
        private AddFiberWithNodes EndFiberCreationMany(RequestAddFiberWithNodes request, int count, EquipmentType type)
        {
            var result = new AddFiberWithNodes()
            {
                Node1 = request.Node1,
                Node2 = request.Node2,
                AddEquipments = new List<AddEquipmentAtGpsLocation>()
            };
            List<Guid> nodeIds = new List<Guid>();

            var intermediateNodes = CreateIntermediateNodes(request.Node1, request.Node2, count, type);
            foreach (var cmd in intermediateNodes)
            {
                result.AddEquipments.Add(cmd);
                nodeIds.Add(cmd.NodeId);
            }

            nodeIds.Insert(0, request.Node1);
            nodeIds.Add(request.Node2);
            result.AddFibers = CreateIntermediateFibers(nodeIds, count).ToList();
            return result;
        }

        private IEnumerable<AddEquipmentAtGpsLocation> CreateIntermediateNodes(Guid startId, Guid finishId, int count, EquipmentType type)
        {
            var startNode = ReadModel.Nodes.First(n => n.Id == startId);
            var finishNode = ReadModel.Nodes.First(n => n.Id == finishId);

            double deltaLat = (finishNode.Latitude - startNode.Latitude) / (count + 1);
            double deltaLng = (finishNode.Longitude - startNode.Longitude) / (count + 1);

            for (int i = 0; i < count; i++)
            {
                double lat = startNode.Latitude + deltaLat * (i + 1);
                double lng = startNode.Longitude + deltaLng * (i + 1);

                var cmd = new AddEquipmentAtGpsLocation()
                {
                    RequestedEquipmentId = Guid.NewGuid(), NodeId = Guid.NewGuid(), Latitude = lat, Longitude = lng, Type = type
                };
                cmd.EmptyNodeEquipmentId = type <= EquipmentType.EmptyNode ? Guid.Empty : Guid.NewGuid();
                yield return cmd;
            }
        }

        private IEnumerable<AddFiber> CreateIntermediateFibers(List<Guid> nodes, int count)
        {
            for (int i = 0; i <= count; i++)
                yield return new AddFiber() { Id = Guid.NewGuid(), Node1 = nodes[i], Node2 = nodes[i + 1] };
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

        

       

    }
}

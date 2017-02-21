using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.TestBench.Properties;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddFiberWithNodes cmd)
        {
            if (cmd.AddNodes.Count > 0)
                foreach (var cmdAddNode in cmd.AddNodes)
                    ApplyToMap(cmdAddNode);
            else
                foreach (var cmdAddEquipment in cmd.AddEquipments)
                    ApplyToMap(cmdAddEquipment);

            foreach (var cmdAddFiber in cmd.AddFibers)
                ApplyToMap(cmdAddFiber);
        }

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
            _windowManager.ShowDialog(vm);
            if (!vm.Result)
                return null;

            return EndFiberCreationMany(request, vm.Count, vm.GetSelectedType());
        }

        private bool Validate(RequestAddFiberWithNodes request)
        {
            if (request.Node1 == request.Node2)
                return false;
            var fiber =
                GraphVm.Fibers.FirstOrDefault(
                    f =>
                        f.Node1.Id == request.Node1 && f.Node2.Id == request.Node2 ||
                        f.Node1.Id == request.Node2 && f.Node2.Id == request.Node1);
            if (fiber == null)
                return true;
            _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, Resources.SID_There_s_such_a_fiber_already));
            return false;
        }

        private void ApplyToMap(AddFiber cmd)
        {
            if (Validate(cmd))
                EndFiberCreationOne(cmd.Id, GraphVm.Nodes.Single(m => m.Id == cmd.Node1), GraphVm.Nodes.Single(m => m.Id == cmd.Node2));
        }

        private bool Validate(AddFiber cmd)
        {
            if (cmd.Node1 == cmd.Node2)
                return false;
            var fiber =
                GraphVm.Fibers.FirstOrDefault(
                    f =>
                        f.Node1.Id == cmd.Node1 && f.Node2.Id == cmd.Node2 ||
                        f.Node1.Id == cmd.Node2 && f.Node2.Id == cmd.Node1);
            if (fiber == null)
                return true;
            _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, Resources.SID_There_s_such_a_fiber_already));
            return false;
        }

        private UpdateFiber PrepareCommand(RequestUpdateFiber request)
        {
            var vm = new FiberUpdateViewModel(request.Id, GraphVm);
            _windowManager.ShowDialog(vm);

            return vm.Command;
        }
        private void ApplyToMap(UpdateFiber cmd)
        {

            GraphVm.Fibers.Single(e => e.Id == cmd.Id).UserInputedLength = cmd.UserInputedLength;
        }
        private void ApplyToMap(RemoveFiber cmd)
        {
            GraphVm.Fibers.Remove(GraphVm.Fibers.Single(f => f.Id == cmd.Id));
        }

        private void EndFiberCreationOne(Guid fiberId, NodeVm startMarker, NodeVm finishMarker)
        {
            GraphVm.Fibers.Add(new FiberVm() { Id = fiberId, Node1 = startMarker, Node2 = finishMarker, State = FiberState.NotInTrace });
        }

    }
}

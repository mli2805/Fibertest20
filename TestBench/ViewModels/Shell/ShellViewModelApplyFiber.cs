using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

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

        private AddFiberWithNodes EndFiberCreationMany(AskAddFiberWithNodes ask, int count, EquipmentType type)
        {
            var result = new AddFiberWithNodes()
            {
                Node1 = ask.Node1, Node2 = ask.Node2, AddNodes = new List<AddNode>(), AddEquipments = new List<AddEquipmentAtGpsLocation>()
            };
            List<Guid> nodeIds = new List<Guid>();

            foreach (var o in CreateMidNodes(ask.Node1, ask.Node2, count, type))
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

            nodeIds.Insert(0, ask.Node1);
            nodeIds.Add(ask.Node2);
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

        private AddFiberWithNodes PrepareCommand(AskAddFiberWithNodes ask)
        {
            if (!Validate(ask))
                return null;

            var vm = new FiberWithNodesAddViewModel();
            _windowManager.ShowDialog(vm);
            if (!vm.Result)
                return null;

            return EndFiberCreationMany(ask, vm.Count, vm.GetSelectedType());
        }

        private bool Validate(AskAddFiberWithNodes ask)
        {
            if (ask.Node1 == ask.Node2)
                return false;
            var fiber =
                GraphVm.Fibers.FirstOrDefault(
                    f =>
                        f.NodeA.Id == ask.Node1 && f.NodeB.Id == ask.Node2 ||
                        f.NodeA.Id == ask.Node2 && f.NodeB.Id == ask.Node1);
            if (fiber == null)
                return true;
            _windowManager.ShowDialog(new NotificationViewModel("", "Уже есть такое волокно"));
            return false;
        }

        private void ApplyToMap(AddFiber cmd)
        {
            if (Validate(cmd))
                EndFiberCreationOne(GraphVm.Nodes.Single(m => m.Id == cmd.Node1), GraphVm.Nodes.Single(m => m.Id == cmd.Node2));
        }

        private bool Validate(AddFiber cmd)
        {
            if (cmd.Node1 == cmd.Node2)
                return false;
            var fiber =
                GraphVm.Fibers.FirstOrDefault(
                    f =>
                        f.NodeA.Id == cmd.Node1 && f.NodeB.Id == cmd.Node2 ||
                        f.NodeA.Id == cmd.Node2 && f.NodeB.Id == cmd.Node1);
            if (fiber == null)
                return true;
            MessageBox.Show("Уже есть такое волокно");
            return false;
        }

        private void ApplyToMap(UpdateFiber cmd)
        {
            var vm = new FiberUpdateViewModel(cmd.Id, GraphVm);
            new WindowManager().ShowDialog(vm);

            GraphVm.Fibers.Single(e => e.Id == cmd.Id).UserInputedLength = cmd.UserInputedLength;
        }
        private void ApplyToMap(RemoveFiber cmd)
        {
            GraphVm.Fibers.Remove(GraphVm.Fibers.Single(f => f.Id == cmd.Id));
        }

        private void EndFiberCreationOne(NodeVm startMarker, NodeVm finishMarker)
        {
            GraphVm.Fibers.Add(new FiberVm() { Id = Guid.NewGuid(), NodeA = startMarker, NodeB = finishMarker, State = FiberState.NotInTrace });
        }

    }
}

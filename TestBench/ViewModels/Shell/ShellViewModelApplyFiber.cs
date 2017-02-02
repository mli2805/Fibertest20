using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddFiberWithNodes cmd)
        {
            if (!Validate(cmd))
                return;

            var vm = new AddFiberWithNodesViewModel();
            new WindowManager().ShowDialog(vm);
            if (!vm.Result)
                return;

            cmd.IntermediateNodesCount = vm.Count;
            cmd.EquipmentInIntermediateNodesType = vm.GetSelectedType();

            EndFiberCreationMany(GraphVm.Nodes.Single(m => m.Id == cmd.Node1),
                GraphVm.Nodes.First(m => m.Id == cmd.Node2),
                cmd.IntermediateNodesCount, cmd.EquipmentInIntermediateNodesType);
        }

        private bool Validate(AddFiberWithNodes cmd)
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

        private void EndFiberCreationMany(NodeVm startMarker, NodeVm finishMarker, int fiberCreateWithNodesCount, EquipmentType intermediateNodeType)
        {
            var nodes = CreateMidNodes(startMarker, finishMarker, fiberCreateWithNodesCount, intermediateNodeType);
            nodes.Insert(0, startMarker);
            nodes.Add(finishMarker);
            CreateMidFibers(nodes, fiberCreateWithNodesCount);
        }

        private void EndFiberCreationOne(NodeVm startMarker, NodeVm finishMarker)
        {
            GraphVm.Fibers.Add(new FiberVm() { Id = Guid.NewGuid(), NodeA = startMarker, NodeB = finishMarker, State = FiberState.NotInTrace });
        }

        private List<NodeVm> CreateMidNodes(NodeVm startMarker,
            NodeVm finishMarker, int fiberCreateWithNodesCount, EquipmentType type)
        {
            List<NodeVm> midNodes = new List<NodeVm>();
            double deltaLat = (finishMarker.Position.Lat - startMarker.Position.Lat) / (fiberCreateWithNodesCount + 1);
            double deltaLng = (finishMarker.Position.Lng - startMarker.Position.Lng) / (fiberCreateWithNodesCount + 1);

            for (int i = 0; i < fiberCreateWithNodesCount; i++)
            {
                double lat = startMarker.Position.Lat + deltaLat * (i + 1);
                double lng = startMarker.Position.Lng + deltaLng * (i + 1);
                var nodeVm = new NodeVm() { Id = Guid.NewGuid(), Position = new PointLatLng(lat, lng), Type = type };
                midNodes.Add(nodeVm);
                GraphVm.Nodes.Add(nodeVm);
            }
            return midNodes;
        }

        private void CreateMidFibers(List<NodeVm> nodes, int fiberCreateWithNodesCount)
        {
            for (int i = 0; i <= fiberCreateWithNodesCount; i++)
            {
                FiberVm fiberVm = new FiberVm() { Id = Guid.NewGuid(), NodeA = nodes[i], NodeB = nodes[i + 1], State = FiberState.NotInTrace };
                GraphVm.Fibers.Add(fiberVm);
            }
        }

    }
}

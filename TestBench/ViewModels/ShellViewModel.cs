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
    public class ShellViewModel : Screen, IShell
    {
        public GraphVm GraphVm { get; set; } = new GraphVm();

        public void AddOneNode()
        {
            var rtu = new NodeVm() { Id = Guid.NewGuid(), Title = "РТУ в Гомеле", State = FiberState.Ok, Type = EquipmentType.Rtu, Position = new PointLatLng(52.429333, 31.006683) };
            GraphVm.Nodes.Add(rtu);
        }

        public void Populate()
        {
            var rtu = new NodeVm() { Id = Guid.NewGuid(), Title = "first rtu", State = FiberState.Ok, Type = EquipmentType.Rtu, Position = new PointLatLng(53.288345, 30.019362) };
            GraphVm.Nodes.Add(rtu);
            var vertSleeve = new NodeVm() { Id = Guid.NewGuid(), Title = "vert sleeve", State = FiberState.Critical, Type = EquipmentType.Sleeve, Position = new PointLatLng(52.301848, 30.018362) };
            GraphVm.Nodes.Add(vertSleeve);
            var horizSleeve = new NodeVm() { Id = Guid.NewGuid(), Title = "horiz sleeve", State = FiberState.Ok, Type = EquipmentType.Sleeve, Position = new PointLatLng(53.287345, 31.016841) };
            GraphVm.Nodes.Add(horizSleeve);

            GraphVm.Fibers.Add(new FiberVm() { Id = Guid.NewGuid(), NodeA = rtu, NodeB = vertSleeve, State = FiberState.Critical });
            GraphVm.Fibers.Add(new FiberVm() { Id = Guid.NewGuid(), NodeA = rtu, NodeB = horizSleeve, State = FiberState.Ok });
        }

        protected override void OnViewLoaded(object view)
        {
            base.OnViewLoaded(view);
            GraphVm.PropertyChanged += GraphVm_PropertyChanged;
        }

        private void GraphVm_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "Command")
                return;

            #region Node
            if (GraphVm.Command is AddNode)
                ApplyToMap((AddNode)GraphVm.Command);
            if (GraphVm.Command is MoveNode)
                ApplyToMap((MoveNode)GraphVm.Command);
            if (GraphVm.Command is RemoveNode)
                ApplyToMap((RemoveNode)GraphVm.Command);
            #endregion

            #region Fiber
            if (GraphVm.Command is AddFiberWithNodes)
                ApplyToMap((AddFiberWithNodes)GraphVm.Command);
            if (GraphVm.Command is AddFiber)
                ApplyToMap((AddFiber)GraphVm.Command);
            if (GraphVm.Command is UpdateFiber)
                ApplyToMap((UpdateFiber)GraphVm.Command);
            if (GraphVm.Command is RemoveFiber)
                ApplyToMap((RemoveFiber)GraphVm.Command);
            #endregion

            if (GraphVm.Command is AddRtuAtGpsLocation)
                ApplyToMap((AddRtuAtGpsLocation)GraphVm.Command);
            if (GraphVm.Command is AddEquipmentAtGpsLocation)
                ApplyToMap((AddEquipmentAtGpsLocation)GraphVm.Command);

            //TODO Send Command to Aggregate
        }

        #region Node
        private void ApplyToMap(AddNode cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = Guid.NewGuid(),
                State = FiberState.Ok,
                Type = cmd.IsJustForCurvature ? EquipmentType.Invisible : EquipmentType.Well,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphVm.Nodes.Add(nodeVm);
        }

        private void ApplyToMap(MoveNode cmd)
        {
            var nodeVm = GraphVm.Nodes.Single(n => n.Id == cmd.Id);
            nodeVm.Position = new PointLatLng(cmd.Latitude, cmd.Longitude);
        }

        private void ApplyToMap(RemoveNode cmd)
        {
            var fiberVms = GraphVm.Fibers.Where(e => e.NodeA.Id == cmd.Id || e.NodeB.Id == cmd.Id).ToList();
            foreach (var fiberVm in fiberVms)
            {
                GraphVm.Fibers.Remove(fiberVm);
            }
            GraphVm.Nodes.Remove(GraphVm.Nodes.Single(n => n.Id == cmd.Id));
        }
        #endregion

        #region Fiber
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
        #endregion

        private void ApplyToMap(AddRtuAtGpsLocation cmd)
        {
            var nodeVm = new NodeVm() { Id = Guid.NewGuid(), State = FiberState.Ok, Type = EquipmentType.Rtu, Position = new PointLatLng(cmd.Latitude, cmd.Longitude) };
            GraphVm.Nodes.Add(nodeVm);
        }

        private void ApplyToMap(AddEquipmentAtGpsLocation cmd)
        {
            var nodeVm = new NodeVm() { Id = Guid.NewGuid(), State = FiberState.Ok, Type = cmd.Type, Position = new PointLatLng(cmd.Latitude, cmd.Longitude) };
            GraphVm.Nodes.Add(nodeVm);
        }
    }
}
using System;
using Caliburn.Micro;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel : Screen, IShell
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
            if (GraphVm.Command is AddNodeIntoFiber)
            {
                // validation if fiber used by trace with base 
                // was applied inside context menu

                // here should be many requests to user - 
                // type of node visible or not, add equipment or not, 
                // which traces if there any should use equipment if it was added
                // after this we have fully filled in AddFiberWithNodes command

                // next we should send this command to Aggregate and
                // if Aggregate executed it new events will appeare
                // and ReadModel will apply them

                // 
                ApplyToMap((AddNodeIntoFiber)GraphVm.Command);
            }
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
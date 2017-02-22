using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddRtuAtGpsLocation cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.NodeId,
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphReadModel.Nodes.Add(nodeVm);

            var rtuVm = new RtuVm() { Id = cmd.Id, Node = nodeVm};
            GraphReadModel.Rtus.Add(rtuVm);
        }

        private UpdateRtu PrepareCommand(RequestUpdateRtu request)
        {
            var vm = new RtuUpdateViewModel(request.NodeId, GraphReadModel);
            _windowManager.ShowDialog(vm);
            return vm.Command;
        }

        private void ApplyToMap(UpdateRtu cmd)
        {
            var rtu = GraphReadModel.Rtus.First(r => r.Id == cmd.Id);
            rtu.Node.Title = rtu.Title;
        }

        private void ApplyToMap(RemoveRtu cmd)
        {
            var rtuVm = GraphReadModel.Rtus.First(r => r.Id == cmd.Id);
            var nodeVm = rtuVm.Node;
            GraphReadModel.Rtus.Remove(rtuVm);
            GraphReadModel.Nodes.Remove(nodeVm);
        }

    }
}
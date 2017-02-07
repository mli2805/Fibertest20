using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddEquipmentAtGpsLocation cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.NodeId,
                State = FiberState.Ok,
                Type = cmd.Type,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphVm.Nodes.Add(nodeVm);

            GraphVm.Equipments.Add(new EquipmentVm() { Id = cmd.Id, Node = nodeVm, Type = cmd.Type });
        }
    }
}

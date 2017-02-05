using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddNode cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.Id,
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
    }

}

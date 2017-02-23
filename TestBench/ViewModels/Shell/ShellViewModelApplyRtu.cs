using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
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
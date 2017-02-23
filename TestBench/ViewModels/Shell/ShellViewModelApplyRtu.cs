using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private UpdateRtu PrepareCommand(RequestUpdateRtu request)
        {
            var vm = new RtuUpdateViewModel(request.NodeId, ReadModel);
            _windowManager.ShowDialog(vm);
            return vm.Command;
        }

        private RemoveRtu PrepareCommand(RequestRemoveRtu request)
        {
            var rtu = GraphReadModel.Rtus.First(r => r.Node.Id == request.NodeId);
            if (GraphReadModel.Traces.Any(t => t.RtuId == rtu.Id && t.Port > 0))
                return null; // It's prohibited to remove rtu where there are traces joined

            return new RemoveRtu() { Id = rtu.Id };

        }

    }
}
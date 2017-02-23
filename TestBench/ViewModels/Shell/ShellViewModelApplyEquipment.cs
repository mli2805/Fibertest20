using System;
using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private AddEquipmentIntoNode PrepareCommand(RequestAddEquipmentIntoNode request)
        {
            var tracesInNode = ReadModel.Traces.Where(t => t.Nodes.Contains(request.NodeId)).ToList();
            TraceChoiceViewModel traceChoiceVm = null;
            if (tracesInNode.Count > 0)
            {
                traceChoiceVm = new TraceChoiceViewModel(tracesInNode);
                _windowManager.ShowDialog(traceChoiceVm);
                if (!traceChoiceVm.ShouldWeContinue)
                    return null;
            }
            var vm = new EquipmentUpdateViewModel(request.NodeId, Guid.Empty);
            _windowManager.ShowDialog(vm);
            if (vm.Command == null)
                return null;
            var command = (AddEquipmentIntoNode) vm.Command;
            if (traceChoiceVm != null)
                command.TracesForInsertion = traceChoiceVm.GetChosenTraces();
            return command;
        }

        private UpdateEquipment PrepareCommand(UpdateEquipment request)
        {
            var cmd = request;
            return cmd;
        }

        private RemoveEquipment PrepareCommand(RemoveEquipment request)
        {
            var cmd = new RemoveEquipment() {Id = request.Id};
            return cmd;
        }

    }
}

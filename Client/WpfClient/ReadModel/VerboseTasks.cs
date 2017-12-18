using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public static class VerboseTasks
    {
        //user asks equipment addition on the map
        public static async Task AddEquipmentIntoNodeFullTask(RequestAddEquipmentIntoNode request, 
            ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            var cmd = BuildAddEquipmentIntoNodeCommand(request.NodeId, readModel, windowManager);
            if (cmd == null)
                return;
            await c2DWcfManager.SendCommandAsObj(cmd);
        }

        // user asks equipment addition from node update view
        public static AddEquipmentIntoNode BuildAddEquipmentIntoNodeCommand(Guid nodeId, 
            ReadModel readModel, IWindowManager windowManager)
        {
            var tracesInNode = readModel.Traces.Where(t => t.Nodes.Contains(nodeId)).ToList();
            TraceChoiceViewModel traceChoiceVm = null;
            if (tracesInNode.Count > 0)
            {
                traceChoiceVm = new TraceChoiceViewModel(tracesInNode);
                windowManager.ShowDialogWithAssignedOwner(traceChoiceVm);
                if (!traceChoiceVm.ShouldWeContinue)
                    return null;
            }
            var vm = new EquipmentInfoViewModel(nodeId);
            windowManager.ShowDialogWithAssignedOwner(vm);
            if (vm.Command == null)
                return null;
            var command = (AddEquipmentIntoNode)vm.Command;
            if (traceChoiceVm != null)
                command.TracesForInsertion = traceChoiceVm.GetChosenTraces();
            return command;
        }
    }
}

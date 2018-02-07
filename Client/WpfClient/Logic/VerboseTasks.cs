using System;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public static class VerboseTasks
    {
        //user asks equipment addition on the map
        public static async Task AddEquipmentIntoNodeFullTask(RequestAddEquipmentIntoNode request, ILifetimeScope globalScope,
            ReadModel readModel, IWindowManager windowManager, IWcfServiceForClient c2DWcfManager)
        {
            var cmd = BuildAddEquipmentIntoNodeCommand(request.NodeId, request.IsCableReserveRequested, readModel, windowManager, globalScope);
            if (cmd == null)
                return;
            await c2DWcfManager.SendCommandAsObj(cmd);
        }

        // user asks equipment addition from node update view
        public static AddEquipmentIntoNode BuildAddEquipmentIntoNodeCommand(Guid nodeId, bool isCableReserveRequested,
            ReadModel readModel, IWindowManager windowManager, ILifetimeScope globalScope)
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

            AddEquipmentIntoNode command;
            if (isCableReserveRequested)
            {
                var vm = new CableReserveInfoViewModel(nodeId);
                windowManager.ShowDialogWithAssignedOwner(vm);
                if (vm.Command == null)
                    return null;
                command = (AddEquipmentIntoNode)vm.Command;
            }
            else
            {
                var vm = globalScope.Resolve<EquipmentInfoViewModel>();
                vm.InitializeForAdd(nodeId);
                windowManager.ShowDialogWithAssignedOwner(vm);
                if (vm.Command == null)
                    return null;
                command = (AddEquipmentIntoNode)vm.Command;
            }
          

            if (traceChoiceVm != null)
                command.TracesForInsertion = traceChoiceVm.GetChosenTraces();
            return command;
        }
    }
}

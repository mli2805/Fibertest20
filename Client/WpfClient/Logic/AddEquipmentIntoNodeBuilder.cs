using System;
using System.Linq;
using Autofac;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class AddEquipmentIntoNodeBuilder
    {
        private readonly ILifetimeScope _globalScope;
        private readonly ReadModel _readModel;
        private readonly IWindowManager _windowManager;

        public AddEquipmentIntoNodeBuilder(ILifetimeScope globalScope, ReadModel readModel, IWindowManager windowManager)
        {
            _globalScope = globalScope;
            _readModel = readModel;
            _windowManager = windowManager;
        }

        public AddEquipmentIntoNode BuildCommand(Guid nodeId)
        {
            var tracesInNode = _readModel.Traces.Where(t => t.Nodes.Contains(nodeId)).ToList();
            TracesToEquipmentInjectionViewModel tracesToEquipmentInjectionVm = null;
            if (tracesInNode.Count > 0)
            {
                tracesToEquipmentInjectionVm = new TracesToEquipmentInjectionViewModel(tracesInNode);
                _windowManager.ShowDialogWithAssignedOwner(tracesToEquipmentInjectionVm);
                if (!tracesToEquipmentInjectionVm.ShouldWeContinue)
                    return null;
            }

            var vm = _globalScope.Resolve<EquipmentInfoViewModel>();
            vm.InitializeForAdd(nodeId);
            _windowManager.ShowDialogWithAssignedOwner(vm);
            if (vm.Command == null)
                return null;
            var command = (AddEquipmentIntoNode)vm.Command;


            if (tracesToEquipmentInjectionVm != null)
                command.TracesForInsertion = tracesToEquipmentInjectionVm.GetChosenTraces();
            return command;
        }
    }


}

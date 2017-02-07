using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private AddTrace PrepareCommand(AskAddTrace ask)
        {
            List<Guid> traceNodes;
            List<Guid> traceEquipments;
            if (!ReadModel.DefineTrace(_windowManager, ask.NodeWithRtuId, ask.LastNodeId,
                out traceNodes, out traceEquipments))
                return null;
            var traceAddViewModel = new TraceAddViewModel();
            _windowManager.ShowDialog(traceAddViewModel);

            if (!traceAddViewModel.IsUserClickedSave)
                return null;

            return new AddTrace()
            {
                Id = Guid.NewGuid(),
                RtuId = ReadModel.Rtus.First(r => r.NodeId == ask.NodeWithRtuId).Id,
                Title = traceAddViewModel.Title,
                Nodes = traceNodes,
                Equipments = traceEquipments,
                Comment = traceAddViewModel.Comment
            };
        }

        private void ApplyToMap(AddTrace cmd)
        {
            GraphVm.Traces.Add(new TraceVm() { Id = cmd.Id, Nodes = cmd.Nodes });
        }

        private AssignBaseRef PrepareCommand(AskAssignBaseRef request)
        {
            var vm = new BaseRefsAssignViewModel(request.TraceId, ReadModel);
            _windowManager.ShowDialog(vm);
            return vm.Command;
        }

        private void ApplyToMap(AssignBaseRef cmd)
        {
            var traceVm = GraphVm.Traces.First(t => t.Id == cmd.TraceId);
            traceVm.PreciseId = cmd.PreciseId;
            traceVm.FastId = cmd.FastId;
            traceVm.AdditionalId = cmd.AdditionalId;
        }

    }

}

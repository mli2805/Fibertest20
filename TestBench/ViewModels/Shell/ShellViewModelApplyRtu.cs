﻿using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private UpdateRtu PrepareCommand(RequestUpdateRtu request)
        {
            var rtu = ReadModel.Rtus.First(r => r.NodeId == request.NodeId);
            var vm = new RtuUpdateViewModel(rtu.Id, ReadModel);
            _windowManager.ShowDialog(vm);
            return vm.Command;
        }

        private RemoveRtu PrepareCommand(RequestRemoveRtu request)
        {
            var rtu = GraphReadModel.Rtus.First(r => r.Node.Id == request.NodeId);
            return new RemoveRtu() { Id = rtu.Id };
        }

    }
}
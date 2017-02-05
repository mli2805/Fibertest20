﻿using System.Linq;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddRtuAtGpsLocation cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.NodeId,
                State = FiberState.Ok,
                Type = EquipmentType.Rtu,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphVm.Nodes.Add(nodeVm);

            var rtuVm = new RtuVm() { Id = cmd.Id, Node = nodeVm};
            GraphVm.Rtus.Add(rtuVm);
        }

        private void ApplyToMap(UpdateRtu cmd)
        {
            var rtu = GraphVm.Rtus.First(r => r.Id == cmd.Id);
            rtu.Node.Position = new PointLatLng(cmd.Latitude, cmd.Longitude);
        }

        private UpdateRtu PrepareCommand(AskUpdateRtu ask)
        {
            var vm = new RtuUpdateViewModel(ask.NodeId, GraphVm);
            _windowManager.ShowDialog(vm);
            return vm.Command;
        }
    }
}
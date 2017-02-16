using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GMap.NET;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private void ApplyToMap(AddEquipmentAtGpsLocation cmd)
        {
            var nodeVm = new NodeVm()
            {
                Id = cmd.NodeId,
                State = FiberState.Ok,
                Type = cmd.Type,
                Position = new PointLatLng(cmd.Latitude, cmd.Longitude)
            };
            GraphVm.Nodes.Add(nodeVm);

            GraphVm.Equipments.Add(new EquipmentVm() { Id = cmd.Id, Node = nodeVm, Type = cmd.Type });
        }

        private AddEquipment PrepareCommand(RequestAddEquipmentIntoNode request)
        {
            var vm = new EquipmentUpdateViewModel(request.NodeId, Guid.Empty, new List<Guid>());
            _windowManager.ShowDialog(vm);
            return (AddEquipment) vm.Command;
        }

        private void ApplyToMap(AddEquipment cmd)
        {
            IMapper mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappingCommandToVm>()).CreateMapper();
            var equipmentVm = mapper.Map<EquipmentVm>(cmd);
            equipmentVm.Node = GraphVm.Nodes.First(n => n.Id == cmd.NodeId); //TODO maybe it should be done in mapper configuration class

            GraphVm.Equipments.Add(equipmentVm);
            // TODO change node pictogram
        }

        private void ApplyToMap(RemoveEquipment cmd)
        {
            GraphVm.Equipments.Remove(GraphVm.Equipments.First(e => e.Id == cmd.Id));
        }
    }
}

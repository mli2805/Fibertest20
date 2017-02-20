using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;

namespace Iit.Fibertest.TestBench
{
    public static class MapTraceDefine
    {

        public static List<Guid> CollectEquipmentForTrace(IWindowManager windowManager, List<Guid> nodes, ReadModel readModel)
        {
            var equipments = new List<Guid> { readModel.Rtus.Single(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var possibleEquipments = readModel.Equipments.Where(e => e.NodeId == nodeId).ToList();
                if (possibleEquipments.Count == 0)
                    equipments.Add(Guid.Empty);
                else
                {
                    var equipmentChoiceViewModel = new EquipmentChoiceViewModel(possibleEquipments, nodeId == nodes.Last());
                    windowManager.ShowDialog(equipmentChoiceViewModel);
                    if (!equipmentChoiceViewModel.ShouldWeContinue) // пользователь прервал процесс, отказавшись выбирать оборудование
                        return null;

                    equipments.Add(equipmentChoiceViewModel.GetSelectedEquipmentGuid());
                }
            }
            return equipments;
        }
    }
}

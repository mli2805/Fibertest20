using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public static class MapTraceDefine
    {
        public static bool DefineTrace(this ReadModel readModel, IWindowManager windowManager, Guid rtuNodeId, Guid lastNodeId, out List<Guid> nodes, out List<Guid> equipments)
        {
            nodes = new List<Guid>();
            equipments = new List<Guid>();
            if (readModel.Equipments.All(e => e.NodeId != lastNodeId))
            {
//                var errorNotificationViewModel =
//                    new NotificationViewModel("Ошибка!", "Last node of trace must contain some equipment");
//                windowManager.ShowDialog(errorNotificationViewModel);
                return false;
            }

            var path = new PathFinder(readModel).FindPath(rtuNodeId, lastNodeId);
            if (path == null)
            {
//                var errorNotificationViewModel =
//                    new NotificationViewModel("Ошибка!", "Path couldn't be found");
//                windowManager.ShowDialog(errorNotificationViewModel);
                return false;
            }

//            var questionViewModel = new QuestionViewModel("Accept the path?");
//            windowManager.ShowDialog(questionViewModel);
//            if (!questionViewModel.IsAnswerPositive)
//                return false;

            nodes = path.ToList();
            equipments = CollectEquipmentForTrace(windowManager, nodes, readModel);
            if (equipments == null) // пользователь прервал процесс, отказавшись выбирать оборудование
                return false;
            return true;

        }

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

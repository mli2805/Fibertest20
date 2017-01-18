using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public static class MapTraceDefineProcess
    {
        public static void DefineTrace(ReadModel readModel, Aggregate aggregate, IWindowManager windowManager, Guid rtuNodeId, Guid lastNodeId)
        {
            if (!readModel.Equipments.Any(e => e.NodeId == lastNodeId))
            {
                var errorNotificationViewModel =
                    new ErrorNotificationViewModel("Last node of trace must contain some equipment");
                windowManager.ShowDialog(errorNotificationViewModel);
                return;
            }

            var path = new PathFinder(readModel).FindPath(rtuNodeId, lastNodeId);
            if (path == null)
            {
                var errorNotificationViewModel =
                    new ErrorNotificationViewModel("Path couldn't be found");
                windowManager.ShowDialog(errorNotificationViewModel);
                return;
            }

            var questionViewModel = new QuestionViewModel("Accept the path?");
            windowManager.ShowDialog(questionViewModel);
            if (!questionViewModel.IsAnswerPositive)
                return;

            var nodes = path.ToList();
            var equipments = CollectEquipmentForTrace(readModel, windowManager, nodes);
            if (equipments == null) // пользователь прервал процесс, отказавшись выбирать оборудование
                return;

//            mapViewModel.AddTraceViewModel = new AddTraceViewModel(windowManager, readModel, aggregate, nodes, equipments);
            //          windowManager.ShowDialog(mapViewModel.AddTraceViewModel);

            var addTraceViewModel = new AddTraceViewModel(windowManager, readModel, aggregate, nodes, equipments);
            windowManager.ShowDialog(addTraceViewModel);
        }

        public static List<Guid> CollectEquipmentForTrace(ReadModel readModel, IWindowManager windowManager, List<Guid> nodes)
        {
            var equipments = new List<Guid> { readModel.Rtus.Single(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var possibleEquipments = readModel.Equipments.Where(e => e.NodeId == nodeId).ToList();
                if (possibleEquipments.Count == 0)
                    equipments.Add(Guid.Empty);
                else
                {
                    var askEquipmentUsageViewModel = new AskEquipmentUsageInTraceViewModel(possibleEquipments, nodeId == nodes.Last());
                    windowManager.ShowDialog(askEquipmentUsageViewModel);
                    if (!askEquipmentUsageViewModel.ShouldWeContinue) // пользователь прервал процесс, отказавшись выбирать оборудование
                        return null;

                    equipments.Add(askEquipmentUsageViewModel.GetSelectedGuid());
                }
            }
            return equipments;
        }
    }
}
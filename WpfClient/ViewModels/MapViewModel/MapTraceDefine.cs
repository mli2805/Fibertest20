using System;
using System.Collections.Generic;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.WpfClient.ViewModels
{
    public static class MapTraceDefine
    {
        public static void DefineTrace(this MapViewModel mapViewModel, IWindowManager windowManager, Guid rtuNodeId, Guid lastNodeId)
        {
            var path = new PathFinder(mapViewModel.ReadModel).FindPath(rtuNodeId, lastNodeId);
            if (path == null)
            {
                var errorNotificationViewModel =
                    new ErrorNotificationViewModel("Path couldn't be found");
                windowManager.ShowDialog(errorNotificationViewModel);
            }
            else
            {
                var nodes = path.ToList();
                var equipments = mapViewModel.CollectEquipmentForTrace(nodes);
                mapViewModel.AddTraceViewModel = new AddTraceViewModel(windowManager, mapViewModel.ReadModel, mapViewModel.Aggregate, nodes, equipments);
                windowManager.ShowDialog(mapViewModel.AddTraceViewModel);
            }
        }

        //TODO: требуется реальное наполнение c запросами пользователю и проверкой, что последний узел содержит оборудование
        public static List<Guid> CollectEquipmentForTrace(this MapViewModel mapViewModel, List<Guid> nodes)
        {
            var equipments = new List<Guid> { mapViewModel.ReadModel.Rtus.Single(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var equipment = mapViewModel.ReadModel.Equipments.FirstOrDefault(e => e.NodeId == nodeId);
                equipments.Add(equipment?.Id ?? Guid.Empty);
            }
            return equipments;
        }
    }
}

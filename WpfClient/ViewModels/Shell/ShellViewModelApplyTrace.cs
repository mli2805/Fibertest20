using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public partial class ShellViewModel
    {
        private void PrepareCommand(RequestAddTrace request)
        {
            if (!Validate(request))
                return;

            List<Guid> traceNodes = GetPath(request);
            if (traceNodes == null)
                return;

            var traceId = Guid.NewGuid();
            ChangeTraceColor(traceId, traceNodes, FiberState.HighLighted);

            var questionViewModel = new QuestionViewModel(Resources.SID_Accept_the_path);
            _windowManager.ShowDialog(questionViewModel);

            ChangeTraceColor(traceId, traceNodes, FiberState.NotInTrace);

            if (!questionViewModel.IsAnswerPositive)
                return;

            List<Guid> traceEquipments = CollectEquipment(traceNodes);
            if (traceEquipments == null)
                return;

            var traceAddViewModel = new TraceInfoViewModel(ReadModel, Bus, _windowManager, Guid.Empty, traceEquipments, traceNodes);
            _windowManager.ShowDialog(traceAddViewModel);
        }

        private bool Validate(RequestAddTrace request)
        {
            if (ReadModel.Equipments.Any(e => e.NodeId == request.LastNodeId)) return true;

            _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, Resources.SID_Last_node_of_trace_must_contain_some_equipment));
            return false;
        }

        private List<Guid> GetPath(RequestAddTrace request)
        {
            List<Guid> path;
            if (!new PathFinder(ReadModel).FindPath(request.NodeWithRtuId, request.LastNodeId, out path))
                _windowManager.ShowDialog(new NotificationViewModel(Resources.SID_Error, Resources.SID_Path_couldn_t_be_found));

            return path;
        }

        private void ChangeTraceColor(Guid traceId, List<Guid> nodes, FiberState state)
        {
            var fibers = ReadModel.GetFibersByNodes(nodes);
            foreach (var fiber in fibers)
            {
                var fiberVm = GraphReadModel.Fibers.First(f => f.Id == fiber);
                if (state != FiberState.NotInTrace)
                    fiberVm.SetState(traceId, state);
                else
                    fiberVm.RemoveState(traceId);
            }
        }

        public List<Guid> CollectEquipment(List<Guid> nodes)
        {
            var equipments = new List<Guid> { ReadModel.Rtus.First(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var possibleEquipments = ReadModel.Equipments.Where(e => e.NodeId == nodeId).ToList();
                if (possibleEquipments.Count == 0)
                    equipments.Add(Guid.Empty);
                else
                {
                    var equipmentChoiceViewModel = new EquipmentChoiceViewModel(_windowManager, Bus, 
                        possibleEquipments, ReadModel.Nodes.First(n=>n.Id==nodeId).Title, nodeId == nodes.Last());
                    _windowManager.ShowDialog(equipmentChoiceViewModel);
                    if (!equipmentChoiceViewModel.ShouldWeContinue) // пользователь прервал процесс, отказавшись выбирать оборудование
                        return null;

                    equipments.Add(equipmentChoiceViewModel.GetSelectedEquipmentGuid());
                }
            }
            return equipments;
        }

    }

}

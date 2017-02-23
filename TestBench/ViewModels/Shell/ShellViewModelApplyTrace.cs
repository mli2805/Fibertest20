using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.TestBench
{
    public partial class ShellViewModel
    {
        private AddTrace PrepareCommand(RequestAddTrace request)
        {
            if (!Validate(request))
                return null;

            List<Guid> traceNodes = GetPath(request);
            if (traceNodes == null)
                return null;

            var traceId = Guid.NewGuid();
            ChangeTraceColor(traceId, traceNodes, FiberState.HighLighted);

            var questionViewModel = new QuestionViewModel(Resources.SID_Accept_the_path);
            _windowManager.ShowDialog(questionViewModel);

            ChangeTraceColor(traceId, traceNodes, FiberState.NotInTrace);

            if (!questionViewModel.IsAnswerPositive)
                return null;

            List<Guid> traceEquipments = CollectEquipment(traceNodes);
            if (traceEquipments == null)
                return null;

            var traceAddViewModel = new TraceAddViewModel();
            _windowManager.ShowDialog(traceAddViewModel);

            if (!traceAddViewModel.IsUserClickedSave)
                return null;

            return new AddTrace()
            {
                Id = traceId,
                RtuId = ReadModel.Rtus.First(r => r.NodeId == request.NodeWithRtuId).Id,
                Title = traceAddViewModel.Title,
                Nodes = traceNodes,
                Equipments = traceEquipments,
                Comment = traceAddViewModel.Comment
            };
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

        private List<Guid> CollectEquipment(List<Guid> nodes)
        {
            var equipments = new List<Guid> { ReadModel.Rtus.Single(r => r.NodeId == nodes[0]).Id };
            foreach (var nodeId in nodes.Skip(1))
            {
                var possibleEquipments = ReadModel.Equipments.Where(e => e.NodeId == nodeId).ToList();
                if (possibleEquipments.Count == 0)
                    equipments.Add(Guid.Empty);
                else
                {
                    var equipmentChoiceViewModel = new EquipmentChoiceViewModel(possibleEquipments, nodeId == nodes.Last());
                    _windowManager.ShowDialog(equipmentChoiceViewModel);
                    if (!equipmentChoiceViewModel.ShouldWeContinue) // пользователь прервал процесс, отказавшись выбирать оборудование
                        return null;

                    equipments.Add(equipmentChoiceViewModel.GetSelectedEquipmentGuid());
                }
            }
            return equipments;
        }

        private AssignBaseRef PrepareCommand(RequestAssignBaseRef request)
        {
            var trace = ReadModel.Traces.First(t => t.Id == request.TraceId);
            var vm = new BaseRefsAssignViewModel(trace, GraphReadModel.Rtus.First(r=>r.Id == trace.RtuId).Title);
            _windowManager.ShowDialog(vm);
            return vm.Command;
        }

        private void ApplyToMap(AssignBaseRef cmd)
        {
            var traceVm = GraphReadModel.Traces.First(t => t.Id == cmd.TraceId);

            if (cmd.Ids.ContainsKey(BaseRefType.Precise))
                traceVm.PreciseId = cmd.Ids[BaseRefType.Precise];
            if (cmd.Ids.ContainsKey(BaseRefType.Fast))
                traceVm.FastId = cmd.Ids[BaseRefType.Fast];
            if (cmd.Ids.ContainsKey(BaseRefType.Additional))
                traceVm.AdditionalId = cmd.Ids[BaseRefType.Additional];
        }

    }

}

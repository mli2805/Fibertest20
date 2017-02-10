using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;

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

            var questionViewModel = new QuestionViewModel("Accept the path?");
            _windowManager.ShowDialog(questionViewModel);
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
                Id = Guid.NewGuid(),
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

            _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", "Last node of trace must contain some equipment"));
            return false;
        }

        private List<Guid> GetPath(RequestAddTrace request)
        {
            List<Guid> path;
            if (!new PathFinder(ReadModel).FindPath(request.NodeWithRtuId, request.LastNodeId, out path))
                _windowManager.ShowDialog(new NotificationViewModel("Ошибка!", "Path couldn't be found"));

            return path;
        }

        private void HighlightTrace(List<Guid> nodes)
        {
            var fibers = ReadModel.GetFibersByNodes(nodes);
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

        private void ApplyToMap(AddTrace cmd)
        {
            GraphVm.Traces.Add(new TraceVm() { Id = cmd.Id, Nodes = cmd.Nodes });
        }

        private AssignBaseRef PrepareCommand(RequestAssignBaseRef request)
        {
            var vm = new BaseRefsAssignViewModel(request.TraceId, ReadModel);
            _windowManager.ShowDialog(vm);
            return vm.Command;
        }

        private void ApplyToMap(AssignBaseRef cmd)
        {
            var traceVm = GraphVm.Traces.First(t => t.Id == cmd.TraceId);

            if (cmd.Ids.ContainsKey(BaseRefType.Precise))
                traceVm.PreciseId = cmd.Ids[BaseRefType.Precise];
            if (cmd.Ids.ContainsKey(BaseRefType.Fast))
                traceVm.FastId = cmd.Ids[BaseRefType.Fast];
            if (cmd.Ids.ContainsKey(BaseRefType.Additional))
                traceVm.AdditionalId = cmd.Ids[BaseRefType.Additional];
        }

    }

}

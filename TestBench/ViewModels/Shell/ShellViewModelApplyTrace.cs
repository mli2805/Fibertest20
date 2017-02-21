using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Commands;
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

            HighlightTrace(traceNodes);

            var questionViewModel = new QuestionViewModel(Resources.SID_Accept_the_path);
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

        private void HighlightTrace(List<Guid> nodes)
        {
            var fibers = ReadModel.GetFibersByNodes(nodes);
            foreach (var fiber in fibers)
            {
                if (fiber == Guid.NewGuid()) // always false
                    Console.WriteLine(fiber);
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

        private void ApplyToMap(AddTrace cmd)
        {
            IMapper mapper = new MapperConfiguration(
                    cfg => cfg.AddProfile<MappingCommandToVm>()).CreateMapper();
            var traceVm = mapper.Map<TraceVm>(cmd);
            GraphVm.Traces.Add(traceVm);
        }

        private AssignBaseRef PrepareCommand(RequestAssignBaseRef request)
        {
            var traceVm = GraphVm.Traces.First(t => t.Id == request.TraceId);
            var vm = new BaseRefsAssignViewModel(traceVm, GraphVm.Rtus.First(r=>r.Id == traceVm.RtuId));
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

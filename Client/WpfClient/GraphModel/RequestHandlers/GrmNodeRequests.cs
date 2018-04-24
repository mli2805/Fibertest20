using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Requests;
using Iit.Fibertest.StringResources;
using Iit.Fibertest.WcfServiceForClientInterface;

namespace Iit.Fibertest.Client
{
    public class GrmNodeRequests
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly IWindowManager _windowManager;
        private readonly Model _model;

        public GrmNodeRequests(IWcfServiceForClient c2DWcfManager, IWindowManager windowManager, Model model)
        {
            _c2DWcfManager = c2DWcfManager;
            _windowManager = windowManager;
            _model = model;
        }

        public async Task MoveNode(MoveNode cmd)
        {
            await _c2DWcfManager.SendCommandAsObj(cmd);
        }

        public async Task AddNodeIntoFiber(RequestAddNodeIntoFiber request)
        {
            var cmd = PrepareAddNodeIntoFiber(request);
            if (cmd == null)
                return;
            var message = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
        }

        private AddNodeIntoFiber PrepareAddNodeIntoFiber(RequestAddNodeIntoFiber request)
        {
            if (request.InjectionType != EquipmentType.AdjustmentPoint && IsFiberContainedInAnyTraceWithBase(request.FiberId))
            {
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, Resources.SID_It_s_impossible_to_change_trace_with_base_reflectogram));
                return null;
            }

            return new AddNodeIntoFiber()
            {
                Id = Guid.NewGuid(),
                EquipmentId = Guid.NewGuid(),
                Position = request.Position,
                InjectionType = request.InjectionType,
                FiberId = request.FiberId,
                NewFiberId1 = Guid.NewGuid(),
                NewFiberId2 = Guid.NewGuid()
            };
        }

        public bool IsFiberContainedInAnyTraceWithBase(Guid fiberId)
        {
            var fiber = _model.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
            if (fiber == null) return false;
            return _model.Traces.Where(t => t.HasAnyBaseRef).ToList().Any(trace => _model.GetFiberIndexInTrace(trace, fiber) != -1);
        }

        public async Task RemoveNode(Guid nodeId, EquipmentType type)
        {
            if (!IsRemoveThisNodePermitted(nodeId, type)) return;

            var detoursForGraph = new List<NodeDetour>();
            foreach (var trace in _model.Traces.Where(t => t.NodeIds.Contains(nodeId)))
            {
               AddDetoursForTrace(nodeId, trace, detoursForGraph);
            }
            var cmd = new RemoveNode { NodeId = nodeId, Type = type, DetoursForGraph = detoursForGraph };

            if (detoursForGraph.Count == 0 && type == EquipmentType.AdjustmentPoint)
                cmd.FiberIdToDetourAdjustmentPoint = Guid.NewGuid();

            var message = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
        }


        private void AddDetoursForTrace(Guid nodeId, Trace trace, List<NodeDetour> alreadyMadeDetours)
        {
            for (int i = 1; i < trace.NodeIds.Count; i++)
            {
                if (trace.NodeIds[i] != nodeId) continue;

                if (trace.NodeIds[i - 1] == trace.NodeIds[i + 1] &&
                    _model.Equipments.First(e => e.EquipmentId == trace.EquipmentIds[i - 1]).Type ==
                    EquipmentType.AdjustmentPoint)
                {
                    var result = _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Confirmation, "It's a trace turn. To remove the turn adjustment points will be deleted. Continue?"));

                }

                var detour = new NodeDetour()
                {
                    FiberId = Guid.NewGuid(), // if there is a fiber between NodeId1 and NodeId2 already - new fiberId just won't be used
                    NodeId1 = trace.NodeIds[i - 1],
                    NodeId2 = trace.NodeIds[i + 1],
                    TraceState = trace.State,
                    TraceId = trace.TraceId,
                };
                alreadyMadeDetours.Add(detour);
            }
        }

        private bool IsRemoveThisNodePermitted(Guid nodeId, EquipmentType type)
        {
            if (_model.Traces.Any(t => t.NodeIds.Contains(nodeId) && t.HasAnyBaseRef) && type != EquipmentType.AdjustmentPoint)
                return false;
            return _model.Traces.All(t => t.NodeIds.Last() != nodeId);
        }
    }
}
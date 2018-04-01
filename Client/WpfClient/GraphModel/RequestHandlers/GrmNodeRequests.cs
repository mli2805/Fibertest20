using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;
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
            return _model.Traces.Where(t => t.HasAnyBaseRef).ToList().Any(trace => Topo.GetFiberIndexInTrace(trace, fiber) != -1);
        }

        public async Task RemoveNode(Guid nodeId, EquipmentType type)
        {
            if (_model.Traces.Any(t => t.NodeIds.Last() == nodeId))
                return;
            if (_model.Traces.Any(t => t.NodeIds.Contains(nodeId) && t.HasAnyBaseRef) && type != EquipmentType.AdjustmentPoint)
                return;

            var detoursForGraph = new List<NodeDetour>();
            var detoursForTracesInModel = new Dictionary<Guid, Guid>();
            foreach (var trace in _model.Traces.Where(t => t.NodeIds.Contains(nodeId)))
            {
                var removedNodeIndex = trace.NodeIds.IndexOf(nodeId);
                var detourFiberId = Guid.NewGuid();

                detoursForTracesInModel.Add(trace.TraceId, detourFiberId);
                var detour = new NodeDetour()
                {
                    FiberId = detourFiberId,
                    NodeId1 = trace.NodeIds[removedNodeIndex-1],
                    NodeId2 = trace.NodeIds[removedNodeIndex+1],
                    TraceState = trace.State,
                    TraceId = trace.TraceId,
                };
                detoursForGraph.Add(detour);
            }

           
            var cmd = new RemoveNode { NodeId = nodeId, Type = type, TraceWithNewFiberForDetourRemovedNode = detoursForTracesInModel, DetoursForGraph = detoursForGraph};
            if (detoursForTracesInModel.Count == 0 && type == EquipmentType.AdjustmentPoint)
            {
                cmd.FiberIdToDetourAdjustmentPoint = Guid.NewGuid();
            }

            var message = await _c2DWcfManager.SendCommandAsObj(cmd);
            if (message != null)
                _windowManager.ShowDialogWithAssignedOwner(new MyMessageBoxViewModel(MessageType.Error, message));
        }
    }
}
using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class RootRenderer
    {
        private readonly Model _readModel;
        private readonly CurrentlyHiddenRtu _currentlyHiddenRtu;

        public RootRenderer(Model readModel, CurrentlyHiddenRtu currentlyHiddenRtu)
        {
            _readModel = readModel;
            _currentlyHiddenRtu = currentlyHiddenRtu;
        }

        public RenderingResult ShowAllMinusHiddenTraces()
        {
            var renderingResult = ShowAll();

            MinusHiddenTraces(renderingResult);

            return renderingResult;
        }

        public RenderingResult ShowNothingPlusShownTraces()
        {
            var renderingResult = ShowOnlyRtusAndNotInTraces();
            PlusShownTraces(renderingResult);
            return renderingResult;
        }

        public RenderingResult ShowAll()
        {
            var renderingResult = new RenderingResult();

            foreach (var node in _readModel.Nodes)
                renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers)
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, renderingResult.NodeVms);
                if (fiberVm != null)
                    renderingResult.FiberVms.Add(fiberVm);
            }

            return renderingResult;
        }

        public RenderingResult ShowOnlyRtusAndNotInTraces() // HideAll()
        {
            var renderingResult = new RenderingResult();

            foreach (var node in _readModel.Nodes)
                if (_readModel.Rtus.Any(r => r.NodeId == node.NodeId) ||
                    (node.AccidentOnTraceId == Guid.Empty && !_readModel.Traces.Any(t => t.NodeIds.Contains(node.NodeId))))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            foreach (var fiber in _readModel.Fibers.Where(f => f.States.Count == 0))
            {
                var fiberVm = ElementRenderer.MapWithStates(fiber, renderingResult.NodeVms);
                if (fiberVm != null)
                    renderingResult.FiberVms.Add(fiberVm);
            }

            return renderingResult;
        }

        private void PlusShownTraces(RenderingResult renderingResult)
        {
            var traces = _readModel.Traces.Where(t => !_currentlyHiddenRtu.Collection.Contains(t.RtuId)).ToList();
            foreach (var trace in traces)
            {
                foreach (var nodeId in trace.NodeIds)
                {
                    var nodeVm = renderingResult.NodeVms.FirstOrDefault(n => n.Id == nodeId);
                    if (nodeVm == null)
                    {
                        nodeVm = ElementRenderer.Map(_readModel.Nodes.First(n => n.NodeId == nodeId));
                        renderingResult.NodeVms.Add(nodeVm);
                    }
                }

                var fibers = _readModel.GetTraceFibers(trace).ToList();
                foreach (var fiber in fibers)
                {
                    var fiberVm = renderingResult.FiberVms.FirstOrDefault(f => f.Id == fiber.FiberId);
                    if (fiberVm == null)
                    {
                        fiberVm = ElementRenderer.Map(fiber, renderingResult.NodeVms);
                        renderingResult.FiberVms.Add(fiberVm);
                    }
                    fiberVm.SetState(trace.TraceId, trace.State);
                    if (fiber.TracesWithExceededLossCoeff.ContainsKey(trace.TraceId))
                        fiberVm.SetBadSegment(trace.TraceId, fiber.TracesWithExceededLossCoeff[trace.TraceId]);
                }

                foreach (var node in _readModel.Nodes.Where(n=>n.AccidentOnTraceId == trace.TraceId))
                {
                    var nodeVm = ElementRenderer.Map(_readModel.Nodes.First(n => n.NodeId == node.NodeId));
                    renderingResult.NodeVms.Add(nodeVm);
                }
            }
        }

        private void MinusHiddenTraces(RenderingResult renderingResult)
        {
            foreach (var trace in _readModel.Traces.Where(t => _currentlyHiddenRtu.Collection.Contains(t.RtuId)))
            {
                var fibers = _readModel.GetTraceFibers(trace).ToList();
                foreach (var fiber in fibers)
                {
                    var fiberVm = renderingResult.FiberVms.FirstOrDefault(f => f.Id == fiber.FiberId);
                    if (fiberVm == null) continue;
                    if (fiberVm.States.ContainsKey(trace.TraceId))
                        fiberVm.States.Remove(trace.TraceId);
                    if (fiberVm.TracesWithExceededLossCoeff.ContainsKey(trace.TraceId))
                        fiberVm.TracesWithExceededLossCoeff.Remove(trace.TraceId);

                    if (fiberVm.States.Count == 0)
                        renderingResult.FiberVms.Remove(fiberVm);
                }

                foreach (var nodeId in trace.NodeIds)
                {
                    var nodeVm = renderingResult.NodeVms.FirstOrDefault(n => n.Id == nodeId);
                    if (nodeVm == null) continue;
                    if (nodeVm.Type == EquipmentType.Rtu) continue;
                    if (renderingResult.FiberVms.Any(f => f.Node1.Id == nodeId || f.Node2.Id == nodeId)) continue;
                    renderingResult.NodeVms.Remove(nodeVm);
                }

                foreach (var nodeVm in renderingResult.NodeVms.Where(n => n.AccidentOnTraceVmId == trace.TraceId).ToList())
                    renderingResult.NodeVms.Remove(nodeVm);
            }
        }
    }
}
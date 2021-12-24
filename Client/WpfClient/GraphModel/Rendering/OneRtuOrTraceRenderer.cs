using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class OneRtuOrTraceRenderer
    {
        private readonly Model _readModel;

        public OneRtuOrTraceRenderer(Model readReadModel)
        {
            _readModel = readReadModel;
        }

        public void GetTraceRendering(Trace trace, RenderingResult renderingResult)
        {
            RenderNodesOfTrace(trace, renderingResult);
            RenderAccidentNodesOfTrace(trace, renderingResult);
            RenderFibersOfTrace(trace, renderingResult);
        }

        private void RenderFibersOfTrace(Trace trace, RenderingResult renderingResult)
        {
            var fibers = _readModel.GetTraceFibers(trace);
            foreach (var fiber in fibers)
            {
                var fiberVm = renderingResult.FiberVms.FirstOrDefault(f => f.Id == fiber.FiberId); // prevent repeating fibers if trace has loop
                if (fiberVm == null)
                {
                    if (fiber.FiberId.ToString().StartsWith(@"821dde"))
                    {
                        Console.WriteLine(@"aaa");
                    }

                    fiberVm = ElementRenderer.Map(fiber, renderingResult.NodeVms);
                    if (fiberVm == null) continue; // something goes wrong, nodeVms not found to define fiberVm
                    if (trace.IsHighlighted) fiberVm.SetLight(trace.TraceId, true);
                    renderingResult.FiberVms.Add(fiberVm);
                }

                if (!fiberVm.States.ContainsKey(trace.TraceId)) // trace could contains loop (pass the same fiber twice or more)
                    fiberVm.States.Add(trace.TraceId, trace.State);
                if (fiber.TracesWithExceededLossCoeff.ContainsKey(trace.TraceId) &&
                    !fiberVm.TracesWithExceededLossCoeff.ContainsKey(trace.TraceId))
                    fiberVm.TracesWithExceededLossCoeff.Add(trace.TraceId, fiber.TracesWithExceededLossCoeff[trace.TraceId]);
            }
        }

        private void RenderAccidentNodesOfTrace(Trace trace, RenderingResult renderingResult)
        {
            foreach (var node in _readModel.Nodes.Where(n =>
                n.TypeOfLastAddedEquipment == EquipmentType.AccidentPlace && n.AccidentOnTraceId == trace.TraceId))
                renderingResult.NodeVms.Add(ElementRenderer.Map(node));
        }

        private void RenderNodesOfTrace(Trace trace, RenderingResult renderingResult)
        {
            foreach (var nodeId in trace.NodeIds)
            {
                if (renderingResult.NodeVms.Any(n => n.Id == nodeId)) continue;
                var node = _readModel.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (node != null)
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
        }

    }
}
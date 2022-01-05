using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class GraphRendererForRoot
    {
        public static async Task<RenderingResult> RenderForRoot(this GraphReadModel graphReadModel)
        {
            await Task.Delay(1);
            if ((graphReadModel.MainMap?.Zoom ?? 15) < graphReadModel.CurrentGis.ThresholdZoom)
            {
                var res = new RenderingResult().RenderRtus(graphReadModel, Guid.Empty);
                var forcedTraces = graphReadModel.CurrentGis.Traces.ToList();
                return res.RenderForcedTracesAndNotInTracesNodes(graphReadModel, forcedTraces)
                    .RenderForcedTracesAndNotInTracesFibers(graphReadModel, forcedTraces);
            }

            return new RenderingResult()
                .RenderAllNodes(graphReadModel)
                .RenderAllFibers(graphReadModel);
        }

        private static RenderingResult RenderForcedTracesAndNotInTracesNodes(this RenderingResult renderingResult, 
            GraphReadModel graphReadModel, List<Trace> forcedTraces)
        {
            var allTracesNodes = new HashSet<Guid>();
            var forcedNodes = new HashSet<Guid>();
            foreach (var trace in graphReadModel.ReadModel.Traces)
            {
                allTracesNodes.UnionWith(trace.NodeIds);
                if (forcedTraces.Contains(trace))
                    forcedNodes.UnionWith(trace.NodeIds);
            }

            foreach (var node in graphReadModel.ReadModel.Nodes)
            {
                if (forcedNodes.Contains(node.NodeId) || !allTracesNodes.Contains(node.NodeId))
                {
                    if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                        renderingResult.NodeVms.Add(ElementRenderer.Map(node));
                }
            }

            return renderingResult;
        }

        private static RenderingResult RenderForcedTracesAndNotInTracesFibers(this RenderingResult renderingResult,
            GraphReadModel graphReadModel, List<Trace> forcedTraces)
        {
            var allTracesFibers = new HashSet<Guid>();
            var forcedFibers = new HashSet<Guid>();
            foreach (var trace in graphReadModel.ReadModel.Traces)
            {
                allTracesFibers.UnionWith(trace.FiberIds);
                if (forcedTraces.Contains(trace))
                    forcedFibers.UnionWith(trace.FiberIds);
            }

            var nodesNear = new List<NodeVm>();
            foreach (var fiber in graphReadModel.ReadModel.Fibers)
            {
                if (forcedFibers.Contains(fiber.FiberId) || !allTracesFibers.Contains(fiber.FiberId))
                {
                    if (GraphRendererCommonDetails.FindFiberNodes(fiber, graphReadModel.ReadModel, renderingResult, nodesNear, 
                                        out NodeVm nodeVm1, out NodeVm nodeVm2))
                        renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
                }
            }

            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

        private static RenderingResult RenderAllNodes(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            foreach (var node in graphReadModel.ReadModel.Nodes)
            {
                if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
            return renderingResult;
        }

        private static RenderingResult RenderAllFibers(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            var nodesNear = new List<NodeVm>();
            foreach (var fiber in graphReadModel.ReadModel.Fibers)
            {
                if (GraphRendererCommonDetails.FindFiberNodes(
                        fiber, graphReadModel.ReadModel, renderingResult, nodesNear, out NodeVm nodeVm1, out NodeVm nodeVm2))
                    renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
            }
            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

      
    }
}

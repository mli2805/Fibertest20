using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class GraphRendererForOperator
    {
        public static async Task<RenderingResult> RenderForOperator(this GraphReadModel graphReadModel)
        {
            await Task.Delay(1);
            if (graphReadModel.MainMap == null || graphReadModel.MainMap.Zoom < graphReadModel.CurrentGis.ThresholdZoom)
            {
                var res = new RenderingResult().RenderRtus(graphReadModel);
                var forcedTraces = graphReadModel.CurrentGis.Traces.ToList();
                return res.RenderForcedTracesNodes(graphReadModel, forcedTraces)
                        .RenderForcedTracesFibers(graphReadModel, forcedTraces);
            }

            var res1 = new RenderingResult().RenderRtus(graphReadModel);
            return res1
                .RenderAllTraceNodes(graphReadModel)
                .RenderAllTraceFibers(graphReadModel);
        }

        private static RenderingResult RenderForcedTracesNodes(this RenderingResult renderingResult,
            GraphReadModel graphReadModel, List<Trace> forcedTraces)
        {
            var existingNodes = new HashSet<Guid>();
            foreach (var trace in forcedTraces)
            {
                foreach (var nodeId in trace.NodeIds)
                {
                    if (!existingNodes.Contains(nodeId))
                    {
                        existingNodes.Add(nodeId);
                        var node = graphReadModel.ReadModel.Nodes.First(n => n.NodeId == nodeId);
                        if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                            renderingResult.NodeVms.Add(ElementRenderer.Map(node));
                    }
                }
            }

            return renderingResult;
        }

        private static RenderingResult RenderForcedTracesFibers(this RenderingResult renderingResult,
            GraphReadModel graphReadModel, List<Trace> forcedTraces)
        {
            var nodesNear = new List<NodeVm>();
            var checkedFibers = new HashSet<Guid>();
            foreach (var trace in forcedTraces)
            {
                foreach (var fiberId in trace.FiberIds)
                {
                    if (!checkedFibers.Contains(fiberId))
                    {
                        checkedFibers.Add(fiberId);
                        var fiber = graphReadModel.ReadModel.Fibers.First(f => f.FiberId == fiberId);
                        if (GraphRendererCommonDetails.FindFiberNodes(
                                fiber, graphReadModel.ReadModel, renderingResult, nodesNear, out NodeVm nodeVm1, out NodeVm nodeVm2))
                            renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
                    }
                }
            }
            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

        private static RenderingResult RenderAllTraceNodes(this RenderingResult renderingResult,
            GraphReadModel graphReadModel)
        {
            var allTracesNodes = new HashSet<Guid>();
            foreach (var trace in graphReadModel.ReadModel.Traces)
                allTracesNodes.UnionWith(trace.NodeIds);

            foreach (var nodeId in allTracesNodes)
            {
                var node = graphReadModel.ReadModel.Nodes.First(n => n.NodeId == nodeId);
                if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            }
            return renderingResult;
        }

        private static RenderingResult RenderAllTraceFibers(this RenderingResult renderingResult,
            GraphReadModel graphReadModel)
        {
            var allTracesFibers = new HashSet<Guid>();
            foreach (var trace in graphReadModel.ReadModel.Traces)
                allTracesFibers.UnionWith(trace.FiberIds);

            var nodesNear = new List<NodeVm>();
            foreach (var fiberId in allTracesFibers)
            {
                var fiber = graphReadModel.ReadModel.Fibers.First(f => f.FiberId == fiberId);
                if (GraphRendererCommonDetails.FindFiberNodes(fiber, graphReadModel.ReadModel, renderingResult, nodesNear,
                        out NodeVm nodeVm1, out NodeVm nodeVm2))
                    renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
            }

            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }
    }
}
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
            if (graphReadModel.MainMap == null || graphReadModel.MainMap.Zoom < graphReadModel.CurrentGis.ThresholdZoom)
            {
                var res = new RenderingResult().RenderRtus(graphReadModel);
                var forcedTraces = graphReadModel.CurrentGis.Traces.ToList();
                return res.RenderNodesForRoot(graphReadModel, forcedTraces)
                    .RenderFibersForRoot(graphReadModel, forcedTraces);
            }

            return new RenderingResult()
                .RenderNodes(graphReadModel)
                .RenderFibers(graphReadModel);
        }

        private static RenderingResult RenderNodesForRoot(this RenderingResult renderingResult, 
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

        private static RenderingResult RenderFibersForRoot(this RenderingResult renderingResult,
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
            var checkedFibers = new HashSet<Guid>();
            foreach (var fiber in graphReadModel.ReadModel.Fibers)
            {
                if ((forcedFibers.Contains(fiber.FiberId) || !allTracesFibers.Contains(fiber.FiberId)) 
                    && !checkedFibers.Contains(fiber.FiberId))
                {
                    checkedFibers.Add(fiber.FiberId);
                    if (FindFiberNodes(fiber, graphReadModel.ReadModel, renderingResult, nodesNear, 
                                        out NodeVm nodeVm1, out NodeVm nodeVm2))
                        renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
                }
            }

            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

        public static RenderingResult RenderNodes(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            foreach (var node in graphReadModel.ReadModel.Nodes)
            {
                if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
            return renderingResult;
        }

        public static RenderingResult RenderFibers(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            var nodesNear = new List<NodeVm>();
            foreach (var fiber in graphReadModel.ReadModel.Fibers)
            {
                if (FindFiberNodes(fiber, graphReadModel.ReadModel, renderingResult, nodesNear, out NodeVm nodeVm1, out NodeVm nodeVm2))
                    renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
            }
            renderingResult.NodeVms.AddRange(nodesNear);
            return renderingResult;
        }

        public static bool FindFiberNodes(Fiber fiber, Model readModel, RenderingResult renderingResult,
            List<NodeVm> nodesNear, out NodeVm nodeVm1, out NodeVm nodeVm2)
        {
            nodeVm1 = renderingResult.NodeVms.FirstOrDefault(n => n.Id == fiber.NodeId1);
            nodeVm2 = renderingResult.NodeVms.FirstOrDefault(n => n.Id == fiber.NodeId2);

            #region One node of the fiber is on screen while other is out
            if (nodeVm1 != null && nodeVm2 == null)
                nodeVm2 = FindNeighbor(fiber.NodeId2, readModel, nodesNear);
            if (nodeVm1 == null && nodeVm2 != null)
                nodeVm1 = FindNeighbor(fiber.NodeId1, readModel, nodesNear);
            #endregion

            return nodeVm1 != null && nodeVm2 != null;
        }

        private static NodeVm FindNeighbor(Guid nodeId, Model readModel, List<NodeVm> nodesNear)
        {
            var neighbor = nodesNear.FirstOrDefault(n => n.Id == nodeId);
            if (neighbor == null)
            {
                var node = readModel.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
                if (node == null) return null;
                neighbor = ElementRenderer.Map(node);
                nodesNear.Add(neighbor);
            }

            return neighbor;
        }
    }
}

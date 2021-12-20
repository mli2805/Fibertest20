using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class GraphRenderer
    {
        public static async Task<RenderingResult> Render(this GraphReadModel graphReadModel)
        {
            await Task.Delay(1);
            if (graphReadModel.MainMap == null || graphReadModel.MainMap.Zoom < graphReadModel.CurrentGis.ThresholdZoom)
            {
                var res = new RenderingResult().RenderRtus(graphReadModel);
                return graphReadModel.ActiveTrace.Trace == null
                    ? res
                    : res.RenderOneTraceNodes(graphReadModel)
                        .RenderOneTraceFibers(graphReadModel);
            }

            return new RenderingResult()
                .RenderNodes(graphReadModel)
                .RenderFibers(graphReadModel);
        }


        private static RenderingResult RenderRtus(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            foreach (var rtu in graphReadModel.ReadModel.Rtus)
            {
                var nodeRtu = graphReadModel.ReadModel.Nodes.First(n => n.NodeId == rtu.NodeId);
                if (graphReadModel.MainMap.Limits.IsInPlus(nodeRtu.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(nodeRtu));
            }

            return renderingResult;
        }

        private static RenderingResult RenderOneTraceNodes(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            foreach (var nodeId in graphReadModel.ActiveTrace.Trace.NodeIds)
            {
                var node = graphReadModel.ReadModel.Nodes.First(n => n.NodeId == nodeId);
                if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            }

            return renderingResult;
        }

        private static RenderingResult RenderOneTraceFibers(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            var nodesNear = new List<NodeVm>();
            foreach (var fiberId in graphReadModel.ActiveTrace.Trace.FiberIds)
            {
                var fiber = graphReadModel.ReadModel.Fibers.First(f => f.FiberId == fiberId);
                if (FindFiberNodes(fiber, graphReadModel.ReadModel, renderingResult, nodesNear, out NodeVm nodeVm1, out NodeVm nodeVm2))
                    renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
            }
            renderingResult.NodeVms.AddRange(nodesNear);     
            return renderingResult;
        }

        private static RenderingResult RenderNodes(this RenderingResult renderingResult, GraphReadModel graphReadModel)
        {
            foreach (var node in graphReadModel.ReadModel.Nodes)
            {
                if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
            return renderingResult;
        }

        private static RenderingResult RenderFibers(this RenderingResult renderingResult, GraphReadModel graphReadModel)
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

        private static bool FindFiberNodes(Fiber fiber, Model readModel, RenderingResult renderingResult, 
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
                neighbor = ElementRenderer.Map(readModel.Nodes.First(n => n.NodeId == nodeId));
                nodesNear.Add(neighbor);
            }

            return neighbor;
        }
    }
}
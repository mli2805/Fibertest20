using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class GraphRenderer
    {
        public static async Task<RenderingResult> Render(this Model readModel, Trace activeTrace, MapLimits limits, double zoom)
        {
            await Task.Delay(1);
            if (zoom < 17)
            {
                var res = new RenderingResult().RenderRtus(readModel, limits);
                return activeTrace == null
                    ? res
                    : res.RenderOneTraceNodes(readModel, activeTrace, limits)
                        .RenderOneTraceFibers(readModel, activeTrace);
            }

            return new RenderingResult()
                .RenderNodes(readModel, limits)
                .RenderFibers(readModel);
        }

        private static RenderingResult RenderRtus(this RenderingResult renderingResult, Model readModel,
            MapLimits limits)
        {
            foreach (var rtu in readModel.Rtus)
            {
                var nodeRtu = readModel.Nodes.First(n => n.NodeId == rtu.NodeId);
                if (limits.IsInPlus(nodeRtu.Position, 0.5))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(nodeRtu));
            }

            return renderingResult;
        }

        private static RenderingResult RenderOneTraceNodes(this RenderingResult renderingResult, Model readModel, Trace trace,
            MapLimits limits)
        {
            foreach (var nodeId in trace.NodeIds)
            {
                var node = readModel.Nodes.First(n => n.NodeId == nodeId);
                if (limits.IsInPlus(node.Position, 0.5))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));

            }

            return renderingResult;
        }

        private static RenderingResult RenderOneTraceFibers(this RenderingResult renderingResult, Model readModel,
            Trace trace)
        {
            var nodesNear = new List<NodeVm>();
            foreach (var fiberId in trace.FiberIds)
            {
                var fiber = readModel.Fibers.First(f => f.FiberId == fiberId);
                if (FindFiberNodes(fiber, readModel, renderingResult, nodesNear, out NodeVm nodeVm1, out NodeVm nodeVm2))
                    renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
            }
            renderingResult.NodeVms.AddRange(nodesNear);     
            return renderingResult;
        }

        private static RenderingResult RenderNodes(this RenderingResult renderingResult, Model readModel, MapLimits limits)
        {
            foreach (var node in readModel.Nodes)
            {
                if (limits.IsInPlus(node.Position, 0.5))
                    renderingResult.NodeVms.Add(ElementRenderer.Map(node));
            }
            return renderingResult;
        }

        private static RenderingResult RenderFibers(this RenderingResult renderingResult, Model readModel)
        {
            var nodesNear = new List<NodeVm>();
            foreach (var fiber in readModel.Fibers)
            {
                if (FindFiberNodes(fiber, readModel, renderingResult, nodesNear, out NodeVm nodeVm1, out NodeVm nodeVm2))
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
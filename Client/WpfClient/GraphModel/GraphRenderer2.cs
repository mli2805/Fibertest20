using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class GraphRenderer2
    {
        public static async Task<RenderingResult> RenderByNumber(this GraphReadModel graphReadModel)
        {
            await Task.Delay(1);
            var result = new RenderingResult()
                .RenderRtus(graphReadModel);
            if (result.RenderNodesNoMoreThan(graphReadModel))
                result.RenderFibers(graphReadModel);

            return result;
        }

        private static bool RenderNodesNoMoreThan(this RenderingResult renderingResult,
            GraphReadModel graphReadModel)
        {   
                                                                               // RTU on screen
            var threshold = graphReadModel.CurrentGis.ThresholdNodeCount - renderingResult.NodeVms.Count;

            var intermediateList = new List<Node>();
            foreach (var node in graphReadModel.ReadModel.Nodes)
            {
                if (graphReadModel.MainMap.Limits.IsInPlus(node.Position, graphReadModel.CurrentGis.ScreenPartAsMargin))
                    intermediateList.Add(node);

                if (intermediateList.Count > threshold)
                    return false; // no nodeVms added at this point 
            }

            renderingResult.NodeVms.AddRange(intermediateList.Select(ElementRenderer.Map));
            return true;
        }
    }
}
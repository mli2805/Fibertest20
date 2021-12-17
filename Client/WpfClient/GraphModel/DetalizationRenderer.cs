using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class DetalizationRenderer
    {
        public static async Task<RenderingResult> Render(this GraphReadModel graphReadModel,
            MapLimits limits, int zoom)
        {
            await Task.Delay(1);
            var renderingResult = new RenderingResult();
            var readModel = graphReadModel.ReadModel;
            foreach (var trace in readModel.Traces)
            {
                var det = readModel.GetTraceDetalization(trace, GetDetalizationMode(zoom));
                foreach (var nodeId in det.Nodes)
                {
                    if (renderingResult.NodeVms.Any(n => n.Id == nodeId)) continue;
                    var node = readModel.Nodes.FirstOrDefault(n => n.NodeId == nodeId);

                    if (node != null && limits.IsInPlus(node.Position, 0))
                        renderingResult.NodeVms.Add(ElementRenderer.Map(node));
                }

                var nodesNear = new List<NodeVm>();
                foreach (var fiber in det.Fibers)
                {
                    if (renderingResult.FiberVms.Any(f => f.Id == fiber.FiberId)) continue;

                    var nodeVm1 = renderingResult.NodeVms.FirstOrDefault(n => n.Id == fiber.NodeId1);
                    var nodeVm2 = renderingResult.NodeVms.FirstOrDefault(n => n.Id == fiber.NodeId2);
                    
                    #region One node of the fiber is on screen while other is out
                    if (nodeVm1 != null && nodeVm2 == null)
                    {
                        nodeVm2 = nodesNear.FirstOrDefault(n => n.Id == fiber.NodeId2);
                        if (nodeVm2 == null)
                        {
                            nodeVm2 = ElementRenderer.Map(readModel.Nodes.First(n => n.NodeId == fiber.NodeId2));
                            nodesNear.Add(nodeVm2);
                        }
                    }
                    if (nodeVm1 == null && nodeVm2 != null)
                    {
                        nodeVm1 = nodesNear.FirstOrDefault(n => n.Id == fiber.NodeId1);
                        if (nodeVm1 == null)
                        {
                            nodeVm1 = ElementRenderer.Map(readModel.Nodes.First(n => n.NodeId == fiber.NodeId1));
                            nodesNear.Add(nodeVm1);
                        }
                    }
                    #endregion

                    if (nodeVm1 != null && nodeVm2 != null)
                        renderingResult.FiberVms.Add(ElementRenderer.MapWithStates(fiber, nodeVm1, nodeVm2));
                }

                renderingResult.NodeVms.AddRange(nodesNear);
            }

            return renderingResult;
        }

        private static EquipmentType GetDetalizationMode(int zoom)
        {
            if (zoom > 15) return EquipmentType.AdjustmentPoint;
            // if (zoom > 17) return EquipmentType.EmptyNode;
            // if (zoom > 14) return EquipmentType.AnyEquipment;
            // if (zoom > 13) return EquipmentType.RtuAndEot;
            return EquipmentType.Rtu;
        }

    }
}

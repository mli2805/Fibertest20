using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public static class DetalizationRenderer
    {
        public static async Task<RenderingResult> Render(this GraphReadModel graphReadModel, 
            MapLimits limits, int zoom, int previousZoom, int needRenderingCause)
        {
            var count = Calc(graphReadModel, limits);
            limits.NodeCountString = count.ToString();

            if (needRenderingCause == 3 && GetDetalizationMode(previousZoom) == GetDetalizationMode(zoom))
                return null;

            await graphReadModel.FullClean();
            var renderingResult = new RenderingResult();
            GetRenderingResultFromMode(graphReadModel, limits, GetDetalizationMode(zoom), renderingResult);
            return renderingResult;
        }

        private static void GetRenderingResultFromMode(GraphReadModel graphReadModel,
            MapLimits limits, EquipmentType detalizationMode, RenderingResult renderingResult)
        {
            var readModel = graphReadModel.ReadModel;
            foreach (var trace in readModel.Traces)
            {
                var det = readModel.GetTraceDetalization(trace, detalizationMode);
                foreach (var nodeId in det.Nodes)
                {
                    if (renderingResult.NodeVms.Any(n => n.Id == nodeId)) continue;
                    var node = readModel.Nodes.FirstOrDefault(n => n.NodeId == nodeId);

                    if (node != null && limits.IsInPlus(node.Position, 0.5))
                        renderingResult.NodeVms.Add(ElementRenderer.Map(node));
                }

                foreach (var fiber in det.Fibers)
                {
                    if (renderingResult.FiberVms.Any(f => f.Id == fiber.FiberId)) continue;
                    var fiberVm = ElementRenderer.Map(fiber, renderingResult.NodeVms);
                    if (fiberVm != null)
                        renderingResult.FiberVms.Add(fiberVm);
                }
            }
        }

        private static EquipmentType GetDetalizationMode(int zoom)
        {
            if (zoom > 18) return EquipmentType.AdjustmentPoint;
            if (zoom > 17) return EquipmentType.EmptyNode;
            if (zoom > 14) return EquipmentType.AnyEquipment;
            if (zoom > 13) return EquipmentType.RtuAndEot;
            return EquipmentType.Rtu;
        }

        private static GraphCount Calc(GraphReadModel graphReadModel, MapLimits limits)
        {
            var count = new GraphCount();
            var nodes = graphReadModel.ReadModel.Nodes.Where(n => limits.IsIn(n.Position)).Select(e => e).ToList();
            count.Rtus = nodes.Count(n => n.TypeOfLastAddedEquipment == EquipmentType.Rtu);
            count.Eqs = nodes.Count(n => n.TypeOfLastAddedEquipment > EquipmentType.EmptyNode);
            count.Wells = nodes.Count(n => n.TypeOfLastAddedEquipment > EquipmentType.AdjustmentPoint);
            count.Total = nodes.Count;
            return count;
        }

    }
}

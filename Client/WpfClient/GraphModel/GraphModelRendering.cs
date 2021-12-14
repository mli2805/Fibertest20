using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public static class GraphModelRendering
    {
        public static void Render(this GraphReadModel graphReadModel, MapLimits limits)
        {
            var count = Calc(graphReadModel, limits);
            limits.NodeCountString = count.ToString();
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

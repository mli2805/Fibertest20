using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class TraceModelBuilder
    {
        public static TraceModelForBaseRef ExcludeAdjustmentPoints(this TraceModelForBaseRef originalModel)
        {
            var nodes = new List<Node>() { originalModel.NodeArray[0] };
            var equipments = new List<Equipment>() { originalModel.EquipArray[0] };
            var fibers = new List<Fiber>();
            var distances = new List<int>();

            var distance = 0;
            for (int i = 1; i < originalModel.EquipArray.Length; i++)
            {
                distance += originalModel.DistancesMm[i - 1];

                if (originalModel.EquipArray[i].Type != EquipmentType.AdjustmentPoint)
                {
                    nodes.Add(originalModel.NodeArray[i]);
                    equipments.Add(originalModel.EquipArray[i]);
                    fibers.Add(originalModel.FiberArray[i - 1]);
                    distances.Add(distance);
                    distance = 0;
                }
                else
                {
                    // AdjustmentPoint && UserInputLength
                    // all parts of section contains the same userLength value - take one from the last
                    if (!originalModel.FiberArray[i].UserInputedLength.Equals(0))
                        distance = 0;
                }
            }

            return new TraceModelForBaseRef
            {
                NodeArray = nodes.ToArray(),
                EquipArray = equipments.ToArray(),
                FiberArray = fibers.ToArray(),
                DistancesMm = distances.ToArray(),
            };
        }

    }
}
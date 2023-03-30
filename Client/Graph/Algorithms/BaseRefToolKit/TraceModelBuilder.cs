using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class TraceModelBuilder
    {
        public static TraceModelForBaseRef GetTraceModelWithoutAdjustmentPoints(TraceModelForBaseRef traceModel)
        {
            var fullModel = FillInDistances(traceModel);
            return ExcludeAdjustmentPoints(fullModel);
        }
        
        private static TraceModelForBaseRef FillInDistances(TraceModelForBaseRef model)
        {
            model.DistancesMm = new int[model.FiberArray.Length];
            for (int i = 0; i < model.FiberArray.Length; i++)
            {
                var fiber = model.FiberArray[i];
                if (!fiber.UserInputedLength.Equals(0))
                    model.DistancesMm[i] = (int)fiber.UserInputedLength * 1000;
                else
                    model.DistancesMm[i] = (int)Math.Round(
                        GisLabCalculator.GetDistanceBetweenPointLatLng(
                            model.NodeArray[i].Position, model.NodeArray[i + 1].Position) * 1000, 0);
            }

            return model;
        }

        private static TraceModelForBaseRef ExcludeAdjustmentPoints(TraceModelForBaseRef originalModel)
        {
            var nodes = new List<Node>(){originalModel.NodeArray[0]};
            var equipments = new List<Equipment>(){originalModel.EquipArray[0]};
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
                    fibers.Add(originalModel.FiberArray[i-1]);
                    distances.Add(distance);
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
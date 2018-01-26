using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class TraceModelBuilder
    {
        private readonly ReadModel _readModel;
        private readonly GraphGpsCalculator _graphGpsCalculator;

        public TraceModelBuilder(ReadModel readModel, GraphGpsCalculator graphGpsCalculator)
        {
            _readModel = readModel;
            _graphGpsCalculator = graphGpsCalculator;
        }

        public TraceModelForBaseRef GetModel(Trace trace)
        {
            var nodes = _readModel.GetTraceNodes(trace).ToArray();
            var equipList =
                new List<Equipment>() { new Equipment() { Type = EquipmentType.Rtu } }; // fake RTU, just for indexes match
            equipList.AddRange(_readModel.GetTraceEquipments(trace).ToList()); // without RTU
            var fibers = _readModel.GetTraceFibers(trace).ToArray();
            var distances = new int[fibers.Length];
            for (int i = 0; i < fibers.Length; i++)
            {
                var fiber = fibers[i];
                if (!fiber.UserInputedLength.Equals(0))
                    distances[i] = (int)fiber.UserInputedLength * 100;
                else
                    distances[i] = _graphGpsCalculator.CalculateDistanceBetweenNodesMm(nodes[i], nodes[i + 1]);
            }

            return new TraceModelForBaseRef
            {
                NodeArray = nodes,
                EquipArray = equipList.ToArray(),
                Distances = distances
            };
        }

        public TraceModelForBaseRef ExcludeAdjustmentPoints(TraceModelForBaseRef originalModel)
        {
            var nodes = new List<Node>(){originalModel.NodeArray[0]};
            var equipments = new List<Equipment>(){originalModel.EquipArray[0]};
            var distances = new List<int>();

            var distance = 0;
            for (int i = 1; i < originalModel.EquipArray.Length; i++)
            {
                if (originalModel.EquipArray[i].Type != EquipmentType.AdjustmentPoint)
                {
                    nodes.Add(originalModel.NodeArray[i]);
                    equipments.Add(originalModel.EquipArray[i]);
                    distances.Add(originalModel.Distances[i-1] + distance);
                    distance = 0;
                }
                else
                {
                    distance = distance + originalModel.Distances[i];
                }
            }

            return new TraceModelForBaseRef
            {
                NodeArray = nodes.ToArray(),
                EquipArray = equipments.ToArray(),
                Distances = distances.ToArray(),
            };
        }


    }
}
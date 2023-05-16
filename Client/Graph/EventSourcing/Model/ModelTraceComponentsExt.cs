using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class ModelTraceComponentsExt
    {
        public static TraceModelForBaseRef GetTraceComponentsByIds(this Model model, Trace trace)
        {
            var nodes = model.GetTraceNodes(trace).ToArray();
            var rtu = model.Rtus.First(r => r.Id == trace.RtuId);
            var equipList =
                new List<Equipment>()
                {
                    new Equipment() {Type = EquipmentType.Rtu, Title = rtu.Title}
                }; // fake RTU, just for indexes match
            equipList.AddRange(model.GetTraceEquipments(trace).ToList()); // without RTU
            var fibers = model.GetTraceFibers(trace).ToArray();

            var traceModel = new TraceModelForBaseRef
            {
                NodeArray = nodes,
                EquipArray = equipList.ToArray(),
                FiberArray = fibers,
            };

            return traceModel.FillInGpsDistancesForTraceModel();
        }

        // модель должна быть с точками привязки
        // если поменяли GPS координаты узлов и пользов длину участков, то эти параметры уже в модели
        // и теперь надо пересчитать массив длин по GPS (distancesMm)
        public static TraceModelForBaseRef FillInGpsDistancesForTraceModel(this TraceModelForBaseRef model)
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

        private static IEnumerable<Node> GetTraceNodes(this Model model, Trace trace)
        {
            return trace.NodeIds.Select(i => model.Nodes.First(eq => eq.NodeId == i));
        }

        public static IEnumerable<Equipment> GetTraceEquipments(this Model model, Trace trace)
        {
            return trace.EquipmentIds.Skip(1).Select(i => model.Equipments.First(eq => eq.EquipmentId == i));
        }

        public static IEnumerable<Fiber> GetTraceFibers(this Model model, Trace trace)
        {
            foreach (var fiberId in trace.FiberIds)
            {
                var fiber = model.Fibers.FirstOrDefault(f => f.FiberId == fiberId);
                if (fiber != null)
                    yield return fiber;
            }
        }

        public static IEnumerable<Guid> GetTraceNodesExcludingAdjustmentPoints(this Model model, Guid traceId)
        {
            var trace = model.Traces.First(t => t.TraceId == traceId);
            foreach (var nodeId in trace.NodeIds)
            {
                var node = model.Nodes.FirstOrDefault(n =>
                    n.NodeId == nodeId && n.TypeOfLastAddedEquipment != EquipmentType.AdjustmentPoint);
                if (node != null)
                    yield return node.NodeId;
            }
        }

        public static IEnumerable<Equipment> GetTraceEquipmentsExcludingAdjustmentPoints(this Model model, Guid traceId)
        {
            var trace = model.Traces.First(t => t.TraceId == traceId);
            foreach (var equipmentId in trace.EquipmentIds.Skip(1)) // 0 - RTU
            {
                var equipment = model.Equipments.First(e => e.EquipmentId == equipmentId);
                if (equipment.Type != EquipmentType.AdjustmentPoint)
                    yield return equipment;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class LandmarksGraphParser
    {
        private readonly Model _readModel;

        public LandmarksGraphParser(Model readModel)
        {
            _readModel = readModel;
        }

        public List<Landmark> GetLandmarksFromModel(TraceModelForBaseRef traceModel)
        {
            var previousNode = traceModel.NodeArray.First();
            var rtuLandmark = CreateRtuLandmark(previousNode);
            rtuLandmark.FiberId = Guid.Empty;
            var result = new List<Landmark> { rtuLandmark };

            var distanceFromRtu = 0.0;
            var sectionAdj = 0.0;
            var j = 1;

            for (int i = 1; i < traceModel.NodeArray.Length; i++)
            {
                var node = traceModel.NodeArray[i];
                var fiber = traceModel.FiberArray[i - 1];
                var section = fiber.UserInputedLength > 0
                    ? fiber.UserInputedLength / 1000
                    : GisLabCalculator.GetDistanceBetweenPointLatLng(previousNode.Position, node.Position) / 1000;
                sectionAdj += section;
                previousNode = node;

                // all parts of section contains the same userLength value - take one from the last
                if (node.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint
                    && fiber.UserInputedLength > 0)
                {
                    sectionAdj = 0;
                }

                if (node.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint) continue;

                distanceFromRtu += section;

                var lm = CreateLandmark(node, traceModel.EquipArray[i].EquipmentId, j++, i);
                lm.FiberId = fiber.FiberId;
                lm.GpsDistance = distanceFromRtu;
                lm.GpsSection = sectionAdj;
                lm.IsUserInput = fiber.UserInputedLength > 0;
                lm.OpticalDistance = 0.0;
                lm.OpticalSection = 0.0;
                result.Add(lm);
                sectionAdj = 0.0;
            }

            return result;
        }

        public List<Landmark> GetLandmarksFromGraph(Trace trace)
        {
            var previousNode = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds[0]);
            var rtuLandmark = CreateRtuLandmark(previousNode);
            rtuLandmark.FiberId = Guid.Empty;
            var result = new List<Landmark> { rtuLandmark };

            var distanceFromRtu = 0.0;
            var sectionAdj = 0.0;
            var j = 1;
            for (var i = 1; i < trace.NodeIds.Count; i++)
            {
                var node = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds[i]);
                var fiber = _readModel.Fibers.First(f => f.FiberId == trace.FiberIds[i-1]);
                var section = fiber.UserInputedLength > 0
                    ? fiber.UserInputedLength / 1000
                    : GisLabCalculator.GetDistanceBetweenPointLatLng(previousNode.Position, node.Position) / 1000;
                sectionAdj += section;
                previousNode = node;

                // all parts of section contains the same userLength value - take one from the last
                if (node.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint
                    && fiber.UserInputedLength > 0)
                {
                    sectionAdj = 0;
                }

                if (node.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint) continue;

                distanceFromRtu += section;

                var lm = CreateLandmark(node, trace.EquipmentIds[i], j++, i);
                lm.FiberId = fiber.FiberId;
                lm.GpsDistance = distanceFromRtu;
                lm.GpsSection = sectionAdj;
                lm.IsUserInput = fiber.UserInputedLength > 0;
                lm.OpticalDistance = 0.0;
                lm.OpticalSection = 0.0;
                result.Add(lm);
                sectionAdj = 0.0;
            }

            return result;
        }

        private Landmark CreateLandmark(Node node, Guid equipmentId, int number, int numberIncludingAdjustmentPoints)
        {
            var equipment = _readModel.Equipments.First(e => e.EquipmentId == equipmentId);
            var comment = number == 0
                ? _readModel.Rtus.First(r => r.NodeId == node.NodeId).Comment
                : node.Comment;
            return new Landmark()
            {
                IsFromBase = false,
                Number = number,
                NumberIncludingAdjustmentPoints = numberIncludingAdjustmentPoints,
                NodeId = node.NodeId,
                NodeTitle = node.Title,
                NodeComment = comment,
                EquipmentId = equipmentId,
                EquipmentTitle = equipment.Title,
                EquipmentType = equipment.Type,
                EventNumber = -1,
                LeftCableReserve = equipment.CableReserveLeft,
                RightCableReserve = equipment.CableReserveRight,
                GpsCoors = node.Position,
            };
        }

        private Landmark CreateRtuLandmark(Node node)
        {
            var rtu = _readModel.Rtus.First(e => e.NodeId == node.NodeId);
            return new Landmark()
            {
                Number = 0,
                NodeId = rtu.NodeId,
                NodeTitle = rtu.Title,
                NodeComment = rtu.Comment,
                EquipmentType = EquipmentType.Rtu,
                OpticalDistance = 0,
                EventNumber = -1,
                GpsCoors = node.Position,
            };
        }
    }
}
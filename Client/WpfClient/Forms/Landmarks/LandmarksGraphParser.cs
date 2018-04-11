using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.Graph.Algorithms;

namespace Iit.Fibertest.Client
{
    public class LandmarksGraphParser
    {
        private readonly Model _readModel;

        public LandmarksGraphParser(Model readModel)
        {
            _readModel = readModel;
        }

        public List<Landmark> GetLandmarks(Trace trace)
        {
            var previousNode = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds[0]);
            var result = new List<Landmark> { CreateRtuLandmark(previousNode) };

            var distance = 0.0;
            for (var i = 1; i < trace.NodeIds.Count; i++)
            {
                var nodeId = trace.NodeIds[i];
                var node = _readModel.Nodes.First(n => n.NodeId == nodeId);
                distance = distance + GpsCalculator.GetDistanceBetweenPointLatLng(previousNode.Position, node.Position) / 1000;
                previousNode = node;
                if (node.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint) continue;

                var lm = CreateLandmark(node, trace.EquipmentIds[i], i);
                lm.Distance = distance;
                result.Add(lm);
            }

            return result;
        }

        private Landmark CreateLandmark(Node node, Guid equipmentId, int number)
        {
            var equipment = _readModel.Equipments.First(e => e.EquipmentId == equipmentId);
            return new Landmark()
            {
                Number = number,
                NodeId = node.NodeId,
                NodeTitle = node.Title,
                EquipmentTitle = equipment.Title,
                EquipmentType = equipment.Type,
                EventNumber = -1,
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
                EquipmentType = EquipmentType.Rtu,
                Distance = 0,
                EventNumber = -1,
                GpsCoors = node.Position,
            };
        }
    }
}
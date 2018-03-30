﻿using System;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class GraphGpsCalculator
    {
        private readonly ReadModel _readModel;

        public GraphGpsCalculator(ReadModel readModel)
        {
            _readModel = readModel;
        }

        public double CalculateTraceGpsLengthKm(Trace trace)
        {
            double result = 0;
            for (int i = 0; i < trace.Nodes.Count - 1; i++)
            {
                var node1 = _readModel.Nodes.FirstOrDefault(n => n.Id == trace.Nodes[i]);
                if (node1 == null) return 0;
                var node2 = _readModel.Nodes.FirstOrDefault(n => n.Id == trace.Nodes[i + 1]);
                if (node2 == null) return 0;

                var equipment1 = i == 0
                    ? new Equipment() { Type = EquipmentType.Rtu, CableReserveLeft = 0, CableReserveRight = 0 }
                    : _readModel.Equipments.FirstOrDefault(e => e.Id == trace.Equipments[i]);
                var equipment2 = _readModel.Equipments.FirstOrDefault(e => e.Id == trace.Equipments[i + 1]);

                result = result +
                         GpsCalculator.GetDistanceBetweenPointLatLng(node1.Position, node2.Position) +
                         GetReserveFromTheLeft(equipment1) + GetReserveFromTheRight(equipment2);
            }
            return result / 1000;
        }

        public int CalculateDistanceBetweenNodesMm(Node leftNode, Equipment leftEquipment, Node rightNode, Equipment rightEquipment)
        {
            var gpsDistance = (int)GpsCalculator.GetDistanceBetweenPointLatLng(leftNode.Position, rightNode.Position);

            return (int)((gpsDistance + GetReserveFromTheLeft(leftEquipment) + GetReserveFromTheRight(rightEquipment)) * 1000);
        }

        private double GetReserveFromTheLeft(Equipment leftEquipment)
        {
            var leftReserve = 0.0;
            if (leftEquipment.Type == EquipmentType.CableReserve)
                leftReserve = (double)leftEquipment.CableReserveLeft / 2;
            else if (leftEquipment.Type > EquipmentType.CableReserve)
                leftReserve = leftEquipment.CableReserveRight;
            return leftReserve;
        }

        private double GetReserveFromTheRight(Equipment rightEquipment)
        {
            var rightReserve = 0.0;
            if (rightEquipment.Type == EquipmentType.CableReserve)
                rightReserve = (double)rightEquipment.CableReserveLeft / 2;
            else if (rightEquipment.Type > EquipmentType.CableReserve)
                rightReserve = rightEquipment.CableReserveLeft;
            return rightReserve;
        }

        public double GetFiberFullGpsDistance(Guid fiberId)
        {
            var fiber = _readModel.Fibers.First(f => f.Id == fiberId);
            var node1 = _readModel.Nodes.First(n => n.Id == fiber.Node1);
            var node2 = _readModel.Nodes.First(n => n.Id == fiber.Node2);
            var result = GpsCalculator.GetDistanceBetweenPointLatLng(node1.Position, node2.Position);

            var fId = fiberId;
            while (node1.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                fiber = _readModel.GetAnotherFiberOfAdjustmentPoint(node1, fId);
                var previousNode1 = node1;
                node1 = _readModel.Nodes.First(n => n.Id == fiber.Node1);
                result = result + GpsCalculator.GetDistanceBetweenPointLatLng(node1.Position, previousNode1.Position);
                fId = fiber.Id;
            }

            fId = fiberId;
            while (node2.TypeOfLastAddedEquipment == EquipmentType.AdjustmentPoint)
            {
                fiber = _readModel.GetAnotherFiberOfAdjustmentPoint(node2, fId);
                var previousNode2 = node2;
                node2 = _readModel.Nodes.First(n => n.Id == fiber.Node2);
                result = result + GpsCalculator.GetDistanceBetweenPointLatLng(node2.Position, previousNode2.Position);
                fId = fiber.Id;
            }

            return result;
        }
    }
}
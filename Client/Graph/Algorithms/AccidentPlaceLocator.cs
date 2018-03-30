using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Graph.Algorithms
{
    public class AccidentPlaceLocator
    {
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;

        public AccidentPlaceLocator(IMyLog logFile, ReadModel readModel)
        {
            _logFile = logFile;
            _readModel = readModel;
        }

        public PointLatLng? GetAccidentGps(AccidentOnTrace accident)
        {
            if (accident is AccidentAsNewEvent accidentAsNewEvent)
                return GetAccidentGps(accidentAsNewEvent);

            if (accident is AccidentInOldEvent accidentInOldEvent)
            {
                var withoutPoints = _readModel.GetTraceNodesExcludingAdjustmentPoints(accident.TraceId).ToList();
                var nodeVm = _readModel.Nodes.FirstOrDefault(n => n.NodeId == withoutPoints[accidentInOldEvent.BrokenLandmarkIndex]);
                return nodeVm?.Position;
            }

            return null;
        }

        private PointLatLng GetAccidentGps(AccidentAsNewEvent accident)
        {
            var trace = _readModel.Traces.First(t => t.TraceId == accident.TraceId);

            var distances = GetGpsDistancesOfSegmentsBetweenLandmarks(accident, trace, out Node leftNodeVm, out Node rightNodeVm);
            GetCableReserves(accident, accident.TraceId, out double leftReserveM, out double rightReserveM);
            var distanceBetweenTwoNodesOnGraphM = distances.Sum();

            var opticalLengthM = (accident.RightNodeKm - accident.LeftNodeKm) * 1000;
            var coeff = opticalLengthM / (distanceBetweenTwoNodesOnGraphM + leftReserveM + rightReserveM);

            var distanceToAccidentOnGraphM = (accident.AccidentDistanceKm - accident.LeftNodeKm) * 1000 / coeff;

            if (distanceToAccidentOnGraphM <= leftReserveM)
                return leftNodeVm.Position;

            if (distanceToAccidentOnGraphM > leftReserveM + distanceBetweenTwoNodesOnGraphM)
                return rightNodeVm.Position;

            var segmentIndex = -1;
            var distancesSum = leftReserveM;
            while (distancesSum < distanceToAccidentOnGraphM)
            {
                segmentIndex++;
                distancesSum = distancesSum + distances[segmentIndex];
            }

            return GetPointOnBrokenSegment(trace, 
                (distances[segmentIndex] - (distancesSum - distanceToAccidentOnGraphM)) / distances[segmentIndex], 
                trace.NodeIds.IndexOf(leftNodeVm.NodeId) + segmentIndex);
        }

        private PointLatLng GetPointOnBrokenSegment(Trace trace, double procentOfSegmentUptoAccident, int leftNodeIndex)
        {
            var leftNode = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds[leftNodeIndex]);
            var rightNode = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds[leftNodeIndex + 1]);

            var latBreak = leftNode.Position.Lat + (rightNode.Position.Lat - leftNode.Position.Lat) * procentOfSegmentUptoAccident;
            var lngBreak = leftNode.Position.Lng + (rightNode.Position.Lng - leftNode.Position.Lng) * procentOfSegmentUptoAccident;
            return new PointLatLng(latBreak, lngBreak);
        }

        private void GetCableReserves(AccidentAsNewEvent accident, Guid traceId, out double leftReserveM, out double rightReserveM)
        {
            var equipmentsWithoutPointsAndRtu = _readModel.GetTraceEquipmentsExcludingAdjustmentPoints(traceId).ToList();
            leftReserveM = GetCableReserve(equipmentsWithoutPointsAndRtu, accident.LeftLandmarkIndex, true);
            rightReserveM = GetCableReserve(equipmentsWithoutPointsAndRtu, accident.LeftLandmarkIndex, false);
        }

        private double GetCableReserve(List<Equipment> equipmentsWithoutPointsAndRtu, int landmarkIndex, bool isLeftLandmark)
        {
            if (landmarkIndex == 0)
                return 0; // RTU cannot contain cable reserve
            var equipment =  equipmentsWithoutPointsAndRtu[landmarkIndex-1];
            if (equipment.Type == EquipmentType.CableReserve) return (double)equipment.CableReserveLeft / 2;
            return isLeftLandmark ? equipment.CableReserveRight : equipment.CableReserveLeft;
        }

        private List<double> GetGpsDistancesOfSegmentsBetweenLandmarks(AccidentAsNewEvent accident, Trace trace, out Node leftNode, out Node rightNode)
        {
            var withoutPoints = _readModel.GetTraceNodesExcludingAdjustmentPoints(accident.TraceId).ToList();
            leftNode = _readModel.Nodes.FirstOrDefault(n => n.NodeId == withoutPoints[accident.LeftLandmarkIndex]);
            rightNode = _readModel.Nodes.FirstOrDefault(n => n.NodeId == withoutPoints[accident.RightLandmarkIndex]);

            if (leftNode == null)
            {
                _logFile.AppendLine($@"Node {withoutPoints[accident.LeftLandmarkIndex].First6()} not found");
                return null;
            }
            if (rightNode == null)
            {
                _logFile.AppendLine($@"Node {withoutPoints[accident.RightLandmarkIndex].First6()} not found");
                return null;
            }

            var indexOfLeft = trace.NodeIds.IndexOf(leftNode.NodeId);
            var indexOfRight = trace.NodeIds.IndexOf(rightNode.NodeId);
            var result = new List<double>();
            var fromNode = leftNode;
            for (int i = indexOfLeft + 1; i < indexOfRight; i++)
            {
                var toNode = _readModel.Nodes.First(n => n.NodeId == trace.NodeIds[i]);
                result.Add(GpsCalculator.GetDistanceBetweenPointLatLng(fromNode.Position, toNode.Position));
                fromNode = toNode;
            }
            result.Add(GpsCalculator.GetDistanceBetweenPointLatLng(fromNode.Position, rightNode.Position));
            return result;
        }
    }
}
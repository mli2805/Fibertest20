using System;
using System.Collections.Generic;
using System.Linq;
using GMap.NET;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class AccidentPlaceLocator
    {
        private readonly GraphReadModel _model;
        private readonly IMyLog _logFile;
        private readonly ReadModel _readModel;

        public AccidentPlaceLocator(IMyLog logFile, ReadModel readModel, GraphReadModel model)
        {
            _model = model;
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
                var nodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == withoutPoints[accidentInOldEvent.BrokenLandmarkIndex]);
                return nodeVm?.Position;
            }

            return null;
        }

        private PointLatLng GetAccidentGps(AccidentAsNewEvent accident)
        {
            var traceVm = _model.Data.Traces.First(t => t.Id == accident.TraceId);

            var distances = GetGpsDistancesOfSegmentsBetweenLandmarks(accident, traceVm, out NodeVm leftNodeVm, out NodeVm rightNodeVm);
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

            return GetPointOnBrokenSegment(traceVm, 
                (distances[segmentIndex] - (distancesSum - distanceToAccidentOnGraphM)) / distances[segmentIndex], 
                traceVm.Nodes.IndexOf(leftNodeVm.Id) + segmentIndex);
        }

        private PointLatLng GetPointOnBrokenSegment(TraceVm traceVm, double procentOfSegmentUptoAccident, int leftNodeIndex)
        {
            var leftNode = _model.Data.Nodes.First(n => n.Id == traceVm.Nodes[leftNodeIndex]);
            var rightNode = _model.Data.Nodes.First(n => n.Id == traceVm.Nodes[leftNodeIndex + 1]);

            var latBreak = leftNode.Position.Lat + (rightNode.Position.Lat - leftNode.Position.Lat) * procentOfSegmentUptoAccident;
            var lngBreak = leftNode.Position.Lng + (rightNode.Position.Lng - leftNode.Position.Lng) * procentOfSegmentUptoAccident;
            return new PointLatLng(latBreak, lngBreak);
        }

        private void GetCableReserves(AccidentAsNewEvent accident, Guid traceId, out double leftReserveM, out double rightReserveM)
        {
            var equipmentsWithoutPoints = _readModel.GetTraceEquipmentsExcludingAdjustmentPoints(traceId).ToList();
            leftReserveM = GetCableReserve(equipmentsWithoutPoints, accident.LeftLandmarkIndex, true);
            rightReserveM = GetCableReserve(equipmentsWithoutPoints, accident.LeftLandmarkIndex, false);
        }

        private double GetCableReserve(List<Equipment> equipmentsWithoutPoints, int landmarkIndex, bool isLeftLandmark)
        {
            var equipment =  equipmentsWithoutPoints[landmarkIndex];
            if (equipment.Type == EquipmentType.CableReserve) return (double)equipment.CableReserveLeft / 2;
            return isLeftLandmark ? equipment.CableReserveRight : equipment.CableReserveLeft;
        }

        private List<double> GetGpsDistancesOfSegmentsBetweenLandmarks(AccidentAsNewEvent accident, TraceVm traceVm, out NodeVm leftNodeVm, out NodeVm rightNodeVm)
        {
            var withoutPoints = _readModel.GetTraceNodesExcludingAdjustmentPoints(accident.TraceId).ToList();
            leftNodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == withoutPoints[accident.LeftLandmarkIndex]);
            rightNodeVm = _model.Data.Nodes.FirstOrDefault(n => n.Id == withoutPoints[accident.RightLandmarkIndex]);

            if (leftNodeVm == null)
            {
                _logFile.AppendLine($@"NodeVm {withoutPoints[accident.LeftLandmarkIndex].First6()} not found");
                return null;
            }
            if (rightNodeVm == null)
            {
                _logFile.AppendLine($@"NodeVm {withoutPoints[accident.RightLandmarkIndex].First6()} not found");
                return null;
            }

            var indexOfLeft = traceVm.Nodes.IndexOf(leftNodeVm.Id);
            var indexOfRight = traceVm.Nodes.IndexOf(rightNodeVm.Id);
            var result = new List<double>();
            var fromNodeVm = leftNodeVm;
            for (int i = indexOfLeft + 1; i < indexOfRight; i++)
            {
                var toNodeVm = _model.Data.Nodes.First(n => n.Id == traceVm.Nodes[i]);
                result.Add(GpsCalculator.GetDistanceBetweenPointLatLng(fromNodeVm.Position, toNodeVm.Position));
                fromNodeVm = toNodeVm;
            }
            result.Add(GpsCalculator.GetDistanceBetweenPointLatLng(fromNodeVm.Position, rightNodeVm.Position));
            return result;
        }
    }
}
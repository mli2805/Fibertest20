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

        public AccidentPlaceLocator(GraphReadModel model, IMyLog logFile)
        {
            _model = model;
            _logFile = logFile;
        }

        public PointLatLng? GetAccidentGps(AccidentOnTrace accident)
        {
            if (accident is AccidentAsNewEvent accidentAsNewEvent)
                return GetAccidentGps(accidentAsNewEvent);

            if (accident is AccidentInOldEvent accidentInOldEvent)
            {
                var withoutPoints = _model.GetTraceNodesExcludingAdjustmentPoints(accident.TraceId);
                var nodeVm = _model.Nodes.FirstOrDefault(n => n.Id == withoutPoints[accidentInOldEvent.BrokenLandmarkIndex]);
                return nodeVm?.Position;
            }

            return null;
        }

        private PointLatLng? GetAccidentGps(AccidentAsNewEvent accident)
        {
            var distances = GetGpsDistancesOfSegmentsBetweenLandmarks(accident, out NodeVm leftNodeVm, out NodeVm rightNodeVm);
            var distanceBetweenTwoNodesOnGraphM = distances.Sum();
            GetCableReserves(accident, out double leftReserveM, out double rightReserveM);

            var opticalLengthM = (accident.RightNodeKm - accident.LeftNodeKm) * 1000;
            var coeff = opticalLengthM / (distanceBetweenTwoNodesOnGraphM + leftReserveM + rightReserveM);


            var distanceToAccidentOnGraphM = (accident.AccidentDistanceKm - accident.LeftNodeKm) * 1000 / coeff;

            if (distanceToAccidentOnGraphM <= leftReserveM)
                return leftNodeVm.Position;

            if (distanceToAccidentOnGraphM > leftReserveM + distanceBetweenTwoNodesOnGraphM)
                return rightNodeVm.Position;



            var partOfSegmentUptoAccidentPlace
                = (distanceToAccidentOnGraphM - leftReserveM) / gpsDistanceM;

            var latBreak = leftNodeVm.Position.Lat + (rightNodeVm.Position.Lat - leftNodeVm.Position.Lat) * partOfSegmentUptoAccidentPlace;
            var lngBreak = leftNodeVm.Position.Lng + (rightNodeVm.Position.Lng - leftNodeVm.Position.Lng) * partOfSegmentUptoAccidentPlace;
            return new PointLatLng(latBreak, lngBreak);
        }

        private void GetCableReserves(AccidentAsNewEvent accident, out double leftReserveM, out double rightReserveM)
        {
            var equipmentsWithoutPoints = _model.GetTraceEquipmentsExcludingAdjustmentPoints(accident.TraceId);
            leftReserveM = GetCableReserve(equipmentsWithoutPoints, accident.LeftLandmarkIndex, true);
            rightReserveM = GetCableReserve(equipmentsWithoutPoints, accident.LeftLandmarkIndex, false);
        }
        private double GetCableReserve(List<Guid> equipmentsWithoutPoints, int landmarkIndex, bool isLeftLandmark)
        {
            var equipmentVm = _model.Equipments.FirstOrDefault(e => e.Id == equipmentsWithoutPoints[landmarkIndex]);
            if (equipmentVm == null)
            {
                _logFile.AppendLine($@"Equipment {equipmentsWithoutPoints[landmarkIndex].First6()} not found");
                return 0;
            }

            if (equipmentVm.Type == EquipmentType.CableReserve) return (double)equipmentVm.CableReserveLeft / 2;
            return isLeftLandmark ? equipmentVm.CableReserveRight : equipmentVm.CableReserveLeft;
        }

        private List<double> GetGpsDistancesOfSegmentsBetweenLandmarks(AccidentAsNewEvent accident, out NodeVm leftNodeVm, out NodeVm rightNodeVm)
        {
            var withoutPoints = _model.GetTraceNodesExcludingAdjustmentPoints(accident.TraceId);
            leftNodeVm = _model.Nodes.FirstOrDefault(n => n.Id == withoutPoints[accident.LeftLandmarkIndex]);
            rightNodeVm = _model.Nodes.FirstOrDefault(n => n.Id == withoutPoints[accident.RightLandmarkIndex]);

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

            var traceVm = _model.Traces.First(t => t.Id == accident.TraceId);
            var indexOfLeft = traceVm.Nodes.IndexOf(leftNodeVm.Id);
            var indexOfRight = traceVm.Nodes.IndexOf(rightNodeVm.Id);
            var result = new List<double>();
            var fromNodeVm = leftNodeVm;
            for (int i = indexOfLeft + 1; i < indexOfRight; i++)
            {
                var toNodeVm = _model.Nodes.First(n => n.Id == traceVm.Nodes[i]);
                result.Add(GpsCalculator.GetDistanceBetweenPointLatLng(fromNodeVm.Position, toNodeVm.Position));
                fromNodeVm = toNodeVm;
            }
            result.Add(GpsCalculator.GetDistanceBetweenPointLatLng(fromNodeVm.Position, rightNodeVm.Position));
            return result;
        }
    }
}
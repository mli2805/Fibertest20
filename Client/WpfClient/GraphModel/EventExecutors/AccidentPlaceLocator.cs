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

        public PointLatLng? GetAccidentGps(AccidentOnTrace accident, TraceVm traceVm)
        {
            if (accident is AccidentAsNewEvent accidentAsNewEvent)
                return GetAccidentGps(accidentAsNewEvent, traceVm);

            if (accident is AccidentInOldEvent accidentInOldEvent)
            {
                var nodeVm = _model.Nodes.FirstOrDefault(n => n.Id == traceVm.Nodes[accidentInOldEvent.BrokenLandmarkIndex]);
                return nodeVm?.Position;
            }

            return null;
        }

        private PointLatLng? GetAccidentGps(AccidentAsNewEvent accident, TraceVm traceVm)
        {
            var fiberVm = _model.GetFiberByLandmarkIndexes(traceVm, accident.LeftLandmarkIndex, accident.RightLandmarkIndex);
            if (fiberVm == null) return null;
            var gpsDistanceM = GetGpsDistanceBetweenNeighbours(accident, traceVm, out NodeVm leftNodeVm, out NodeVm rightNodeVm);
            if (leftNodeVm == null || rightNodeVm == null) return null;

            double distanceBetweenTwoNodesOnGraphM = fiberVm.UserInputedLength != 0
                ? fiberVm.UserInputedLength
                : gpsDistanceM;

            var leftReserveM = GetLeftCableReserves(accident, traceVm);
            var rightReserveM = GetRightCableReserves(accident, traceVm);
            var fullDistanceBetweenTwoEquipmentsOnGraphM = distanceBetweenTwoNodesOnGraphM + leftReserveM + rightReserveM;
            var opticalLengthM = (accident.RightNodeKm - accident.LeftNodeKm) * 1000;
            var coeff = opticalLengthM / fullDistanceBetweenTwoEquipmentsOnGraphM;

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

        private double GetLeftCableReserves(AccidentAsNewEvent accident, TraceVm traceVm)
        {
            var leftEquipmentVm = _model.Equipments.FirstOrDefault(e => e.Id == traceVm.Equipments[accident.LeftLandmarkIndex]);
            if (leftEquipmentVm == null)
            {
                _logFile.AppendLine($@"Equipment {traceVm.Equipments[accident.LeftLandmarkIndex].First6()} not found");
                return 0;
            }
            return leftEquipmentVm.Type == EquipmentType.CableReserve
                ? leftEquipmentVm.CableReserveLeft / 2
                : leftEquipmentVm.CableReserveRight;
        }

        private double GetRightCableReserves(AccidentAsNewEvent accident, TraceVm traceVm)
        {
            var rightEquipmentVm = _model.Equipments.FirstOrDefault(e => e.Id == traceVm.Equipments[accident.RightLandmarkIndex]);
            if (rightEquipmentVm == null)
            {
                _logFile.AppendLine($@"Equipment {traceVm.Equipments[accident.RightLandmarkIndex].First6()} not found");
                return 0;
            }
            return rightEquipmentVm.Type == EquipmentType.CableReserve
                ? rightEquipmentVm.CableReserveLeft / 2
                : rightEquipmentVm.CableReserveLeft;
        }

        private double GetGpsDistanceBetweenNeighbours(AccidentAsNewEvent accident, TraceVm traceVm, out NodeVm leftNodeVm, out NodeVm rightNodeVm)
        {
            rightNodeVm = null;

            leftNodeVm = _model.Nodes.FirstOrDefault(n => n.Id == traceVm.Nodes[accident.LeftLandmarkIndex]);
            if (leftNodeVm == null)
            {
                _logFile.AppendLine($@"NodeVm {traceVm.Nodes[accident.LeftLandmarkIndex].First6()} not found");
                return 0;
            }

            rightNodeVm = _model.Nodes.FirstOrDefault(n => n.Id == traceVm.Nodes[accident.RightLandmarkIndex]);
            if (rightNodeVm == null)
            {
                _logFile.AppendLine($@"NodeVm {traceVm.Nodes[accident.RightLandmarkIndex].First6()} not found");
                return 0;
            }

            return GpsCalculator.GetDistanceBetweenPointLatLng(leftNodeVm.Position, rightNodeVm.Position);
        }
    }
}
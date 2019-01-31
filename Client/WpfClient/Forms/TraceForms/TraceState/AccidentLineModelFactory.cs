using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class AccidentLineModelFactory
    {
        private readonly Model _readModel;
        private readonly AccidentPlaceLocator _accidentPlaceLocator;
        private readonly CurrentGis _currentGis;

        public AccidentLineModelFactory(Model readModel,
            AccidentPlaceLocator accidentPlaceLocator, CurrentGis currentGis)
        {
            _readModel = readModel;
            _accidentPlaceLocator = accidentPlaceLocator;
            _currentGis = currentGis;
        }

        public AccidentLineModel Create(AccidentOnTrace accidentOnTrace, int number)
        {
            switch (accidentOnTrace)
            {
                case AccidentAsNewEvent accidentAsNewEvent:
                    return CreateBetweenNodes(accidentAsNewEvent, number);

                case AccidentInOldEvent accidentInOldEvent:
                    if (accidentInOldEvent.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                        return CreateBadSegment(accidentInOldEvent, number);

                    return CreateInNode(accidentInOldEvent, number);

                default: return null;
            }
        }

        private AccidentLineModel CreateInNode(AccidentInOldEvent accidentInOldEvent, int number)
        {
            var nodesExcludingAdjustmentPoints =
                _readModel.GetTraceNodesExcludingAdjustmentPoints(accidentInOldEvent.TraceId).ToList();
            var nodeId = nodesExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex];
            var isLastNode = accidentInOldEvent.BrokenLandmarkIndex == nodesExcludingAdjustmentPoints.Count - 1;
            var node = _readModel.Nodes.FirstOrDefault(n => n.NodeId == nodeId);
            var equipmentExcludingAdjustmentPoints =
                _readModel.GetTraceEquipmentsExcludingAdjustmentPoints(accidentInOldEvent.TraceId).ToArray();
            var equipment = equipmentExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex];
            var modelTopCenter = node?.Title;
            if (equipment.Type != EquipmentType.EmptyNode && !string.IsNullOrEmpty(equipment.Title))
                modelTopCenter = modelTopCenter + @" / " + equipment.Title;

            var model = new AccidentLineModel
            {
                Caption = $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} ({
                    accidentInOldEvent.OpticalTypeOfAccident.ToLetter()}) {Resources.SID_in_the_node}:",
                TopCenter = modelTopCenter,
                TopLeft = $@"{accidentInOldEvent.AccidentDistanceKm:0.000} {Resources.SID_km}",
                Bottom2 = _currentGis.IsGisOn
                    ? node?.Position.ToDetailedString(_currentGis.GpsInputMode)
                    : "",
                Scheme = accidentInOldEvent.AccidentSeriousness == FiberState.FiberBreak
                    ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/FiberBrokenInNode.png")
                    : isLastNode
                        ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentInLastNode.png")
                        : new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentInNode.png"),
                Position = node?.Position
            };

            return model;
        }

        private AccidentLineModel CreateBetweenNodes(AccidentAsNewEvent accidentAsNewEvent, int number)
        {
            var nodesExcludingAdjustmentPoints =
                _readModel.GetTraceNodesExcludingAdjustmentPoints(accidentAsNewEvent.TraceId).ToList();
            var equipmentExcludingAdjustmentPoints =
                _readModel.GetTraceEquipmentsExcludingAdjustmentPoints(accidentAsNewEvent.TraceId).ToArray();

            var leftNodeId = nodesExcludingAdjustmentPoints[accidentAsNewEvent.LeftLandmarkIndex];
            var leftNode = _readModel.Nodes.FirstOrDefault(n => n.NodeId == leftNodeId);
            var leftEquipment = equipmentExcludingAdjustmentPoints[accidentAsNewEvent.LeftLandmarkIndex];
            var leftNodeTitle = leftNode?.Title;
            if (leftEquipment.Type != EquipmentType.EmptyNode && !string.IsNullOrEmpty(leftEquipment.Title))
                leftNodeTitle = leftNodeTitle + @" / " + leftEquipment.Title;
         
            var rightNodeId = nodesExcludingAdjustmentPoints[accidentAsNewEvent.RightLandmarkIndex];
            var rightNode = _readModel.Nodes.FirstOrDefault(n => n.NodeId == rightNodeId);
            var rightEquipment = equipmentExcludingAdjustmentPoints[accidentAsNewEvent.RightLandmarkIndex];
            var rightNodeTitle = rightNode?.Title;
            if (rightEquipment.Type != EquipmentType.EmptyNode && !string.IsNullOrEmpty(rightEquipment.Title))
                rightNodeTitle = rightNodeTitle + @" / " + rightEquipment.Title;
        
            var accidentGps = _accidentPlaceLocator.GetAccidentGps(accidentAsNewEvent);

            var model = new AccidentLineModel
            {
                Caption = $@"{number}. {accidentAsNewEvent.AccidentSeriousness.ToLocalizedString()} ({
                    accidentAsNewEvent.OpticalTypeOfAccident.ToLetter() }) {Resources.SID_between_nodes}:",
                TopLeft = leftNodeTitle,
                TopCenter = $@"{accidentAsNewEvent.AccidentDistanceKm:0.000} {Resources.SID_km}",
                TopRight = rightNodeTitle,
                Bottom1 = $@"{accidentAsNewEvent.AccidentDistanceKm - accidentAsNewEvent.LeftNodeKm:0.000} {Resources.SID_km}",
                Bottom2 = _currentGis.IsGisOn ? accidentGps?.ToDetailedString(_currentGis.GpsInputMode) : "",
                Bottom3 = $@"{accidentAsNewEvent.RightNodeKm - accidentAsNewEvent.AccidentDistanceKm:0.000} {Resources.SID_km}",
                Scheme = accidentAsNewEvent.AccidentSeriousness == FiberState.FiberBreak
                    ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/FiberBrokenBetweenNodes.png")
                    : new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentBetweenNodes.png"),
                Position = accidentGps
            };

            return model;
        }

        private AccidentLineModel CreateBadSegment(AccidentInOldEvent accidentInOldEvent, int number)
        {
            var nodesExcludingAdjustmentPoints =
                _readModel.GetTraceNodesExcludingAdjustmentPoints(accidentInOldEvent.TraceId).ToList();
            var equipmentExcludingAdjustmentPoints =
                _readModel.GetTraceEquipmentsExcludingAdjustmentPoints(accidentInOldEvent.TraceId).ToArray();

            var leftNodeId = nodesExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex - 1];
            var leftNode = _readModel.Nodes.FirstOrDefault(n => n.NodeId == leftNodeId);
            var leftEquipment = equipmentExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex - 1];
            var leftNodeTitle = leftNode?.Title;
            if (leftEquipment.Type != EquipmentType.EmptyNode && !string.IsNullOrEmpty(leftEquipment.Title))
                leftNodeTitle = leftNodeTitle + @" / " + leftEquipment.Title;
         
            var rightNodeId = nodesExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex];
            var rightNode = _readModel.Nodes.FirstOrDefault(n => n.NodeId == rightNodeId);
            var rightEquipment = equipmentExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex];
            var rightNodeTitle = rightNode?.Title;
            if (rightEquipment.Type != EquipmentType.EmptyNode && !string.IsNullOrEmpty(rightEquipment.Title))
                rightNodeTitle = rightNodeTitle + @" / " + rightEquipment.Title;

            var model = new AccidentLineModel
            {
                Caption = $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} (C) {
                    Resources.SID_between_nodes }: ",
                TopLeft = leftNodeTitle,
                TopRight = rightNodeTitle,
                Bottom0 = $@"{accidentInOldEvent.PreviousLandmarkDistanceKm:0.000} {Resources.SID_km}",
                Bottom4 = $@"{accidentInOldEvent.AccidentDistanceKm:0.000} {Resources.SID_km}",
                Scheme = new Uri(@"pack://application:,,,/Resources/AccidentSchemes/BadSegment.png"),
                Position = leftNode?.Position
            };

            return model;
        }

    }
}
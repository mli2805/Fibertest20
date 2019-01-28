using System;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class AccidentLineModelFactory
    {
        private readonly GraphReadModel _graphReadModel;
        private readonly Model _readModel;
        private readonly AccidentPlaceLocator _accidentPlaceLocator;
        private readonly CurrentGis _currentGis;

        public AccidentLineModelFactory(GraphReadModel graphReadModel, Model readModel,
            AccidentPlaceLocator accidentPlaceLocator, CurrentGis currentGis)
        {
            _graphReadModel = graphReadModel;
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
            var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            var nodeTitle = nodeVm?.Title;
            var nodeCoors = nodeVm?.Position.ToDetailedString(_currentGis.GpsInputMode);

            var model = new AccidentLineModel();
            model.Caption =
                $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} ({
                        accidentInOldEvent.OpticalTypeOfAccident.ToLetter()
                    }) {Resources.SID_in_the_node}:";

            model.TopCenter = nodeTitle;
            model.TopLeft = $@"{accidentInOldEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";
            model.Bottom2 = _currentGis.IsGisOn ? nodeCoors : "";

            model.Scheme = accidentInOldEvent.AccidentSeriousness == FiberState.FiberBreak
                ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/FiberBrokenInNode.png")
                : isLastNode
                    ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentInLastNode.png")
                    : new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentInNode.png");
            model.Position = nodeVm?.Position;
            return model;
        }

        private AccidentLineModel CreateBetweenNodes(AccidentAsNewEvent accidentAsNewEvent, int number)
        {
            var nodesExcludingAdjustmentPoints =
                _readModel.GetTraceNodesExcludingAdjustmentPoints(accidentAsNewEvent.TraceId).ToList();
            var leftNodeId = nodesExcludingAdjustmentPoints[accidentAsNewEvent.LeftLandmarkIndex];
            var leftNodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == leftNodeId);
            var leftNodeTitle = leftNodeVm?.Title;

            var rightNodeId = nodesExcludingAdjustmentPoints[accidentAsNewEvent.RightLandmarkIndex];
            var rightNodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == rightNodeId);
            var rightNodeTitle = rightNodeVm?.Title;

            var accidentGps = _accidentPlaceLocator.GetAccidentGps(accidentAsNewEvent);

            var model = new AccidentLineModel();
            model.Caption =
                $@"{number}. {accidentAsNewEvent.AccidentSeriousness.ToLocalizedString()} ({
                        accidentAsNewEvent.OpticalTypeOfAccident.ToLetter()
                    }) {Resources.SID_between_nodes}:";

            model.TopLeft = leftNodeTitle;
            model.TopCenter = $@"{accidentAsNewEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";
            model.TopRight = rightNodeTitle;

            model.Bottom1 = $@"{accidentAsNewEvent.AccidentDistanceKm - accidentAsNewEvent.LeftNodeKm:0.000} {Resources.SID_km}";
            model.Bottom2 = _currentGis.IsGisOn ? accidentGps?.ToDetailedString(_currentGis.GpsInputMode) : "";
            model.Bottom3 = $@"{accidentAsNewEvent.RightNodeKm - accidentAsNewEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";

            model.Scheme = accidentAsNewEvent.AccidentSeriousness == FiberState.FiberBreak
                            ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/FiberBrokenBetweenNodes.png")
                            : new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentBetweenNodes.png");
            model.Position = accidentGps;
            return model;
        }

        private AccidentLineModel CreateBadSegment(AccidentInOldEvent accidentInOldEvent, int number)
        {
            var nodesExcludingAdjustmentPoints =
                _readModel.GetTraceNodesExcludingAdjustmentPoints(accidentInOldEvent.TraceId).ToList();
            var leftNodeId = nodesExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex - 1];
            var leftNodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == leftNodeId);
            var leftNodeTitle = leftNodeVm?.Title;

            var rightNodeId = nodesExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex];
            var rightNodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == rightNodeId);
            var rightNodeTitle = rightNodeVm?.Title;

            var model = new AccidentLineModel();
            model.Caption = $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} (C) {Resources.SID_between_nodes}:";

            model.TopLeft = leftNodeTitle;
            model.TopRight = rightNodeTitle;

            model.Bottom0 = $@"{accidentInOldEvent.PreviousLandmarkDistanceKm:0.000} {Resources.SID_km}";
            model.Bottom4 = $@"{accidentInOldEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";

            model.Scheme = new Uri(@"pack://application:,,,/Resources/AccidentSchemes/BadSegment.png");
            model.Position = leftNodeVm?.Position;
            return model;
        }

    }
}
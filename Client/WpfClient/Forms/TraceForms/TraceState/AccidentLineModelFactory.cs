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
        private readonly AccidentPlaceLocator _accidentPlaceLocator;
        private readonly CurrentGpsInputMode _currentGpsInputMode;

        public AccidentLineModelFactory(GraphReadModel graphReadModel, AccidentPlaceLocator accidentPlaceLocator, CurrentGpsInputMode currentGpsInputMode)
        {
            _graphReadModel = graphReadModel;
            _accidentPlaceLocator = accidentPlaceLocator;
            _currentGpsInputMode = currentGpsInputMode;
        }

        public AccidentLineModel Create(AccidentOnTrace accidentOnTrace, int number)
        {
            var traceVm = _graphReadModel.Data.Traces.FirstOrDefault(t => t.Id == accidentOnTrace.TraceId);
            switch (accidentOnTrace)
            {
                case AccidentAsNewEvent accidentAsNewEvent:
                    return CreateBetweenNodes(accidentAsNewEvent, number);

                case AccidentInOldEvent accidentInOldEvent:
                    if (accidentInOldEvent.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                        return CreateBadSegment(accidentInOldEvent, traceVm, number);

                    return CreateInNode(accidentInOldEvent, number);

                default: return null;
            }
        }

        private AccidentLineModel CreateInNode(AccidentInOldEvent accidentInOldEvent, int number)
        {
            var nodesExcludingAdjustmentPoints =
                _graphReadModel.GetTraceNodesExcludingAdjustmentPoints(accidentInOldEvent.TraceId);
            var nodeId = nodesExcludingAdjustmentPoints[accidentInOldEvent.BrokenLandmarkIndex];
            var isLastNode = accidentInOldEvent.BrokenLandmarkIndex == nodesExcludingAdjustmentPoints.Count - 1;
            var nodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == nodeId);
            var nodeTitle = nodeVm?.Title;
            var nodeCoors = nodeVm?.Position.ToDetailedString(_currentGpsInputMode.Mode);

            var model = new AccidentLineModel();
            model.Caption =
                $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} ({
                        accidentInOldEvent.OpticalTypeOfAccident.ToLetter()
                    }) {Resources.SID_in_the_node}:";

            model.TopCenter = nodeTitle;
            model.TopLeft = $@"{accidentInOldEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";
            model.Bottom2 = nodeCoors;

            model.Scheme = accidentInOldEvent.AccidentSeriousness == FiberState.FiberBreak
                ? new Uri("pack://application:,,,/Resources/AccidentSchemes/FiberBrokenInNode.png")
                : isLastNode
                    ? new Uri("pack://application:,,,/Resources/AccidentSchemes/AccidentInLastNode.png")
                    : new Uri("pack://application:,,,/Resources/AccidentSchemes/AccidentInNode.png");
            return model;
        }

        private AccidentLineModel CreateBetweenNodes(AccidentAsNewEvent accidentAsNewEvent, int number)
        {
            var nodesExcludingAdjustmentPoints =
                _graphReadModel.GetTraceNodesExcludingAdjustmentPoints(accidentAsNewEvent.TraceId);
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
            model.Bottom2 = accidentGps?.ToDetailedString(_currentGpsInputMode.Mode);
            model.Bottom3 = $@"{accidentAsNewEvent.RightNodeKm - accidentAsNewEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";

            model.Scheme = accidentAsNewEvent.AccidentSeriousness == FiberState.FiberBreak
                            ? new Uri("pack://application:,,,/Resources/AccidentSchemes/FiberBrokenBetweenNodes.png")
                            : new Uri("pack://application:,,,/Resources/AccidentSchemes/AccidentBetweenNodes.png");

            return model;
        }

        private AccidentLineModel CreateBadSegment(AccidentInOldEvent accidentInOldEvent, TraceVm trace, int number)
        {
            var leftNodeId = trace.Nodes[accidentInOldEvent.BrokenLandmarkIndex - 1];
            var leftNodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == leftNodeId);
            var leftNodeTitle = leftNodeVm?.Title;

            var rightNodeId = trace.Nodes[accidentInOldEvent.BrokenLandmarkIndex];
            var rightNodeVm = _graphReadModel.Data.Nodes.FirstOrDefault(n => n.Id == rightNodeId);
            var rightNodeTitle = rightNodeVm?.Title;

            var model = new AccidentLineModel();
            model.Caption = $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} (C) {Resources.SID_between_nodes}:";

            model.TopLeft = leftNodeTitle;
            model.TopRight = rightNodeTitle;

            model.Bottom0 = $@"{accidentInOldEvent.PreviousLandmarkDistanceKm:0.000} {Resources.SID_km}";
            model.Bottom4 = $@"{accidentInOldEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";

            model.Scheme = new Uri("pack://application:,,,/Resources/AccidentSchemes/BadSegment.png");
            return model;
        }

    }
}
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

        public AccidentLineModelFactory(GraphReadModel graphReadModel, AccidentPlaceLocator accidentPlaceLocator)
        {
            _graphReadModel = graphReadModel;
            _accidentPlaceLocator = accidentPlaceLocator;
        }

        public AccidentLineModel Create(AccidentOnTrace accidentOnTrace, Trace trace, int number)
        {
            switch (accidentOnTrace)
            {
                case AccidentAsNewEvent accidentAsNewEvent:
                    return CreateBetweenNodes(accidentAsNewEvent, trace, number);

                case AccidentInOldEvent accidentInOldEvent:
                    if (accidentOnTrace.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff)
                        return CreateBadSegment(accidentInOldEvent, trace, number);

                    return CreateInNode(accidentInOldEvent, trace, number);

                default: return null;
            }
        }

        private AccidentLineModel CreateInNode(AccidentInOldEvent accidentInOldEvent, Trace trace, int number)
        {
            var isLastNode = accidentInOldEvent.BrokenLandmarkIndex == trace.Nodes.Count - 1;
            var nodeId = trace.Nodes[accidentInOldEvent.BrokenLandmarkIndex];
            var nodeVm = _graphReadModel.Nodes.FirstOrDefault(n => n.Id == nodeId);
            var nodeTitle = nodeVm?.Title;
            var nodeCoors = nodeVm?.Position.ToDetailedString(GpsInputMode.Degrees);

            var model = new AccidentLineModel();
            model.Caption =
                $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} ({
                        accidentInOldEvent.OpticalTypeOfAccident.ToLetter()
                    }) {Resources.SID_in_the_node}:";

            model.TopCenter = nodeTitle;
            model.TopLeft = $@"{accidentInOldEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";
            model.Bottom2 = nodeCoors;

            model.Scheme = accidentInOldEvent.AccidentSeriousness == FiberState.FiberBreak
                ? new Uri("pack://application:,,,/Resources/AccidentSchemes/AccidentInLastNode.png")
                : isLastNode
                    ? new Uri("pack://application:,,,/Resources/AccidentSchemes/AccidentInLastNode.png")
                    : new Uri("pack://application:,,,/Resources/AccidentSchemes/AccidentInNode.png");
            return model;
        }

        private AccidentLineModel CreateBetweenNodes(AccidentAsNewEvent accidentAsNewEvent, Trace trace, int number)
        {
            var leftNodeId = trace.Nodes[accidentAsNewEvent.LeftLandmarkIndex];
            var leftNodeVm = _graphReadModel.Nodes.FirstOrDefault(n => n.Id == leftNodeId);
            var leftNodeTitle = leftNodeVm?.Title;

            var rightNodeId = trace.Nodes[accidentAsNewEvent.RightLandmarkIndex];
            var rightNodeVm = _graphReadModel.Nodes.FirstOrDefault(n => n.Id == rightNodeId);
            var rightNodeTitle = rightNodeVm?.Title;

            var traceVm = _graphReadModel.Traces.First(t => t.Id == trace.Id);
            var accidentGps = _accidentPlaceLocator.GetAccidentGps(accidentAsNewEvent, traceVm);

            var model = new AccidentLineModel();
            model.Caption =
                $@"{number}. {accidentAsNewEvent.AccidentSeriousness.ToLocalizedString()} ({
                        accidentAsNewEvent.OpticalTypeOfAccident.ToLetter()
                    }) {Resources.SID_between_nodes}:";

            model.TopLeft = leftNodeTitle;
            model.TopCenter = $@"{accidentAsNewEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";
            model.TopRight = rightNodeTitle;

            model.Bottom1 = $@"{accidentAsNewEvent.AccidentDistanceKm - accidentAsNewEvent.LeftNodeKm:0.000} {Resources.SID_km}";
            model.Bottom2 = accidentGps?.ToDetailedString(GpsInputMode.Degrees);
            model.Bottom3 = $@"{accidentAsNewEvent.RightNodeKm - accidentAsNewEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";

            model.Scheme = accidentAsNewEvent.AccidentSeriousness == FiberState.FiberBreak
                            ? new Uri("pack://application:,,,/Resources/AccidentSchemes/FiberBrokenBetweenNodes.png")
                            : new Uri("pack://application:,,,/Resources/AccidentSchemes/AccidentBetweenNodes.png");

            return model;
        }

        private AccidentLineModel CreateBadSegment(AccidentInOldEvent accidentInOldEvent, Trace trace, int number)
        {
            var leftNodeId = trace.Nodes[accidentInOldEvent.BrokenLandmarkIndex - 1];
            var leftNodeVm = _graphReadModel.Nodes.FirstOrDefault(n => n.Id == leftNodeId);
            var leftNodeTitle = leftNodeVm?.Title;

            var rightNodeId = trace.Nodes[accidentInOldEvent.BrokenLandmarkIndex];
            var rightNodeVm = _graphReadModel.Nodes.FirstOrDefault(n => n.Id == rightNodeId);
            var rightNodeTitle = rightNodeVm?.Title;

            var model = new AccidentLineModel();
            model.Caption = $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} (C) {Resources.SID_between_nodes}:";

            model.TopLeft = leftNodeTitle;
            model.TopRight = rightNodeTitle;

            model.Bottom1 = $@"{accidentInOldEvent.PreviousLandmarkDistanceKm:0.000} {Resources.SID_km}";
            model.Bottom3 = $@"{accidentInOldEvent.AccidentDistanceKm:0.000} {Resources.SID_km}";

            model.Scheme = new Uri("pack://application:,,,/Resources/AccidentSchemes/BadSegment.png");
            return model;
        }

    }
}
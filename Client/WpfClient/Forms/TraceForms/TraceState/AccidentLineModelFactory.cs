using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class AccidentLineModelFactory
    {
        private readonly CurrentGis _currentGis;

        public AccidentLineModelFactory(CurrentGis currentGis)
        {
            _currentGis = currentGis;
        }

        public AccidentLineModel Create(AccidentOnTraceV2 accident, int number)
        {
            if (accident.IsAccidentInOldEvent)
            {
                return accident.OpticalTypeOfAccident == OpticalAccidentType.LossCoeff
                    ? CreateBadSegment(accident, number)
                    : CreateInNode(accident, number);
            }
            else
                return CreateBetweenNodes(accident, number);
        }

        private AccidentLineModel CreateInNode(AccidentOnTraceV2 accidentInOldEvent, int number)
        {
            var model = new AccidentLineModel
            {
                Caption =
                    $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} ({
                            accidentInOldEvent.OpticalTypeOfAccident.ToLetter()
                        }) {Resources.SID_in_the_node}:",
                TopCenter = accidentInOldEvent.AccidentTitle,
                TopLeft = $@"RTU <- {accidentInOldEvent.AccidentToRtuOpticalDistanceKm:0.000} {Resources.SID_km}",
                Bottom2 = _currentGis.IsGisOn
                    ? accidentInOldEvent.AccidentCoors.ToDetailedString(_currentGis.GpsInputMode)
                    : "",
                Scheme = accidentInOldEvent.AccidentSeriousness == FiberState.FiberBreak
                    ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/FiberBrokenInNode.png")
                    : accidentInOldEvent.IsAccidentInLastNode
                        ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentInLastNode.png")
                        : new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentInNode.png"),
                Position = accidentInOldEvent.AccidentCoors,
            };

            return model;
        }

        private AccidentLineModel CreateBetweenNodes(AccidentOnTraceV2 accidentAsNewEvent, int number)
        {
            var model = new AccidentLineModel
            {
                Caption =
                             $@"{number}. {accidentAsNewEvent.AccidentSeriousness.ToLocalizedString()} ({
                                     accidentAsNewEvent.OpticalTypeOfAccident.ToLetter()
                                 }) {Resources.SID_between_nodes}:",
                TopLeft = accidentAsNewEvent.Left.Title,
                TopCenter = $@"RTU <- {accidentAsNewEvent.AccidentToRtuOpticalDistanceKm:0.000} {Resources.SID_km}",
                TopRight = accidentAsNewEvent.Right.Title,
                Bottom1 = $@"{accidentAsNewEvent.AccidentToLeftOpticalDistanceKm:0.000} {Resources.SID_km}",
                Bottom2 = _currentGis.IsGisOn
                             ? accidentAsNewEvent.AccidentCoors.ToDetailedString(_currentGis.GpsInputMode)
                             : "",
                Bottom3 = $@"{accidentAsNewEvent.AccidentToRightOpticalDistanceKm:0.000} {Resources.SID_km}",
                Scheme = accidentAsNewEvent.AccidentSeriousness == FiberState.FiberBreak
                             ? new Uri(@"pack://application:,,,/Resources/AccidentSchemes/FiberBrokenBetweenNodes.png")
                             : new Uri(@"pack://application:,,,/Resources/AccidentSchemes/AccidentBetweenNodes.png"),
                Position = accidentAsNewEvent.AccidentCoors
            };

            return model;
        }

        private AccidentLineModel CreateBadSegment(AccidentOnTraceV2 accidentInOldEvent, int number)
        {
            var model = new AccidentLineModel
            {
                Caption =
                    $@"{number}. {accidentInOldEvent.AccidentSeriousness.ToLocalizedString()} (C) {
                            Resources.SID_between_nodes
                        }: ",
                TopLeft = accidentInOldEvent.Left.Title,
                TopRight = accidentInOldEvent.Right.Title,
                Bottom0 = $@"RTU <- {accidentInOldEvent.Left.ToRtuOpticalDistanceKm:0.000} {Resources.SID_km}",
                Bottom4 = $@"RTU <- {accidentInOldEvent.AccidentToRtuOpticalDistanceKm:0.000} {Resources.SID_km}",
                Scheme = new Uri(@"pack://application:,,,/Resources/AccidentSchemes/BadSegment.png"),
                Position = accidentInOldEvent.Left.Coors,
            };

            return model;
        }
    }
}
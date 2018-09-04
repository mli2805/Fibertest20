using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsExecutor
    {
        private readonly IMyLog _logFile;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;
        private readonly SystemState _systemState;

        public OpticalEventsExecutor(IMyLog logFile, OpticalEventsDoubleViewModel opticalEventsDoubleViewModel, SystemState systemState)
        {
            _logFile = logFile;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
            _systemState = systemState;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case MeasurementAdded evnt:
                    if (evnt.EventStatus > EventStatus.JustMeasurementNotAnEvent)
                        _opticalEventsDoubleViewModel.AddMeasurement(evnt);
                    break;
                case MeasurementUpdated evnt:
                    _opticalEventsDoubleViewModel.UpdateMeasurement(evnt); break;

                case TraceAttached evnt:
                    _opticalEventsDoubleViewModel.AttachTrace(evnt); break;
                case TraceDetached evnt:
                    _opticalEventsDoubleViewModel.DetachTrace(evnt); break;

                case TraceRemoved evnt: _opticalEventsDoubleViewModel.RemoveTrace(evnt); break;
                case TraceCleaned evnt: _opticalEventsDoubleViewModel.CleanTrace(evnt); break;

                case RtuUpdated evnt: _opticalEventsDoubleViewModel.UpdateRtu(evnt); break;
                case TraceUpdated evnt: _opticalEventsDoubleViewModel.UpdateTrace(evnt); break;

                case ResponsibilitiesChanged evnt: _opticalEventsDoubleViewModel.ChangeResponsibilities(evnt); break;
            }

            try
            {
                _systemState.HasActualOpticalProblems =
                    _opticalEventsDoubleViewModel.ActualOpticalEventsViewModel.Rows.Any();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                _logFile.AppendLine(exception.Message);
            }
         
        }
    }

    public class SystemState : PropertyChangedBase
    {
        private bool _hasActualOpticalProblems;
        private bool _hasActualNetworkProblems;
        private bool _hasActualBopNetworkProblems;
        private bool _hasAnyActualProblem;

        public bool HasActualOpticalProblems
        {
            get => _hasActualOpticalProblems;
            set
            {
                if (value == _hasActualOpticalProblems) return;
                _hasActualOpticalProblems = value;
                HasAnyActualProblem = _hasActualOpticalProblems || _hasActualNetworkProblems ||
                                      _hasActualBopNetworkProblems;
            }
        }

        public bool HasActualNetworkProblems
        {
            get => _hasActualNetworkProblems;
            set
            {
                if (value == _hasActualNetworkProblems) return;
                _hasActualNetworkProblems = value;
                HasAnyActualProblem = _hasActualOpticalProblems || _hasActualNetworkProblems ||
                                      _hasActualBopNetworkProblems;
            }
        }

        public bool HasActualBopNetworkProblems
        {
            get => _hasActualBopNetworkProblems;
            set
            {
                if (value == _hasActualBopNetworkProblems) return;
                _hasActualBopNetworkProblems = value;
                HasAnyActualProblem = _hasActualOpticalProblems || _hasActualNetworkProblems ||
                                      _hasActualBopNetworkProblems;
            }
        }

        public bool HasAnyActualProblem
        {
            get => _hasAnyActualProblem;
            set
            {
                if (value == _hasAnyActualProblem) return;
                _hasAnyActualProblem = value;
                NotifyOfPropertyChange();
            }
        }
    }
}
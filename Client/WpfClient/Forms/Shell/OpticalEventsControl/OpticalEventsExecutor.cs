using System;
using System.Linq;
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
                case AllTracesDetached evnt:
                    _opticalEventsDoubleViewModel.DetachAllTraces(evnt); break;

                case TraceRemoved evnt: _opticalEventsDoubleViewModel.RemoveTrace(evnt); break;
                case TraceCleaned evnt: _opticalEventsDoubleViewModel.CleanTrace(evnt); break;

                case RtuUpdated evnt: _opticalEventsDoubleViewModel.UpdateRtu(evnt); break;
                case TraceUpdated evnt: _opticalEventsDoubleViewModel.UpdateTrace(evnt); break;

                case ResponsibilitiesChanged evnt: _opticalEventsDoubleViewModel.ChangeResponsibilities(evnt); break;
                case EventsAndSorsRemoved evnt: _opticalEventsDoubleViewModel.
                    AllOpticalEventsViewModel.RemoveEventsAndSors(evnt); break;
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
}
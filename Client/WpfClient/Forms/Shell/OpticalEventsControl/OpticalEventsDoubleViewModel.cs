using System;
using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsDoubleViewModel : PropertyChangedBase
    {
        private readonly ReadModel _readModel;

        public OpticalEventsViewModel AllOpticalEventsViewModel { get; set; }
        public OpticalEventsViewModel ActualOpticalEventsViewModel { get; set; }

    

        public OpticalEventsDoubleViewModel(ReadModel readModel,
            OpticalEventsViewModel allOpticalEventsViewModel,
            OpticalEventsViewModel actualOpticalEventsViewModel)
        {
            _readModel = readModel;
            ActualOpticalEventsViewModel = actualOpticalEventsViewModel;
            ActualOpticalEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllOpticalEventsViewModel = allOpticalEventsViewModel;
            AllOpticalEventsViewModel.TableTitle = Resources.SID_All_optical_events;
        }

        public void Apply(Measurement measurement)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == measurement.TraceId);
            if (trace == null || !trace.IsAttached)
                return;

            ActualOpticalEventsViewModel.RemoveOldEventForTraceIfExists(measurement.TraceId);

            if (measurement.TraceState == FiberState.Ok)
                return;

            ActualOpticalEventsViewModel.AddEvent(measurement);
        }

        public void ApplyToTableAll(Measurement measurement)
        {
            AllOpticalEventsViewModel.AddEvent(measurement);
        }

        public void ApplyUsersChanges(UpdateMeasurementDto dto)
        {
            ActualOpticalEventsViewModel.ApplyUsersChanges(dto);
            AllOpticalEventsViewModel.ApplyUsersChanges(dto);
        }

        public void RemoveEventsOfTrace(Guid traceId)
        {
            ActualOpticalEventsViewModel.RemoveEventsOfTrace(traceId);
            AllOpticalEventsViewModel.RemoveEventsOfTrace(traceId);
        }
    }
}

﻿using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsDoubleViewModel : PropertyChangedBase
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

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

        public void AddMeasurement(MeasurementAdded measurementAdded)
        {
            var measurement = _mapper.Map<Measurement>(measurementAdded);

            var trace = _readModel.Traces.FirstOrDefault(t => t.Id == measurement.TraceId);
            if (trace == null || !trace.IsAttached)
                return;

            AllOpticalEventsViewModel.AddEvent(measurement);

            ActualOpticalEventsViewModel.RemovePreviousEventForTraceIfExists(measurement.TraceId);
            if (measurement.TraceState != FiberState.Ok)
                ActualOpticalEventsViewModel.AddEvent(measurement);
        }

        public void UpdateMeasurement(MeasurementUpdated evnt)
        {
            ActualOpticalEventsViewModel.UpdateEvent(evnt);
            AllOpticalEventsViewModel.UpdateEvent(evnt);
        }

        public void AttachTrace(TraceAttached evnt)
        {
            var lastMeasurementOnThisTrace = _readModel.Measurements.LastOrDefault(m => m.TraceId == evnt.TraceId);
            if (lastMeasurementOnThisTrace != null && lastMeasurementOnThisTrace.TraceState != FiberState.Ok)
                ActualOpticalEventsViewModel.AddEvent(lastMeasurementOnThisTrace);
        }

        public void DetachTrace(TraceDetached evnt)
        {
            ActualOpticalEventsViewModel.RemovePreviousEventForTraceIfExists(evnt.TraceId);
        }

        public void CleanTrace(TraceCleaned evnt)
        {
            ActualOpticalEventsViewModel.RemoveEventsOfTrace(evnt.Id);
            AllOpticalEventsViewModel.RemoveEventsOfTrace(evnt.Id);
        }

        public void RemoveTrace(TraceRemoved evnt)
        {
            ActualOpticalEventsViewModel.RemoveEventsOfTrace(evnt.Id);
            AllOpticalEventsViewModel.RemoveEventsOfTrace(evnt.Id);
        }
      
    }
}

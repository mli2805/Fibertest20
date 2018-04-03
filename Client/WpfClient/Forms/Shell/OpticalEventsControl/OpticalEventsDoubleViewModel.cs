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
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();

        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        public OpticalEventsViewModel AllOpticalEventsViewModel { get; set; }
        public OpticalEventsViewModel ActualOpticalEventsViewModel { get; set; }

        public OpticalEventsDoubleViewModel(Model readModel, CurrentUser currentUser,
            OpticalEventsViewModel allOpticalEventsViewModel,
            OpticalEventsViewModel actualOpticalEventsViewModel)
        {
            _readModel = readModel;
            _currentUser = currentUser;
            ActualOpticalEventsViewModel = actualOpticalEventsViewModel;
            ActualOpticalEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllOpticalEventsViewModel = allOpticalEventsViewModel;
            AllOpticalEventsViewModel.TableTitle = Resources.SID_All_optical_events;
        }

        public void AddMeasurement(MeasurementAdded measurementAdded)
        {
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == measurementAdded.TraceId);
            if (trace == null || !trace.ZoneIds.Contains(_currentUser.ZoneId) || !trace.IsAttached)
                return;

            var measurement = Mapper.Map<Measurement>(measurementAdded);
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
            var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == evnt.TraceId);
            if (trace == null || !trace.ZoneIds.Contains(_currentUser.ZoneId))
                return;

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
            ActualOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
            AllOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
        }

        public void RemoveTrace(TraceRemoved evnt)
        {
            ActualOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
            AllOpticalEventsViewModel.RemoveEventsOfTrace(evnt.TraceId);
        }

        public void UpdateRtu(RtuUpdated evnt)
        {
            ActualOpticalEventsViewModel.RefreshRowsWithUpdatedRtu(evnt.RtuId);
            AllOpticalEventsViewModel.RefreshRowsWithUpdatedRtu(evnt.RtuId);
        }

        public void UpdateTrace(TraceUpdated evnt)
        {
            ActualOpticalEventsViewModel.RefreshRowsWithUpdatedTrace(evnt.Id);
            AllOpticalEventsViewModel.RefreshRowsWithUpdatedTrace(evnt.Id);
        }

        public void ChangeResponsibilities(ResponsibilitiesChanged evnt)
        {

        }
      
    }
}

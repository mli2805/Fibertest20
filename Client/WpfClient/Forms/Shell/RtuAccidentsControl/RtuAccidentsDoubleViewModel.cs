using System;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class RtuAccidentsDoubleViewModel : PropertyChangedBase
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly SystemState _systemState;
        public RtuAccidentsViewModel AllRtuAccidentsViewModel { get; set; }
        public RtuAccidentsViewModel ActualRtuAccidentsViewModel { get; set; }

        public RtuAccidentsDoubleViewModel(Model readModel, CurrentUser currentUser, SystemState systemState,
            RtuAccidentsViewModel allRtuAccidentsViewModel, RtuAccidentsViewModel actualRtuAccidentsViewModel)
        {
            _readModel = readModel;
            _currentUser = currentUser;
            _systemState = systemState;
            AllRtuAccidentsViewModel = allRtuAccidentsViewModel;
            AllRtuAccidentsViewModel.TableTitle = Resources.SID_All_RTU_state_events;
            ActualRtuAccidentsViewModel = actualRtuAccidentsViewModel;
            ActualRtuAccidentsViewModel.TableTitle = Resources.SID_Current_accidents;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case RtuAccidentAdded evnt: AddRtuAccident(evnt); break;

                case RtuUpdated evnt: RtuUpdated(evnt.RtuId); break; // Title
                case RtuRemoved evnt: RtuRemoved(evnt.RtuId); break;
                case TraceUpdated evnt: TraceUpdated(evnt.Id); break; // Title
                case TraceCleaned evnt: TraceRemovedOrCleaned(evnt.TraceId); break;
                case TraceRemoved evnt: TraceRemovedOrCleaned(evnt.TraceId); break;
                case ResponsibilitiesChanged evnt: ChangeResponsibilities(evnt); break;
            }

            _systemState.HasActualBopNetworkProblems = ActualRtuAccidentsViewModel.Rows.Any();
        }

        private void AddRtuAccident(RtuAccidentAdded evnt)
        {
            var accident = Mapper.Map<RtuAccident>(evnt);
            ApplyOneAccident(accident);
        }

        public void RenderRtuAccidentsFromSnapshotWhileLoading()
        {
            foreach (var accident in _readModel.RtuAccidents)
            {
                ApplyOneAccident(accident);
            }
        }

        private void ApplyOneAccident(RtuAccident accident)
        {
            AllRtuAccidentsViewModel.AddAccident(accident);

            ActualRtuAccidentsViewModel.RemoveOldAccidentIfExists(accident);
            if (accident.ReturnCode == ReturnCode.MeasurementEndedNormally
                || accident.ReturnCode == ReturnCode.Ok
                || accident.ReturnCode == ReturnCode.RtuManagerServiceWorking
                || accident.ReturnCode == ReturnCode.MeasurementErrorCleared
                || accident.ReturnCode == ReturnCode.MeasurementErrorClearedByInit) // special code for RTU recovered ?
                return;

            ActualRtuAccidentsViewModel.AddAccident(accident);
        }

        private void RtuUpdated(Guid rtuId)
        {
            ActualRtuAccidentsViewModel.RefreshRowsWithUpdatedRtu(rtuId);
            AllRtuAccidentsViewModel.RefreshRowsWithUpdatedRtu(rtuId);
        }

        private void RtuRemoved(Guid rtuId)
        {
            ActualRtuAccidentsViewModel.RemoveAllEventsForRtu(rtuId);
            AllRtuAccidentsViewModel.RemoveAllEventsForRtu(rtuId);
        }
        private void TraceUpdated(Guid traceId)
        {
            ActualRtuAccidentsViewModel.RefreshRowsWithUpdatedTrace(traceId);
            AllRtuAccidentsViewModel.RefreshRowsWithUpdatedTrace(traceId);
        }

        private void TraceRemovedOrCleaned(Guid traceId)
        {
            ActualRtuAccidentsViewModel.RemoveAllEventsForTrace(traceId);
            AllRtuAccidentsViewModel.RemoveAllEventsForTrace(traceId);
        }

        private void ChangeResponsibilities(ResponsibilitiesChanged evnt)
        {
            foreach (var pair in evnt.ResponsibilitiesDictionary)
            {
                if (!pair.Value.Contains(_currentUser.ZoneId)) continue; // for current zone this trace doesn't change

                var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == pair.Key);
                if (rtu != null)
                {
                    RtuResponsibilityChanged(rtu);
                }
                else
                {
                    var trace = _readModel.Traces.FirstOrDefault(t => t.TraceId == pair.Key);
                    if (trace == null) continue; // impossible, but check

                    TraceResponsibilityChanged(trace);
                }
            }
        }

        private void RtuResponsibilityChanged(Rtu rtu)
        {
            if (rtu.ZoneIds.Contains(_currentUser.ZoneId)) // was NOT became YES
            {
                var rtuAccident = _readModel.RtuAccidents.LastOrDefault(n => n.RtuId == rtu.Id);
                if (rtuAccident != null && !rtuAccident.IsGoodAccident)
                    ActualRtuAccidentsViewModel.AddAccident(rtuAccident);

                foreach (var accident in _readModel.RtuAccidents.Where(n => n.RtuId == rtu.Id))
                {
                    AllRtuAccidentsViewModel.AddAccident(accident);
                }
            }
            else
            {
                RtuRemoved(rtu.Id);
            }
        }

        private void TraceResponsibilityChanged(Trace trace)
        {
            if (trace.ZoneIds.Contains(_currentUser.ZoneId)) // was NOT became YES
            {
                var rtuAccident = _readModel.RtuAccidents.LastOrDefault(n => n.TraceId == trace.TraceId);
                if (rtuAccident != null && !rtuAccident.IsGoodAccident)
                    ActualRtuAccidentsViewModel.AddAccident(rtuAccident);

                foreach (var accident in _readModel.RtuAccidents.Where(n => n.TraceId == trace.TraceId))
                {
                    AllRtuAccidentsViewModel.AddAccident(accident);
                }
            }
            else
            {
                RtuRemoved(trace.TraceId);
            }
        }
    }
}

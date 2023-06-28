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
            ActualRtuAccidentsViewModel = actualRtuAccidentsViewModel;
            ActualRtuAccidentsViewModel.TableTitle = Resources.SID_Current_accidents;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case RtuAccidentAdded evnt: AddRtuAccident(evnt); break;

                case RtuUpdated evnt: break; // Title
                case RtuRemoved evnt: break;
                case TraceUpdated evnt: break; // Title
                case TraceCleaned evnt: break;
                case TraceRemoved evnt: break;
                case ResponsibilitiesChanged evnt: break;
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
                || accident.ReturnCode == ReturnCode.Ok) // special code for RTU recovered ?
                return;

            ActualRtuAccidentsViewModel.AddAccident(accident);
        }

    }
}

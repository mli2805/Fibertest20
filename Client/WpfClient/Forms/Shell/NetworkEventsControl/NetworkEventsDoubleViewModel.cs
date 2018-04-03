using System;
using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class NetworkEventsDoubleViewModel : PropertyChangedBase
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;

        public NetworkEventsViewModel ActualNetworkEventsViewModel { get; set; }
        public NetworkEventsViewModel AllNetworkEventsViewModel { get; set; }

        public NetworkEventsDoubleViewModel(Model readModel, CurrentUser currentUser,
            NetworkEventsViewModel actualNetworkEventsViewModel, NetworkEventsViewModel allNetworkEventsViewModel)
        {
            ActualNetworkEventsViewModel = actualNetworkEventsViewModel;
            ActualNetworkEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllNetworkEventsViewModel = allNetworkEventsViewModel;
            AllNetworkEventsViewModel.TableTitle = Resources.SID_All_network_events;
            _readModel = readModel;
            _currentUser = currentUser;
        }

        public void Apply(object evnt)
        {
            switch (evnt)
            {
                case NetworkEventAdded e: NotifyUserRtuAvailabilityChanged(e); return;
                case RtuUpdated e: NotifyUserRtuUpdated(e.RtuId); return;
                case ResponsibilitiesChanged e: ChangeResponsibilities(e); return;
                default: return;
            }
        }

        private void NotifyUserRtuAvailabilityChanged(NetworkEventAdded networkEventAdded)
        {
            var networkEvent = Mapper.Map<NetworkEvent>(networkEventAdded);
            var rtu = _readModel.Rtus.FirstOrDefault(t => t.Id == networkEvent.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId))
                return;

            AllNetworkEventsViewModel.AddEvent(networkEvent);
            ActualNetworkEventsViewModel.RemoveOldEventForRtuIfExists(networkEvent.RtuId);

            if (networkEvent.IsAllRight)
                return;

            ActualNetworkEventsViewModel.AddEvent(networkEvent);
        }

        private void NotifyUserRtuUpdated(Guid rtuId)
        {
            ActualNetworkEventsViewModel.RefreshRowsWithUpdatedRtu(rtuId);
            AllNetworkEventsViewModel.RefreshRowsWithUpdatedRtu(rtuId);
        }

        private void ChangeResponsibilities(ResponsibilitiesChanged e)
        {

        }

    }
}

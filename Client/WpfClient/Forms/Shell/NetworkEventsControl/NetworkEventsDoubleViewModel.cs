using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Iit.Fibertest.Client
{
    public class NetworkEventsDoubleViewModel : PropertyChangedBase
    {
        private static readonly IMapper Mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly SystemState _systemState;

        public NetworkEventsViewModel ActualNetworkEventsViewModel { get; set; }
        public NetworkEventsViewModel AllNetworkEventsViewModel { get; set; }

        public NetworkEventsDoubleViewModel(Model readModel, CurrentUser currentUser, SystemState systemState,
            NetworkEventsViewModel actualNetworkEventsViewModel, NetworkEventsViewModel allNetworkEventsViewModel)
        {
            ActualNetworkEventsViewModel = actualNetworkEventsViewModel;
            ActualNetworkEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllNetworkEventsViewModel = allNetworkEventsViewModel;
            AllNetworkEventsViewModel.TableTitle = Resources.SID_All_network_events;
            _readModel = readModel;
            _currentUser = currentUser;
            _systemState = systemState;
        }

        public void Apply(object evnt)
        {
            switch (evnt)
            {
                case NetworkEventAdded e: RtuAvailabilityChanged(e); break;
                case RtuUpdated e: RtuUpdated(e.RtuId); break;
                case RtuRemoved e: RtuRemoved(e.RtuId); break;
                case ResponsibilitiesChanged e: ChangeResponsibilities(e); break;
                case EventsAndSorsRemoved e: AllNetworkEventsViewModel.RemoveEventsAndSors(e); break;
                default: return;
            }

            _systemState.HasActualNetworkProblems = ActualNetworkEventsViewModel.Rows.Any();
        }

        private void RtuAvailabilityChanged(NetworkEventAdded networkEventAdded)
        {
            var rtu = _readModel.Rtus.First(t => t.Id == networkEventAdded.RtuId);
            var networkEvent = Mapper.Map<NetworkEvent>(networkEventAdded);
            ApplyOneEvent(networkEvent, rtu.MainChannelState, rtu.ReserveChannelState);
        }

        public void RenderNetworkEvents()
        {
            var currentRtuState = new Dictionary<Guid, NetworkEventModel>();
            foreach (var networkEvent in _readModel.NetworkEvents)
            {
                // использует текущее состояние рту,
                // если вот так пачкой применять для всех ивентов будет использовать текущее состояние рту
                // поэтому делаем словарь

                currentRtuState.TryGetValue(networkEvent.RtuId, out var currentNetworkEventModel);
                var mainChannelState = currentNetworkEventModel?.MainChannel ?? RtuPartState.NotSetYet;
                var reserveChannelState = currentNetworkEventModel?.ReserveChannel ?? RtuPartState.NotSetYet;
                if (!networkEvent.IsReserveChannelSet) reserveChannelState = RtuPartState.NotSetYet;
                var result = ApplyOneEvent(networkEvent, mainChannelState, reserveChannelState);
                if (result.Item2 != null)
                {
                    currentRtuState[result.Item1] = result.Item2;
                }
            }
        }

        private (Guid, NetworkEventModel) ApplyOneEvent(NetworkEvent networkEvent, RtuPartState mainChannelState, RtuPartState reserveChannelState)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(t => t.Id == networkEvent.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId))
                return (Guid.Empty, null);
            networkEvent.IsRtuAvailable = rtu.MainChannelState == RtuPartState.Ok || rtu.ReserveChannelState == RtuPartState.Ok;

            var networkEventModel = AllNetworkEventsViewModel.AddEvent(networkEvent, mainChannelState, reserveChannelState);
            ActualNetworkEventsViewModel.RemoveOldEventForRtuIfExists(networkEvent.RtuId);
            var result = (rtu.Id, networkEventModel);

            if (!rtu.IsAllRight)
                ActualNetworkEventsViewModel.AddEvent(networkEvent, mainChannelState, reserveChannelState);

            return result;
        }

        private void RtuUpdated(Guid rtuId)
        {
            ActualNetworkEventsViewModel.RefreshRowsWithUpdatedRtu(rtuId);
            AllNetworkEventsViewModel.RefreshRowsWithUpdatedRtu(rtuId);
        }

        private void RtuRemoved(Guid rtuId)
        {
            ActualNetworkEventsViewModel.RemoveAllEventsForRtu(rtuId);
            AllNetworkEventsViewModel.RemoveAllEventsForRtu(rtuId);
        }

        private void ChangeResponsibilities(ResponsibilitiesChanged evnt)
        {
            foreach (var pair in evnt.ResponsibilitiesDictionary)
            {
                var rtu = _readModel.Rtus.FirstOrDefault(r => r.Id == pair.Key);
                if (rtu == null) continue; // not interested here in traces
                if (!pair.Value.Contains(_currentUser.ZoneId)) continue; // for current zone this RTU doesn't change

                if (rtu.ZoneIds.Contains(_currentUser.ZoneId)) // was NOT became YES
                {
                    var lastNetworkEvent = _readModel.NetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id);
                    if (lastNetworkEvent != null && !rtu.IsAllRight)
                        ActualNetworkEventsViewModel.AddEvent(lastNetworkEvent, rtu.MainChannelState, rtu.ReserveChannelState);

                    foreach (var networkEvent in _readModel.NetworkEvents.Where(n => n.RtuId == rtu.Id))
                    {
                        AllNetworkEventsViewModel.AddEvent(networkEvent, rtu.MainChannelState, rtu.ReserveChannelState);
                    }
                }
                else
                {
                    RtuRemoved(rtu.Id);
                }
            }
        }

    }
}

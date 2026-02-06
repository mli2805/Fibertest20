using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class NetworkEventsViewModel : PropertyChangedBase
    {
        private readonly Model _readModel;
        public string TableTitle { get; set; }

        public ObservableCollection<NetworkEventModel> Rows { get; set; } = new ObservableCollection<NetworkEventModel>();

        public NetworkEventsViewModel(Model readModel)
        {
            _readModel = readModel;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"EventTimestamp", ListSortDirection.Descending));
        }

        public NetworkEventModel AddEvent(NetworkEvent networkEvent, RtuPartState mainChannelState, RtuPartState reserveChannelState)
        {
            var rtu = _readModel.Rtus.First(r => r.Id == networkEvent.RtuId);
            var networkEventModel = new NetworkEventModel()
            {
                Ordinal = networkEvent.Ordinal,
                EventTimestamp = networkEvent.EventTimestamp,
                RtuId = networkEvent.RtuId,
                RtuTitle = rtu.Title,
                IsRtuAvailable = networkEvent.IsRtuAvailable,
                OnMainChannel = networkEvent.OnMainChannel,
                OnReserveChannel = networkEvent.OnReserveChannel,
                MainChannel = networkEvent.OnMainChannel == ChannelEvent.Nothing
                    ? mainChannelState
                    : networkEvent.OnMainChannel == ChannelEvent.Broken
                        ? RtuPartState.Broken
                        : RtuPartState.Ok,
                ReserveChannel = networkEvent.OnReserveChannel == ChannelEvent.Nothing
                    ? reserveChannelState
                    : networkEvent.OnReserveChannel == ChannelEvent.Broken
                        ? RtuPartState.Broken
                        : RtuPartState.Ok,
            };
            Rows.Add(networkEventModel);
            return networkEventModel;
        } 
        
        public void RemoveOldEventForRtuIfExists(Guid rtuId)
        {
            var oldEvent = Rows.FirstOrDefault(r => r.RtuId == rtuId);
            if (oldEvent != null)
                Rows.Remove(oldEvent);
        }

        public void RemoveAllEventsForRtu(Guid rtuId)
        {
            for (var i = Rows.Count - 1; i >= 0; i--)
            {
                if (Rows[i].RtuId == rtuId)
                    Rows.RemoveAt(i);
            }
        }

        public void RefreshRowsWithUpdatedRtu(Guid rtuId)
        {
            foreach (var networkEventModel in Rows.Where(m => m.RtuId == rtuId).ToList())
            {
                Rows.Remove(networkEventModel);
                networkEventModel.RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == rtuId)?.Title;
                Rows.Add(networkEventModel);
            }
        }

        public void RemoveEventsAndSors(EventsAndSorsRemoved evnt)
        {
            if (!evnt.IsNetworkEvents) return;

            foreach (var networkEventModel in Rows.ToList())
            {
                if (_readModel.NetworkEvents.All(n => n.EventTimestamp != networkEventModel.EventTimestamp))
                    Rows.Remove(networkEventModel);
            }
        }

    }
}

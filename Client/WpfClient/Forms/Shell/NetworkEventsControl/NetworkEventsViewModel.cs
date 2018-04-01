using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.Client
{
    public class NetworkEventsViewModel : PropertyChangedBase
    {
        public string TableTitle { get; set; }

        private readonly Model _readModel;
      
        private ObservableCollection<NetworkEventModel> _rows = new ObservableCollection<NetworkEventModel>();
        public ObservableCollection<NetworkEventModel> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        public NetworkEventsViewModel(Model readModel)
        {
            _readModel = readModel;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"EventTimestamp", ListSortDirection.Descending));
        }

        public void AddEvent(NetworkEvent networkEvent)
        {
            Rows.Add(new NetworkEventModel()
            {
                Ordinal = networkEvent.Ordinal,
                EventTimestamp = networkEvent.EventTimestamp,
                RtuId = networkEvent.RtuId,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == networkEvent.RtuId)?.Title,
                MainChannelState = networkEvent.MainChannelState,
                ReserveChannelState = networkEvent.ReserveChannelState,
            });
        }

        public void RemoveOldEventForRtuIfExists(Guid rtuId)
        {
            var oldEvent = Rows.FirstOrDefault(r => r.RtuId == rtuId);
            if (oldEvent != null)
                Rows.Remove(oldEvent);
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
    }
}

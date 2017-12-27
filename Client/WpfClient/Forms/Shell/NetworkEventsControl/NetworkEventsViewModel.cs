using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class NetworkEventsViewModel : PropertyChangedBase
    {
        public string TableTitle { get; set; }

        private readonly ReadModel _readModel;
      
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

        public NetworkEventsViewModel(ReadModel readModel)
        {
            _readModel = readModel;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Nomer", ListSortDirection.Descending));
        }

        public void AddEvent(NetworkEvent networkEvent)
        {
            Rows.Add(new NetworkEventModel()
            {
                Nomer = networkEvent.Id,
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
    }
}

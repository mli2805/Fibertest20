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
    public class BopNetworkEventsViewModel : PropertyChangedBase
    {
        public string TableTitle { get; set; }

        private readonly ReadModel _readModel;

        private ObservableCollection<BopNetworkEventModel> _rows = new ObservableCollection<BopNetworkEventModel>();
        public ObservableCollection<BopNetworkEventModel> Rows
        {
            get { return _rows; }
            set
            {
                if (Equals(value, _rows)) return;
                _rows = value;
                NotifyOfPropertyChange();
            }
        }

        public BopNetworkEventsViewModel(ReadModel readModel)
        {
            _readModel = readModel;

            var view = CollectionViewSource.GetDefaultView(Rows);
            view.SortDescriptions.Add(new SortDescription(@"Nomer", ListSortDirection.Descending));
        }

        public void AddEvent(BopNetworkEvent bopNetworkEvent)
        {
            Rows.Add(new BopNetworkEventModel()
            {
                Nomer = bopNetworkEvent.Id,
                EventTimestamp = bopNetworkEvent.EventTimestamp,
                BopId = bopNetworkEvent.BopId,
                RtuId = bopNetworkEvent.RtuId,
                BopTitle = _readModel.Otaus.FirstOrDefault(o=>o.Id == bopNetworkEvent.BopId)?.NetAddress.ToStringA(),
                RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == bopNetworkEvent.RtuId)?.Title,
                State = bopNetworkEvent.State,
            });
        }

        public void RemoveOldEventForBopIfExists(Guid bopId)
        {
            var oldEvent = Rows.FirstOrDefault(r => r.BopId == bopId);
            if (oldEvent != null)
                Rows.Remove(oldEvent);
        }
    }
}

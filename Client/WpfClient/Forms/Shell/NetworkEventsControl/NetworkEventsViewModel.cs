using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class NetworkEventsViewModel : PropertyChangedBase
    {
        private readonly ReadModel _readModel;

        private Visibility _networkEventsVisibility;
        public Visibility NetworkEventsVisibility
        {
            get { return _networkEventsVisibility; }
            set
            {
                if (value == _networkEventsVisibility) return;
                _networkEventsVisibility = value;
                NotifyOfPropertyChange();
            }
        }

        private ObservableCollection<NetworkEventVm> _rows = new ObservableCollection<NetworkEventVm>();
        public ObservableCollection<NetworkEventVm> Rows
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

        public void Apply(NetworkEvent networkEvent)
        {
            Rows.Add(new NetworkEventVm()
            {
                Nomer = networkEvent.Id,
                EventTimestamp = networkEvent.EventTimestamp,
                RtuTitle = _readModel.Rtus.FirstOrDefault(r => r.Id == networkEvent.RtuId)?.Title,
                MainChannelState = networkEvent.MainChannelState,
                ReserveChannelState = networkEvent.ReserveChannelState,
                BopString = networkEvent.BopString,
            });
        }
    }
}

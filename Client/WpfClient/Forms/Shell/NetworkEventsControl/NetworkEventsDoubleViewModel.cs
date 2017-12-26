using System.Linq;
using System.Windows;
using Caliburn.Micro;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class NetworkEventsDoubleViewModel : PropertyChangedBase
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
        public NetworkEventsViewModel ActualNetworkEventsViewModel { get; set; }
        public NetworkEventsViewModel AllNetworkEventsViewModel { get; set; }

        public NetworkEventsDoubleViewModel(ReadModel readModel, 
            NetworkEventsViewModel actualNetworkEventsViewModel, NetworkEventsViewModel allNetworkEventsViewModel)
        {
            ActualNetworkEventsViewModel = actualNetworkEventsViewModel;
            AllNetworkEventsViewModel = allNetworkEventsViewModel;
            _readModel = readModel;
        }

        public void Apply(NetworkEvent networkEvent)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(t => t.Id == networkEvent.RtuId);
            if (rtu == null)
                return;

            ActualNetworkEventsViewModel.RemoveOldEventForRtuIfExists(networkEvent.RtuId);

            if (networkEvent.IsAllRight)
                return;

            ActualNetworkEventsViewModel.AddEvent(networkEvent);
        }

        public void ApplyToTableAll(NetworkEvent networkEvent)
        {
            AllNetworkEventsViewModel.AddEvent(networkEvent);
        }


    }
}

using System.Linq;
using Caliburn.Micro;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class BopNetworkEventsDoubleViewModel : PropertyChangedBase
    {
        private readonly Model _readModel;
        private readonly CurrentUser _currentUser;
        private readonly SystemState _systemState;

        public BopNetworkEventsViewModel ActualBopNetworkEventsViewModel { get; set; }
        public BopNetworkEventsViewModel AllBopNetworkEventsViewModel { get; set; }

        public BopNetworkEventsDoubleViewModel(Model readModel, CurrentUser currentUser, SystemState systemState,
            BopNetworkEventsViewModel actualBopNetworkEventsViewModel, BopNetworkEventsViewModel allBopNetworkEventsViewModel)
        {
            ActualBopNetworkEventsViewModel = actualBopNetworkEventsViewModel;
            ActualBopNetworkEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllBopNetworkEventsViewModel = allBopNetworkEventsViewModel;
            AllBopNetworkEventsViewModel.TableTitle = Resources.SID_All_BOP_network_events;
            _readModel = readModel;
            _currentUser = currentUser;
            _systemState = systemState;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case BopNetworkEventAdded evnt: AddBopNetworkEvent(evnt); break;
                case OtauDetached evnt: DetachOtau(evnt); break;
            }

            _systemState.HasActualBopNetworkProblems = ActualBopNetworkEventsViewModel.Rows.Any();
        }

        private void AddBopNetworkEvent(BopNetworkEventAdded evnt)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(t => t.Id == evnt.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId))
                return;

            var bop = _readModel.Otaus.FirstOrDefault(o => o.Serial == evnt.Serial);
            if (bop == null)
            {
                return;
            }

            evnt.OtauIp = bop.NetAddress.Ip4Address;
            evnt.TcpPort = bop.NetAddress.Port;

            AllBopNetworkEventsViewModel.AddEvent(evnt);
            ActualBopNetworkEventsViewModel.RemoveOldEventForBopIfExists(evnt.OtauIp);

            if (evnt.IsOk)
                return;

            ActualBopNetworkEventsViewModel.AddEvent(evnt);
        }

        private void DetachOtau(OtauDetached evnt)
        {
            ActualBopNetworkEventsViewModel.RemoveEvents(evnt);
            AllBopNetworkEventsViewModel.RemoveEvents(evnt);
        }

    }
}

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

        public BopNetworkEventsViewModel ActualBopNetworkEventsViewModel { get; set; }
        public BopNetworkEventsViewModel AllBopNetworkEventsViewModel { get; set; }

        public BopNetworkEventsDoubleViewModel(Model readModel, CurrentUser currentUser,
            BopNetworkEventsViewModel actualBopNetworkEventsViewModel, BopNetworkEventsViewModel allBopNetworkEventsViewModel)
        {
            ActualBopNetworkEventsViewModel = actualBopNetworkEventsViewModel;
            ActualBopNetworkEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllBopNetworkEventsViewModel = allBopNetworkEventsViewModel;
            AllBopNetworkEventsViewModel.TableTitle = @"All Bop network events";
            _readModel = readModel;
            _currentUser = currentUser;
        }

        public void Apply(object e)
        {
            switch (e)
            {
                case BopNetworkEventAdded evnt: AddBopNetworkEvent(evnt); return;
                case OtauDetached evnt: DetachOtau(evnt); return;
            }
        }

        private void AddBopNetworkEvent(BopNetworkEventAdded evnt)
        {
            var rtu = _readModel.Rtus.FirstOrDefault(t => t.Id == evnt.RtuId);
            if (rtu == null || !rtu.ZoneIds.Contains(_currentUser.ZoneId))
                return;

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

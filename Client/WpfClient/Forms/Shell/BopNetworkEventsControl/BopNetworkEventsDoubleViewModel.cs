using System.Linq;
using AutoMapper;
using Caliburn.Micro;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.StringResources;

namespace Iit.Fibertest.Client
{
    public class BopNetworkEventsDoubleViewModel : PropertyChangedBase
    {
        private readonly IMapper _mapper = new MapperConfiguration(
            cfg => cfg.AddProfile<MappingEventToDomainModelProfile>()).CreateMapper();
        private readonly ReadModel _readModel;

        public BopNetworkEventsViewModel ActualBopNetworkEventsViewModel { get; set; }
        public BopNetworkEventsViewModel AllBopNetworkEventsViewModel { get; set; }

        public BopNetworkEventsDoubleViewModel(ReadModel readModel, 
            BopNetworkEventsViewModel actualBopNetworkEventsViewModel, BopNetworkEventsViewModel allBopNetworkEventsViewModel)
        {
            ActualBopNetworkEventsViewModel = actualBopNetworkEventsViewModel;
            ActualBopNetworkEventsViewModel.TableTitle = Resources.SID_Current_accidents;
            AllBopNetworkEventsViewModel = allBopNetworkEventsViewModel;
            AllBopNetworkEventsViewModel.TableTitle = @"All Bop network events";
            _readModel = readModel;
        }

        public void Apply(BopNetworkEventAdded evnt)
        {
            var bopNetworkEvent = _mapper.Map<BopNetworkEvent>(evnt);
            var rtu = _readModel.Rtus.FirstOrDefault(t => t.Id == bopNetworkEvent.RtuId);
            if (rtu == null)
                return;

            ActualBopNetworkEventsViewModel.RemoveOldEventForBopIfExists(bopNetworkEvent.RtuId);

            if (bopNetworkEvent.State == RtuPartState.Ok)
                return;

            ActualBopNetworkEventsViewModel.AddEvent(bopNetworkEvent);
        }

        public void ApplyToTableAll(BopNetworkEvent bopNetworkEvent)
        {
            AllBopNetworkEventsViewModel.AddEvent(bopNetworkEvent);
        }
    }
}

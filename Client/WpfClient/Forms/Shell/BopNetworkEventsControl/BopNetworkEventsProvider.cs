using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Client
{
    public class BopNetworkEventsProvider
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly BopNetworkEventsDoubleViewModel _bopNetworkEventsDoubleViewModel;

        private readonly List<object> _models = new List<object>();

        public BopNetworkEventsProvider(IWcfServiceForClient c2DWcfManager,
            ReadModel readModel, TreeOfRtuModel treeOfRtuModel,
            BopNetworkEventsDoubleViewModel bopNetworkEventsDoubleViewModel)
        {
            _c2DWcfManager = c2DWcfManager;
            _bopNetworkEventsDoubleViewModel = bopNetworkEventsDoubleViewModel;

            _models.Add(readModel);
            _models.Add(treeOfRtuModel);
            _models.Add(_bopNetworkEventsDoubleViewModel);
        }

        public async void LetsGetStarted()
        {
            var bopNetworkEvents = await _c2DWcfManager.GetBopNetworkEvents();
            if (bopNetworkEvents == null)
                return;

            ApplyNetworkEvents(bopNetworkEvents);
        }

        private void ApplyNetworkEvents(BopNetworkEventsList list)
        {
            foreach (var networkEvent in list.ActualEvents)
            {
                foreach (var m in _models)
                {
                    m.AsDynamic().Apply(networkEvent);
                }
            }

            foreach (var networkEvent in list.PageOfLastEvents)
            {
                _bopNetworkEventsDoubleViewModel.ApplyToTableAll(networkEvent);
            }
        }
    }
}

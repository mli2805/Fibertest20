using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;
using Iit.Fibertest.WcfServiceForClientInterface;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Client
{
    public class NetworkEventsProvider
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly NetworkEventsDoubleViewModel _networkEventsDoubleViewModel;

        private readonly List<object> _models = new List<object>();

        public NetworkEventsProvider(IWcfServiceForClient c2DWcfManager,
            ReadModel readModel, TreeOfRtuModel treeOfRtuModel,
            NetworkEventsDoubleViewModel networkEventsDoubleViewModel)
        {
            _c2DWcfManager = c2DWcfManager;
            _networkEventsDoubleViewModel = networkEventsDoubleViewModel;

            _models.Add(readModel);
            _models.Add(treeOfRtuModel);
            _models.Add(_networkEventsDoubleViewModel);
        }

        public async void LetsGetStarted()
        {
            var networkEvents = await _c2DWcfManager.GetNetworkEvents();
            if (networkEvents == null)
                return;

            ApplyNetworkEvents(networkEvents);
        }

        private void ApplyNetworkEvents(NetworkEventsList list)
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
                _networkEventsDoubleViewModel.ApplyToTableAll(networkEvent);
            }
        }
    }
}
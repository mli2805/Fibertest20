using System.Collections.Generic;
using Iit.Fibertest.Dto;
using Iit.Fibertest.WcfServiceForClientInterface;
using PrivateReflectionUsingDynamic;

namespace Iit.Fibertest.Client
{
    public class OpticalEventsProvider
    {
        private readonly IWcfServiceForClient _c2DWcfManager;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;

        private readonly List<object> _models = new List<object>();

        public OpticalEventsProvider(IWcfServiceForClient c2DWcfManager,
            ReadModel readModel, TreeOfRtuModel treeOfRtuModel,
            OpticalEventsDoubleViewModel opticalEventsDoubleViewModel)
        {
            _c2DWcfManager = c2DWcfManager;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;

            _models.Add(readModel);
            _models.Add(treeOfRtuModel);
            _models.Add(opticalEventsDoubleViewModel);
        }

        public async void LetsGetStarted()
        {
            var opticalEvents = await _c2DWcfManager.GetOpticalEvents();
            if (opticalEvents == null)
                return;

            ApplyOpticalEvents(opticalEvents);
        }

        private void ApplyOpticalEvents(MeasurementsList list)
        {
            foreach (var opticalEvent in list.ActualMeasurements)
            {
                foreach (var m in _models)
                {
                    m.AsDynamic().Apply(opticalEvent);
                }
            }

            foreach (var opticalEvent in list.PageOfLastMeasurements)
            {
                _opticalEventsDoubleViewModel.ApplyToTableAll(opticalEvent);
            }
        }
    }
}
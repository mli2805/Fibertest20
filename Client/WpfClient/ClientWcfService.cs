using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.ClientWcfServiceInterface;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        private readonly TreeOfRtuModel _treeOfRtuModel;
        private readonly TraceStateViewsManager _traceStateViewsManager;
        private readonly TraceStatisticsViewsManager _traceStatisticsViewsManager;
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly OpticalEventsDoubleViewModel _opticalEventsDoubleViewModel;

        public ClientWcfService(TreeOfRtuModel treeOfRtuModel,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            RtuStateViewsManager rtuStateViewsManager, OpticalEventsDoubleViewModel opticalEventsDoubleViewModel)
        {
            _treeOfRtuModel = treeOfRtuModel;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _rtuStateViewsManager = rtuStateViewsManager;
            _opticalEventsDoubleViewModel = opticalEventsDoubleViewModel;
        }

        public Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _rtuStateViewsManager.NotifyUserRtuCurrentMonitoringStep(dto);
            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutMonitoringResult(Measurement measurement)
        {
            _treeOfRtuModel.Apply(measurement);
            _traceStateViewsManager.NotifyAboutMonitoringResult(measurement);
            _traceStatisticsViewsManager.AddNewMeasurement(measurement);
            _rtuStateViewsManager.NotifyUserMonitoringResult(measurement);

            if (measurement.EventStatus > EventStatus.JustMeasurementNotAnEvent)
            {
                _opticalEventsDoubleViewModel.Apply(measurement);
                _opticalEventsDoubleViewModel.ApplyToTableAll(measurement);
            }

            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutNetworkEvents(NetworkEventsList dto)
        {
            foreach (var networkEvent in dto.Events)
            {
                _treeOfRtuModel.Apply(networkEvent);

                var rtuLeaf = (RtuLeaf)_treeOfRtuModel.Tree.GetById(networkEvent.RtuId);
                if (rtuLeaf != null)
                    _rtuStateViewsManager.NotifyUserRtuAvailabilityChanged(rtuLeaf);
            }
            return Task.FromResult(0);
        }

    }
}

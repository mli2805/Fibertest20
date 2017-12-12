using System.ServiceModel;
using System.Threading.Tasks;
using ClientWcfServiceInterface;
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
        private readonly OpticalEventsViewModel _opticalEventsViewModel;

        public ClientWcfService(TreeOfRtuModel treeOfRtuModel,
            TraceStateViewsManager traceStateViewsManager, TraceStatisticsViewsManager traceStatisticsViewsManager,
            RtuStateViewsManager rtuStateViewsManager, OpticalEventsViewModel opticalEventsViewModel)
        {
            _treeOfRtuModel = treeOfRtuModel;
            _traceStateViewsManager = traceStateViewsManager;
            _traceStatisticsViewsManager = traceStatisticsViewsManager;
            _rtuStateViewsManager = rtuStateViewsManager;
            _opticalEventsViewModel = opticalEventsViewModel;
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

            //TODO - polling or reacting ?
//            if (measurement.EventStatus > EventStatus.JustMeasurementNotAnEvent)
//            {
//                _opticalEventsViewModel.Apply(measurement);
//            }

            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutRtuChangedAvailability(ListOfRtuWithChangedAvailabilityDto dto)
        {
            _treeOfRtuModel.Apply(dto);
            foreach (var rtuWithChannelChanges in dto.List)
            {
                var rtuLeaf = (RtuLeaf)_treeOfRtuModel.Tree.GetById(rtuWithChannelChanges.RtuId);
                _rtuStateViewsManager.NotifyUserRtuAvailabilityChanged(rtuLeaf);
            }
            return Task.FromResult(0);
        }

    }
}

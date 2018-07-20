using System.ServiceModel;
using System.Threading.Tasks;
using Iit.Fibertest.ClientWcfServiceInterface;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        private readonly RtuStateViewsManager _rtuStateViewsManager;
        private readonly ClientMeasurementViewModel _clientMeasurementViewModel;

        public ClientWcfService(RtuStateViewsManager rtuStateViewsManager, ClientMeasurementViewModel clientMeasurementViewModel)
        {
            _rtuStateViewsManager = rtuStateViewsManager;
            _clientMeasurementViewModel = clientMeasurementViewModel;
        }

        public Task<int> NotifyUsersRtuCurrentMonitoringStep(CurrentMonitoringStepDto dto)
        {
            _rtuStateViewsManager.NotifyUserRtuCurrentMonitoringStep(dto);
            return Task.FromResult(0);
        }

        public Task<int> NotifyAboutMeasurementClientDone(ClientMeasurementDoneDto dto)
        {
            if (_clientMeasurementViewModel.IsOpen)
                _clientMeasurementViewModel.ShowReflectogram(dto.SorBytes);
            return Task.FromResult(0);
        }
    }
}

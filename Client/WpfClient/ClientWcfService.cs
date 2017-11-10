using System.ServiceModel;
using System.Threading.Tasks;
using ClientWcfServiceInterface;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        public event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);

        public void ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
            MessageReceived?.Invoke(dto);
        }

        public async Task<int> ProcessMonitoringResult(MonitoringResultDto dto)
        {
            MessageReceived?.Invoke(dto);
            return 0;
        }

        public async Task<int> NotifyAboutRtuChangedAvailability(ListOfRtuWithChangedAvailabilityDto dto)
        {
            MessageReceived?.Invoke(dto);
            return 0;
        }

    }
}

using System.ServiceModel;
using System.Threading.Tasks;
using ClientWcfServiceInterface;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.ClientWcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        public static IMyLog ClientLog { get; set; }

        private readonly IMyLog _logFile;
        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);

        public ClientWcfService(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public void ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
//            ClientLog.AppendLine($"RTU {dto.RtuId} sent current state: {dto.MonitoringStep}");
            MessageReceived?.Invoke(dto);
        }

        public async Task<int> NotifyAboutRtuChangedAvailability(ListOfRtuWithChangedAvailabilityDto dto)
        {
            foreach (var rtu in dto.List)
            {
               _logFile.AppendLine(rtu.Report()); 
            }
            return 0;
        }

    }
}

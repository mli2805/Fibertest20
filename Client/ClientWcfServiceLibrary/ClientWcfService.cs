using System.ServiceModel;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace ClientWcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        public static IMyLog ClientLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);


        public void ProcessRtuCurrentMonitoringStep(KnowRtuCurrentMonitoringStepDto dto)
        {
//            ClientLog.AppendLine($"RTU {dto.RtuId} sent current state: {dto.MonitoringStep}");
            MessageReceived?.Invoke(dto);
        }
    }
}

using System.ServiceModel;
using Dto;
using Iit.Fibertest.Utils35;

namespace ClientWcfServiceLibrary
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ClientWcfService : IClientWcfService
    {
        public static Logger35 ClientLog { get; set; }

        public static event OnMessageReceived MessageReceived;
        public delegate void OnMessageReceived(object e);

        public void ConfirmRtuInitialized(RtuInitialized rtu)
        {
            ClientLog.AppendLine($"RTU serial={rtu.Serial} confirmed initialization");
            MessageReceived?.Invoke(rtu);
        }

        public void ConfirmMonitoringStarted(MonitoringStarted confirm)
        {
            var result = confirm.IsSuccessful ? "confirmed: monitoring started." : "ERROR, can't start monitoring ";
            ClientLog.AppendLine($"Rtu {confirm.RtuId} {result}");
            MessageReceived?.Invoke(confirm);
        }
        public void ConfirmMonitoringStopped(MonitoringStopped confirm)
        {
            var result = confirm.IsSuccessful ? "confirmed: monitoring stopped." : "ERROR, can't stop monitoring ";
            ClientLog.AppendLine($"Rtu {confirm.RtuId} {result}");
            MessageReceived?.Invoke(confirm);
        }

    }
}

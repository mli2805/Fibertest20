using Dto;
using Iit.Fibertest.Utils35;

namespace Client_WcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "ClientWcfService" in both code and config file together.
    public class ClientWcfService : IClientWcfService
    {
        public static IniFile ClientIni { get; set; }
        public static Logger35 ClientLog { get; set; }
        public void ConfirmRtuInitialized(RtuInitialized rtu)
        {
            ClientLog.AppendLine($"{rtu.Serial}");
        }

        public void ConfirmMonitoringStarted(MonitoringStarted confirm)
        {
            var result = confirm.IsSuccessful ? "confirmed: monitoring started." : "ERROR, can't start monitoring ";
            ClientLog.AppendLine($"Rtu {confirm.RtuId} {result}");
        }
        public void ConfirmMonitoringStopped(MonitoringStopped confirm)
        {
            var result = confirm.IsSuccessful ? "confirmed: monitoring stopped." : "ERROR, can't stop monitoring ";
            ClientLog.AppendLine($"Rtu {confirm.RtuId} {result}");
        }
    }
}

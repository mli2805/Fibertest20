using Dto;
using Iit.Fibertest.Utils35;

namespace WcfConnections
{
    public class D2CWcfManager
    {
        private readonly Logger35 _logger35;
        private readonly WcfFactory _wcfFactory;

        public D2CWcfManager(string clientAddress, IniFile iniFile, Logger35 logger35)
        {
            _logger35 = logger35;
            _wcfFactory = new WcfFactory(clientAddress, iniFile, _logger35);
        }

        public void ConfirmRtuConnectionChecked(RtuConnectionCheckedDto dto)
        {
            var wcfConnection = _wcfFactory.CreateClientConnection();
            if (wcfConnection == null)
                return;

            wcfConnection.ConfirmRtuConnectionChecked(dto);
            _logger35.AppendLine($"Sent response on check connection with RTU {dto.RtuId}");
        }

        public void ConfirmRtuInitialized(RtuInitializedDto dto)
        {
            var wcfConnection = _wcfFactory.CreateClientConnection();
            if (wcfConnection == null)
                return;

            wcfConnection.ConfirmRtuInitialized(dto);
            _logger35.AppendLine($"Sent response on initialize RTU {dto.Serial}");
        }

        public void ConfirmMonitoringStarted(MonitoringStartedDto dto)
        {
            var wcfConnection = _wcfFactory.CreateClientConnection();
            if (wcfConnection == null)
                return;

            wcfConnection.ConfirmMonitoringStarted(dto);
            _logger35.AppendLine($"Sent response on start monitoring on RTU {dto.RtuId}");
        }

        public void ConfirmMonitoringStopped(MonitoringStoppedDto dto)
        {
            var wcfConnection = _wcfFactory.CreateClientConnection();
            if (wcfConnection == null)
                return;

            wcfConnection.ConfirmMonitoringStopped(dto);
            _logger35.AppendLine($"Sent response on stop monitoring on RTU {dto.RtuId}");
        }
    }
}
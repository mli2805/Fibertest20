using Dto;
using Iit.Fibertest.Utils35;

namespace WcfTestBench
{
    public class WcfC2DManager
    {
        private readonly Logger35 _logger35;
        private readonly WcfConnectionFactory _wcfConnectionFactory;

        public WcfC2DManager(string dataCenterAddress, IniFile iniFile, Logger35 logger35)
        {
            _logger35 = logger35;
            _wcfConnectionFactory = new WcfConnectionFactory(dataCenterAddress, iniFile, _logger35);
        }

        public bool RegisterClient(string clientAddress)
        {
            var wcfConnection = _wcfConnectionFactory.CreateServerConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.RegisterClientAsync(clientAddress);
            _logger35.AppendLine($@"Registered on server");
            return true;
        }

        public void UnRegisterClient(string clientAddress)
        {
            var wcfConnection = _wcfConnectionFactory.CreateServerConnection();
            if (wcfConnection == null)
                return;

            wcfConnection.UnRegisterClientAsync(clientAddress);
            _logger35.AppendLine($@"Unregistered on server");
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            var wcfConnection = _wcfConnectionFactory.CreateServerConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.CheckRtuConnectionAsync(dto);
            _logger35.AppendLine($@"Sent command to check connection with RTU {dto.RtuId}");
            return true;
        }

        public bool InitializeRtu(InitializeRtuDto dto)
        {
            var wcfConnection = _wcfConnectionFactory.CreateServerConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.InitializeRtuAsync(dto);
            _logger35.AppendLine($@"Sent command to initialize RTU {dto.RtuId} with ip={dto.RtuIpAddress}");
            return true;
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            var wcfConnection = _wcfConnectionFactory.CreateServerConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.AssignBaseRefAsync(dto);
            _logger35.AppendLine($@"Sent base ref to RTU with ip={dto.RtuIpAddress}");
            return true;
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            var wcfConnection = _wcfConnectionFactory.CreateServerConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.ApplyMonitoringSettings(dto);
            _logger35.AppendLine($@"Sent monitoring settings to RTU with ip={dto.RtuIpAddress}");
            return true;
        }

        public bool StartMonitoring(string rtuAddress)
        {
            var wcfConnection = _wcfConnectionFactory.CreateServerConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.StartMonitoringAsync(rtuAddress);
            _logger35.AppendLine($@"Sent command to start monitoring on RTU with ip={rtuAddress}");
            return true;
        }

        public bool StopMonitoring(string rtuAddress)
        {
            var wcfConnection = _wcfConnectionFactory.CreateServerConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.StopMonitoringAsync(rtuAddress);
            _logger35.AppendLine($@"Sent command to stop monitoring on RTU with ip={rtuAddress}");
            return true;
        }


    }
}

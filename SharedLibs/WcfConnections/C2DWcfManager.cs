using System.Threading;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfConnections
{
    public class C2DWcfManager
    {
        private readonly Logger35 _logger35;
        private readonly WcfFactory _wcfFactory;

        public C2DWcfManager(string dataCenterAddress, IniFile iniFile, Logger35 logger35)
        {
            _logger35 = logger35;
            _wcfFactory = new WcfFactory(dataCenterAddress, iniFile, _logger35);
        }

        public bool RegisterClient(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.RegisterClient(dto);
            _logger35.AppendLine($@"Registered on server");
            return true;
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return;

            wcfConnection.UnRegisterClient(dto);
            _logger35.AppendLine($@"Unregistered on server");
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.CheckRtuConnection(dto);
            _logger35.AppendLine($@"Sent command to check connection with RTU {dto.RtuId}");
            return true;
        }

        public bool InitializeRtu(InitializeRtuDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.InitializeRtu(dto);
            _logger35.AppendLine($@"Sent command to initialize RTU {dto.RtuId} with ip={dto.RtuIpAddress}");
            return true;
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.AssignBaseRef(dto);
            _logger35.AppendLine($@"Sent base ref to RTU with ip={dto.RtuIpAddress}");
            return true;
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.ApplyMonitoringSettings(dto);
            _logger35.AppendLine($@"Sent monitoring settings to RTU with ip={dto.RtuIpAddress}");
            return true;
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.StartMonitoring(dto);
            _logger35.AppendLine($@"Sent command to start monitoring on RTU with ip={dto.RtuAddress}");
            return true;
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            wcfConnection.StopMonitoring(dto);
            _logger35.AppendLine($@"Sent command to stop monitoring on RTU with ip={dto.RtuAddress}");
            return true;
        }


    }
}

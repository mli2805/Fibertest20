using System;
using Dto;
using Iit.Fibertest.Utils35;

namespace WcfConnections
{
    public class C2DWcfManager
    {
        private readonly LogFile _logFile;
        private readonly string _localAddress;
        private readonly WcfFactory _wcfFactory;

        public C2DWcfManager(DoubleAddressWithLastConnectionCheck dataCenterAddress, IniFile iniFile, LogFile logFile, string localAddress)
        {
            _logFile = logFile;
            _localAddress = localAddress;
            _wcfFactory = new WcfFactory(dataCenterAddress, iniFile, _logFile);
        }

        public bool RegisterClient(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientAddress = _localAddress;
                wcfConnection.RegisterClient(dto);
                _logFile.AppendLine($@"Registered on server");
                return true;
            }
            catch (Exception e)
            {
               _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public void UnRegisterClient(UnRegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                dto.ClientAddress = _localAddress;
                wcfConnection.UnRegisterClient(dto);
                _logFile.AppendLine($@"Unregistered on server");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);

            }
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientAddress = _localAddress;
                wcfConnection.CheckRtuConnection(dto);
                _logFile.AppendLine($@"Sent command to check connection with RTU {dto.RtuId}");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public bool InitializeRtu(InitializeRtuDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientAddress = _localAddress;
                wcfConnection.InitializeRtu(dto);
                _logFile.AppendLine($@"Sent command to initialize RTU {dto.RtuId} with ip={dto.RtuAddresses.Main.Ip4Address}");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public bool AssignBaseRef(AssignBaseRefDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientAddress = _localAddress;
                wcfConnection.AssignBaseRef(dto);
                _logFile.AppendLine($@"Sent base ref to RTU {dto.RtuId}");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public bool ApplyMonitoringSettings(ApplyMonitoringSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientAddress = _localAddress;
                wcfConnection.ApplyMonitoringSettings(dto);
                _logFile.AppendLine($@"Sent monitoring settings to RTU {dto.RtuId}");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public bool StartMonitoring(StartMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientAddress = _localAddress;
                wcfConnection.StartMonitoring(dto);
                _logFile.AppendLine($@"Sent command to start monitoring on RTU {dto.RtuId}");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public bool StopMonitoring(StopMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientAddress = _localAddress;
                wcfConnection.StopMonitoring(dto);
                _logFile.AppendLine($@"Sent command to stop monitoring on RTU {dto.RtuId}");
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }


    }
}

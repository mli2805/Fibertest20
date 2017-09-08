using System;
using Dto;
using Iit.Fibertest.UtilsLib;

namespace WcfConnections
{
    public class C2DWcfManager
    {
        private readonly LogFile _logFile;
        private readonly Guid _clientId;
        private readonly WcfFactory _wcfFactory;

        public C2DWcfManager(DoubleAddress dataCenterAddress, IniFile iniFile, LogFile logFile, Guid clientId)
        {
            _logFile = logFile;
            _clientId = clientId;
            _wcfFactory = new WcfFactory(dataCenterAddress, iniFile, _logFile);
        }

        public bool RegisterClient(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientId = _clientId;
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
                dto.ClientId = _clientId;
                wcfConnection.UnRegisterClient(dto);
                _logFile.AppendLine($@"Unregistered on server");
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);

            }
        }

        public bool CheckServerConnection(CheckServerConnectionDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientId = _clientId;
                var result = wcfConnection.CheckServerConnection(dto);
                _logFile.AppendLine($@"Server connection is {result.ToString().ToUpper()}");
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public bool CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientId = _clientId;
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
                dto.ClientId = _clientId;
                wcfConnection.InitializeRtu(dto);
                _logFile.AppendLine($@"Sent command to initialize RTU {dto.RtuId.First6()}");
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
                dto.ClientId = _clientId;
                wcfConnection.AssignBaseRef(dto);
                _logFile.AppendLine($@"Sent base ref to RTU {dto.RtuId.First6()}");
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
                dto.ClientId = _clientId;
                wcfConnection.ApplyMonitoringSettings(dto);
                _logFile.AppendLine($@"Sent monitoring settings to RTU {dto.RtuId.First6()}");
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
                dto.ClientId = _clientId;
                wcfConnection.StartMonitoring(dto);
                _logFile.AppendLine($@"Sent command to start monitoring on RTU {dto.RtuId.First6()}");
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
                dto.ClientId = _clientId;
                wcfConnection.StopMonitoring(dto);
                _logFile.AppendLine($@"Sent command to stop monitoring on RTU {dto.RtuId.First6()}");
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

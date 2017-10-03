using System;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class C2DWcfManager
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private readonly Guid _clientId;
        private WcfFactory _wcfFactory;

        public C2DWcfManager(DoubleAddress dataCenterAddress, IniFile iniFile, IMyLog logFile, Guid clientId)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            _clientId = clientId;
            _wcfFactory = new WcfFactory(dataCenterAddress, _iniFile, _logFile);
        }

        public void ChangeServerAddresses(DoubleAddress newServerAddress)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
        }

        public bool InitializeRtuLongTask(InitializeRtuDto dto)
        {
            var c2DChannel = _wcfFactory.CreateC2DConnection();
            if (c2DChannel == null)
                return false;

            try
            {
                c2DChannel.InitializeRtuLongTask(dto);
                return true;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }



        public ClientRegisteredDto RegisterClient(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new ClientRegisteredDto() { IsRegistered = false };

            try
            {
                dto.ClientId = _clientId;
                return wcfConnection.RegisterClientAsync(dto).Result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new ClientRegisteredDto() { IsRegistered = false };
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
                _logFile.AppendLine($@"Sent command to check connection with RTU {dto.RtuId.First6()}");
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

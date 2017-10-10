﻿using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.WcfConnections
{
    public class C2DWcfManager
    {
        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

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

        public async Task<string> SendCommand(object cmd)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            return await 
                wcfConnection.SendCommand(JsonConvert.SerializeObject(
                cmd, cmd.GetType(), JsonSerializerSettings));
        }

        public async Task<string[]> GetEvents(int revision)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            return await wcfConnection.GetEvents(revision);
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new ClientRegisteredDto() { IsRegistered = false };

            try
            {
                dto.ClientId = _clientId;
                return await wcfConnection.RegisterClientAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new ClientRegisteredDto() { IsRegistered = false };
            }
        }

        public async Task UnregisterClientAsync(UnRegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return;

            try
            {
                dto.ClientId = _clientId;
                await wcfConnection.UnregisterClientAsync(dto);
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

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($@"Check connection with RTU {dto.RtuId.First6()}");
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };

            try
            {
                dto.ClientId = _clientId;
                return await wcfConnection.CheckRtuConnectionAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };
            }
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var c2DChannel = _wcfFactory.CreateC2DConnection();
            if (c2DChannel == null)
                return new RtuInitializedDto() { IsInitialized = false, ErrorCode = 11 };

            try
            {
                _logFile.AppendLine($@"Sent command to initialize RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await c2DChannel.InitializeRtuAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new RtuInitializedDto() { IsInitialized = false, ErrorCode = 12 };
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

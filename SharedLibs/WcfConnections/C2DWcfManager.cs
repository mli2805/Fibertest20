using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Iit.Fibertest.WcfServiceForClientInterface;
using Newtonsoft.Json;

namespace Iit.Fibertest.WcfConnections
{
    public class C2DWcfManager : IWcfServiceForClient
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

        public C2DWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            Guid.TryParse(iniFile.Read(IniSection.General, IniKey.ClientGuidOnServer, Guid.NewGuid().ToString()), out _clientId);
        }

        public void SetServerAddresses(DoubleAddress newServerAddress)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            return await SendCommand(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings));
        }

        public async Task<string> SendCommand(string serializedCmd)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return @"Cannot establish datacenter connection.";

            try
            {
                return await wcfConnection.SendCommand(serializedCmd);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return @"Cannot send command to datacenter.";
            }
        }

        public async Task<string[]> GetEvents(int revision)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                // await wcfConnection.GetEvents(revision) blocks client !!!!!!!!!!!
                return wcfConnection.GetEvents(revision).Result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new string[0];
            }
        }

        public async Task<OpticalEventsList> GetOpticalEvents(int revision)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
//                var result = await wcfConnection.GetOpticalEvents(revision); // blocks client too
//                return result;
                return wcfConnection.GetOpticalEvents(revision).Result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new OpticalEventsList() {Events = new List<OpticalEvent>()};
            }
        }

        public async Task<NetworkEventsList> GetNetworkEvents(int revision)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                // await wcfConnection.GetEvents(revision) blocks client !!!!!!!!!!!
                return wcfConnection.GetNetworkEvents(revision).Result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new NetworkEventsList() { Events = new List<NetworkEvent>() };
            }
        }

        public Task<TraceStatistics> GetTraceStatistics(Guid traceId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return wcfConnection.GetTraceStatistics(traceId);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public Task<byte[]> GetSorBytesOfBase(Guid baseRefId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return wcfConnection.GetSorBytesOfBase(baseRefId);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public Task<byte[]> GetSorBytesOfMeasurement(int sorFileId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return wcfConnection.GetSorBytesOfMeasurement(sorFileId);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public Task<TraceStateDto> GetLastTraceState(Guid traceId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return wcfConnection.GetLastTraceState(traceId);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }


        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                dto.ClientId = _clientId;
                return await wcfConnection.RegisterClientAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.C2DWcfOperationError, ExceptionMessage = e.Message};
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

        public Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return TaskEx.FromResult(false);

            try
            {
                dto.ClientId = _clientId;
                return wcfConnection.CheckServerConnection(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return TaskEx.FromResult(false);
            }
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($@"Check connection with RTU {dto.NetAddress.ToStringA()}");
            var c2DChannel = _wcfFactory.CreateC2DConnection();
            if (c2DChannel == null)
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };

            try
            {
                dto.ClientId = _clientId;
                return await c2DChannel.CheckRtuConnectionAsync(dto);
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
                return new RtuInitializedDto() { IsInitialized = false, ReturnCode = ReturnCode.C2DWcfConnectionError, ErrorMessage = "Can't establish connection with DataCenter"};

            try
            {
                _logFile.AppendLine($@"Sent command to initialize RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await c2DChannel.InitializeRtuAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new RtuInitializedDto() { IsInitialized = false, ReturnCode = ReturnCode.C2DWcfOperationError, ErrorMessage = e.Message};
            }
        }

        public async Task<bool> StartMonitoringAsync(StartMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                _logFile.AppendLine($@"Sent command to start monitoring on RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await wcfConnection.StartMonitoringAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return false;

            try
            {
                _logFile.AppendLine($@"Sent command to stop monitoring on RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await wcfConnection.StopMonitoringAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new BaseRefAssignedDto() {ReturnCode = ReturnCode.C2DWcfConnectionError};

            try
            {
                _logFile.AppendLine($@"Sent base ref to RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await wcfConnection.AssignBaseRefAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message};
            }
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new MonitoringSettingsAppliedDto() {ReturnCode = ReturnCode.C2DWcfConnectionError};

            try
            {
                _logFile.AppendLine($@"Sent monitoring settings to RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await wcfConnection.ApplyMonitoringSettingsAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message};
            }
        }

    }
}

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
        private string _username;
        private string _clientIp;
        private WcfFactory _wcfFactory;

        public C2DWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
            Guid.TryParse(iniFile.Read(IniSection.General, IniKey.ClientGuidOnServer, Guid.NewGuid().ToString()), out _clientId);
        }

        public void SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
            _username = username;
            _clientIp = clientIp;
        }

        public async Task<int> SendCommandsAsObjs(List<object> cmds)
        {
            var list = new List<string>();
            foreach (var cmd in cmds)
            {
                list.Add(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings));
            }

            return await SendCommands(list, _username, _clientIp);
        }

        public async Task<int> SendCommands(List<string> jsons, string username, string clientIp)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return -1;

            try
            {
                return await wcfConnection.SendCommands(jsons, username, clientIp);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }

        public async Task<string> SendCommandAsObj(object cmd)
        {
            return await SendCommand(JsonConvert.SerializeObject(cmd, cmd.GetType(), JsonSerializerSettings), _username, _clientIp);
        }

        public async Task<string> SendCommand(string serializedCmd, string username, string clientIp)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return @"Cannot establish data-center connection.";

            try
            {
                return await wcfConnection.SendCommand(serializedCmd, username, clientIp);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return @"Cannot send command to data-center.";
            }
        }

        public async Task<string[]> GetEvents(int revision)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                var result = await wcfConnection.GetEvents(revision);
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new string[0];
            }
        }

        public async Task<MeasurementsList> GetOpticalEvents()
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                var result = await wcfConnection.GetOpticalEvents();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new MeasurementsList() {ActualMeasurements = new List<Measurement>(), PageOfLastMeasurements = new List<Measurement>()};
            }
        }

        public async Task<NetworkEventsList> GetNetworkEvents()
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return await wcfConnection.GetNetworkEvents();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new NetworkEventsList() { ActualEvents = new List<NetworkEvent>(), PageOfLastEvents = new List<NetworkEvent>()};
            }
        }

        public async Task<BopNetworkEventsList> GetBopNetworkEvents()
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return await wcfConnection.GetBopNetworkEvents();
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new BopNetworkEventsList() { ActualEvents = new List<BopNetworkEvent>(), PageOfLastEvents = new List<BopNetworkEvent>() };
            }
        }

        public async Task<TraceStatistics> GetTraceStatistics(Guid traceId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                var traceStatistics = await wcfConnection.GetTraceStatistics(traceId);
                return traceStatistics;
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

        public Task<byte[]> GetSorBytes(int sorFileId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return wcfConnection.GetSorBytes(sorFileId);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public async Task<byte[]> GetSorBytesOfLastTraceMeasurement(Guid traceId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return await wcfConnection.GetSorBytesOfLastTraceMeasurement(traceId);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public async Task<MeasurementWithSor> GetLastMeasurementForTrace(Guid traceId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return await wcfConnection.GetLastMeasurementForTrace(traceId);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public async Task<MeasurementUpdatedDto> SaveMeasurementChanges(UpdateMeasurementDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                dto.ClientId = _clientId;
                return await wcfConnection.SaveMeasurementChanges(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }

        public async Task<List<BaseRefDto>> GetTraceBaseRefsAsync(Guid traceId)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return null;

            try
            {
                return await wcfConnection.GetTraceBaseRefsAsync(traceId);
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
            var wcfConnection = _wcfFactory?.CreateC2DConnection();
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

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var c2DChannel = _wcfFactory.CreateC2DConnection();
            if (c2DChannel == null)
                return new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.C2DWcfConnectionError, ErrorMessage = "Can't establish connection with DataCenter" };

            try
            {
                _logFile.AppendLine($@"Sent command to attach OTAU {dto.OtauId.First6()}");
                dto.ClientId = _clientId;
                return await c2DChannel.AttachOtauAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.C2DWcfOperationError, ErrorMessage = e.Message };
            }
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var c2DChannel = _wcfFactory.CreateC2DConnection();
            if (c2DChannel == null)
                return new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.C2DWcfConnectionError, ErrorMessage = "Can't establish connection with DataCenter" };

            try
            {
                _logFile.AppendLine($@"Sent command to detach OTAU {dto.OtauId.First6()}");
                dto.ClientId = _clientId;
                return await c2DChannel.DetachOtauAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.C2DWcfOperationError, ErrorMessage = e.Message };
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

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
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

        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Re-Sent base ref to RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await wcfConnection.ReSendBaseRefAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Send command to do client's measurement on RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await wcfConnection.DoClientMeasurementAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoClientMeasuremenTask:" + e.Message);
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }

        public async Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var wcfConnection = _wcfFactory.CreateC2DConnection();
            if (wcfConnection == null)
                return new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Send command to do out of turn precise measurement on RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                return await wcfConnection.DoOutOfTurnPreciseMeasurementAsync(dto);
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoOutOfTurnPreciseMeasurement:" + e.Message);
                return new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }
    }
}

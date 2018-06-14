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
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SendCommands(jsons, username, clientIp);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }

        public async Task<int> SendMeas(List<AddMeasurementFromOldBase> list)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SendMeas(list);
                wcfConnection.Close();
                return result;
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
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return @"Cannot establish data-center connection.";

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SendCommand(serializedCmd, username, clientIp);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return @"Cannot send command to data-center.";
            }
        }

        public async Task<string[]> GetEvents(int revision)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetEvents(revision);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new string[0];
            }
        }

        public async Task<byte[]> GetSorBytes(int sorFileId)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetSorBytes(sorFileId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
        }




        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                dto.ClientId = _clientId;
                dto.Username = _username;
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.RegisterClientAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.C2DWcfOperationError, ExceptionMessage = e.Message };
            }
        }

        public async Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory?.GetC2DChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
                dto.ClientId = _clientId;
                dto.Username = _username;
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.UnregisterClientAsync(dto);
                _logFile.AppendLine($@"Unregistered on server");
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return -1;
            }
        }

        public async Task<bool> CheckServerConnection(CheckServerConnectionDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.CheckServerConnection(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public async Task<bool> SendTestEmail()
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.SendTestEmail();
                wcfConnection.Close();
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
            _logFile.AppendLine($@"Checking connection with RTU {dto.NetAddress.ToStringA()}");
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };

            try
            {
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.CheckRtuConnectionAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };
            }
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new RtuInitializedDto() { IsInitialized = false, ReturnCode = ReturnCode.C2DWcfConnectionError, ErrorMessage = "Can't establish connection with DataCenter" };

            try
            {
                _logFile.AppendLine($@"Sending command to initialize RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.InitializeRtuAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new RtuInitializedDto() { IsInitialized = false, ReturnCode = ReturnCode.C2DWcfOperationError, ErrorMessage = e.Message };
            }
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.C2DWcfConnectionError, ErrorMessage = "Can't establish connection with DataCenter" };

            try
            {
                _logFile.AppendLine($@"Sending command to attach OTAU {dto.OtauId.First6()}");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.AttachOtauAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.C2DWcfOperationError, ErrorMessage = e.Message };
            }
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.C2DWcfConnectionError, ErrorMessage = "Can't establish connection with DataCenter" };

            try
            {
                _logFile.AppendLine($@"Sending command to detach OTAU {dto.OtauId.First6()}");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.DetachOtauAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.C2DWcfOperationError, ErrorMessage = e.Message };
            }
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                _logFile.AppendLine($@"Sending command to stop monitoring on RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.StopMonitoringAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return false;
            }
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending monitoring settings to RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.ApplyMonitoringSettingsAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending base ref for trace {dto.TraceId.First6()}...");
                dto.ClientId = _clientId;
                dto.Username = _username;
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.AssignBaseRefAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending base ref for trace {dto.TraceId.First6()}...");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.AssignBaseRefAsyncFromMigrator(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }

        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Re-Sending base ref to RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.ReSendBaseRefAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending command to do client's measurement on RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.DoClientMeasurementAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoClientMeasuremenTask:" + e.Message);
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }

        public async Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var wcfConnection = _wcfFactory.GetC2DChannelFactory();
            if (wcfConnection == null)
                return new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending command to do out of turn precise measurement on RTU {dto.RtuId.First6()}");
                dto.ClientId = _clientId;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.DoOutOfTurnPreciseMeasurementAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoOutOfTurnPreciseMeasurement:" + e.Message);
                return new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.C2DWcfConnectionError, ExceptionMessage = e.Message };
            }
        }
    }
}

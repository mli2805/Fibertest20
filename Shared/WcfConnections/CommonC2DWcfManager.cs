using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class CommonC2DWcfManager : IWcfServiceCommonC2D
    {
        private readonly IniFile _iniFile;
        private readonly IMyLog _logFile;
        private string _username;
        private string _clientIp;
        private WcfFactory _wcfFactory;

        public CommonC2DWcfManager(IniFile iniFile, IMyLog logFile)
        {
            _iniFile = iniFile;
            _logFile = logFile;
        }

        public IWcfServiceCommonC2D SetServerAddresses(DoubleAddress newServerAddress, string username, string clientIp)
        {
            _wcfFactory = new WcfFactory(newServerAddress, _iniFile, _logFile);
            _username = username;
            _clientIp = clientIp;
            return this;
        }

        public async Task<ClientRegisteredDto> RegisterClientAsync(RegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.C2DWcfConnectionError };

            try
            {
                dto.UserName = _username;
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.RegisterClientAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterClientAsync: " + e.Message);
                return new ClientRegisteredDto() { ReturnCode = ReturnCode.C2DWcfOperationError, ErrorMessage = e.Message };
            }
        }

        public async Task<RequestAnswer> RegisterHeartbeat(string connectionId)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new RequestAnswer(){ReturnCode = ReturnCode.C2DWcfConnectionError};

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.RegisterHeartbeat(connectionId);
                wcfConnection.Close();

                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("RegisterClientAsync: " + e.Message);
                return new RequestAnswer() { ReturnCode = ReturnCode.C2DWcfOperationError, ErrorMessage = e.Message };
            }
        }

        public async Task<int> UnregisterClientAsync(UnRegisterClientDto dto)
        {
            var wcfConnection = _wcfFactory?.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return -1;

            try
            {
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
                _logFile.AppendLine("UnregisterClientAsync: " + e.Message);
                return -1;
            }
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnectionAsync(CheckRtuConnectionDto dto)
        {
            _logFile.AppendLine($@"Checking connection with RTU {dto.NetAddress.GetAddress()}");
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };

            try
            {
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.CheckRtuConnectionAsync(dto);
                wcfConnection.Close();
                _logFile.AppendLine(!result.IsConnectionSuccessfull
                    ? "No RTU connection!"
                    : $"RTU connected successfully {result.NetAddress.ToStringA()}");
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("CheckRtuConnectionAsync: " + e.Message);
                return new RtuConnectionCheckedDto() { IsConnectionSuccessfull = false };
            }
        }

        public async Task<RtuInitializedDto> InitializeRtuAsync(InitializeRtuDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new RtuInitializedDto()
                {
                    IsInitialized = false, 
                    ReturnCode = ReturnCode.C2RWcfConnectionError, 
                    ErrorMessage = "Can't establish connection with DataCenter"
                };

            try
            {
                _logFile.AppendLine($@"Sending command to initialize RTU {dto.RtuId.First6()}");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.InitializeRtuAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("InitializeRtuAsync: " + e.Message);
                return new RtuInitializedDto()
                {
                    IsInitialized = false, 
                    ReturnCode = ReturnCode.C2RWcfOperationError, 
                    ErrorMessage = e.Message
                };
            }
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new OtauAttachedDto()
                {
                    IsAttached = false, 
                    ReturnCode = ReturnCode.C2RWcfConnectionError, 
                    ErrorMessage = "Can't establish connection with DataCenter"
                };

            try
            {
                _logFile.AppendLine($@"Sending command to attach OTAU {dto.OtauId.First6()}");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.AttachOtauAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AttachOtauAsync: " + e.Message);
                return new OtauAttachedDto() { IsAttached = false, ReturnCode = ReturnCode.C2RWcfOperationError, ErrorMessage = e.Message };
            }
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.C2RWcfConnectionError, ErrorMessage = "Can't establish connection with DataCenter" };

            try
            {
                _logFile.AppendLine($@"Sending command to detach OTAU {dto.OtauId.First6()}");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.DetachOtauAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DetachOtauAsync: " + e.Message);
                return new OtauDetachedDto() { IsDetached = false, ReturnCode = ReturnCode.C2RWcfOperationError, ErrorMessage = e.Message };
            }
        }

        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return false;

            try
            {
                _logFile.AppendLine($@"Sending command to stop monitoring on RTU {dto.RtuId.First6()}");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.StopMonitoringAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("StopMonitoringAsync: " + e.Message);
                return false;
            }
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending monitoring settings to RTU {dto.RtuId.First6()}");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.ApplyMonitoringSettingsAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ApplyMonitoringSettingsAsync: " + e.Message);
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError, ErrorMessage = e.Message };
            }
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending base ref for trace {dto.TraceId.First6()}...");
                dto.Username = _username;
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.AssignBaseRefAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AssignBaseRefAsync: " + e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError, ErrorMessage = e.Message };
            }
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsyncFromMigrator(AssignBaseRefsDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending base ref for trace {dto.TraceId.First6()}...");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.AssignBaseRefAsyncFromMigrator(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AssignBaseRefAsyncFromMigrator: " + e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError, ErrorMessage = e.Message };
            }
        }

        public async Task<BaseRefAssignedDto> ReSendBaseRefAsync(ReSendBaseRefsDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Re-Sending base ref to RTU {dto.RtuId.First6()}");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.ReSendBaseRefAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ReSendBaseRefAsync: " + e.Message);
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError, ErrorMessage = e.Message };
            }
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending command to do client's measurement on RTU {dto.RtuId.First6()}");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.DoClientMeasurementAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoClientMeasuremenTask:" + e.Message);
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError, ErrorMessage = e.Message };
            }
        }

        public async Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError };

            try
            {
                _logFile.AppendLine($@"Sending command to do out of turn precise measurement on RTU {dto.RtuId.First6()}");
                dto.ClientIp = _clientIp;
                var channel = wcfConnection.CreateChannel();
                var result = await channel.DoOutOfTurnPreciseMeasurementAsync(dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoOutOfTurnPreciseMeasurement:" + e.Message);
                return new OutOfTurnMeasurementStartedDto() { ReturnCode = ReturnCode.C2RWcfConnectionError, ErrorMessage = e.Message };
            }
        }

        public async Task<string> UpdateMeasurement(string username, UpdateMeasurementDto dto)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.UpdateMeasurement(username, dto);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("UpdateMeasurement: " + e.Message);
                return null;
            }
        }

        public async Task<byte[]> GetSorBytes(int sorFileId)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
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
                _logFile.AppendLine("GetSorBytes: " + e.Message);
                return null;
            }
        }

        public async Task<RftsEventsDto> GetRftsEvents(int sorFileId)
        {
            var wcfConnection = _wcfFactory.GetCommonC2DChannelFactory();
            if (wcfConnection == null)
                return null;

            try
            {
                var channel = wcfConnection.CreateChannel();
                var result = await channel.GetRftsEvents(sorFileId);
                wcfConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("GetRftsEvents: " + e.Message);
                return null;
            }
        }
    }
}
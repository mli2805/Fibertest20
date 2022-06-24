using System;
using System.Threading;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class D2RWcfManager : ID2RWcfManager
    {
        private IMyLog _logFile;
        private WcfFactory _wcfFactory;

        public ID2RWcfManager SetRtuAddresses(DoubleAddress rtuAddress, IniFile iniFile, IMyLog logFile)
        {
            _logFile = logFile;
            _wcfFactory = new WcfFactory(rtuAddress, iniFile, logFile);
            return this;
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto, IniFile iniFile, IMyLog logFile)
        {
            var result = new RtuConnectionCheckedDto() { RtuId = dto.RtuId };
            var backward = new RtuWcfServiceBackward();

            var addressToCheck = new DoubleAddress() { Main = dto.NetAddress.Clone() };
            if (addressToCheck.Main.Port == -1)
            {
                logFile.AppendLine("D2RWcfManager: new RTU address.");
                addressToCheck.Main.Port = (int)TcpPorts.RtuListenTo;
                logFile.AppendLine($"Testing {addressToCheck.Main.ToStringA()} ...");
            }

            var wcfFactory = new WcfFactory(addressToCheck, iniFile, logFile);
            var rtuConnection = wcfFactory.GetDuplexRtuChannelFactory(backward);
            await Task.Factory.StartNew(() => Thread.Sleep(1)); // just to have await in function :)

            if (rtuConnection == null && dto.NetAddress.Port == -1)
            {
                addressToCheck.Main.Port = (int)TcpPorts.RtuVeexListenTo;
                logFile.AppendLine($"Testing {addressToCheck.Main.ToStringA()} ...");
                wcfFactory = new WcfFactory(addressToCheck, iniFile, logFile);
                rtuConnection = wcfFactory.GetDuplexRtuChannelFactory(backward);
            }

            result.IsConnectionSuccessfull = rtuConnection != null;
            result.NetAddress = dto.NetAddress.Clone();

            if (result.IsConnectionSuccessfull)
                result.NetAddress.Port = addressToCheck.Main.Port;
            else
                result.IsPingSuccessful = Pinger.Ping(dto.NetAddress.IsAddressSetAsIp ? dto.NetAddress.Ip4Address : dto.NetAddress.HostName);
            logFile.AppendLine($"Return {result.NetAddress.ToStringA()}");
            return result;
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.GetDuplexRtuChannelFactory(backward);
            if (rtuDuplexConnection == null)
                return new RtuInitializedDto(){ ReturnCode = ReturnCode.D2RWcfConnectionError };

            try
            {
                var channel = rtuDuplexConnection.CreateChannel();
                var result = await channel.InitializeAsync(backward, dto);
                rtuDuplexConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("InitializeAsync: " + e.Message);
                return null;
            }
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.GetDuplexRtuChannelFactory(backward);
            if (rtuDuplexConnection == null)
                return null;

            try
            {
                var channel = rtuDuplexConnection.CreateChannel();
                var result = await channel.AttachOtauAsync(backward, dto);
                rtuDuplexConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AttachOtauAsync: " + e.Message);
                return null;
            }
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.GetDuplexRtuChannelFactory(backward);
            if (rtuDuplexConnection == null)
                return null;

            try
            {
                var channel = rtuDuplexConnection.CreateChannel();
                var result = await channel.DetachOtauAsync(backward, dto);
                rtuDuplexConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DetachOtauAsync: " + e.Message);
                return null;
            }
        }


        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.GetDuplexRtuChannelFactory(backward);
            if (rtuDuplexConnection == null)
                return false;

            try
            {
                var channel = rtuDuplexConnection.CreateChannel();
                var result = await channel.StopMonitoringAsync(backward, dto);
                rtuDuplexConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("StopMonitoringAsync: " + e.Message);
                return false;
            }
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.GetDuplexRtuChannelFactory(backward);
            if (rtuDuplexConnection == null)
                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.D2RWcfConnectionError };

            try
            {
                var channel = rtuDuplexConnection.CreateChannel();
                var result = await channel.AssignBaseRefAsync(backward, dto);
                rtuDuplexConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("AssignBaseRefAsync: " + e.Message);
                return null;
            }
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.GetDuplexRtuChannelFactory(backward);
            if (rtuDuplexConnection == null)
                return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.D2RWcfConnectionError };

            try
            {
                var channel = rtuDuplexConnection.CreateChannel();
                var result = await channel.ApplyMonitoringSettingsAsync(backward, dto);
                rtuDuplexConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("ApplyMonitoringSettingsAsync: " + e.Message);
                return null;
            }
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.GetDuplexRtuChannelFactory(backward);
            if (rtuDuplexConnection == null)
                return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.D2RWcfConnectionError };

            try
            {
                var channel = rtuDuplexConnection.CreateChannel();
                var result = await channel.DoClientMeasurementAsync(backward, dto);
                rtuDuplexConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("DoClientMeasurementAsync: " + e.Message);
                return null;
            }
        }

        public async Task<RequestAnswer> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.GetDuplexRtuChannelFactory(backward);
            if (rtuDuplexConnection == null)
                return new RequestAnswer() { ReturnCode = ReturnCode.D2RWcfConnectionError };

            try
            {
                var channel = rtuDuplexConnection.CreateChannel();
                var result = await channel.StartOutOfTurnMeasurementAsync(backward, dto);
                rtuDuplexConnection.Close();
                return result;
            }
            catch (Exception e)
            {
                _logFile.AppendLine("StartOutOfTurnMeasurementAsync: " + e.Message);
                return null;
            }
        }
    }
}

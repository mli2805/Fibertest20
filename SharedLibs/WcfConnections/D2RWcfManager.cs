﻿using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.RtuWcfServiceInterface;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.WcfConnections
{
    public class D2RWcfManager : ID2RWcfManager
    {
        private WcfFactory _wcfFactory;

        public ID2RWcfManager SetRtuAddresses(DoubleAddress rtuAddress, IniFile iniFile, IMyLog logFile)
        {
            _wcfFactory = new WcfFactory(rtuAddress, iniFile, logFile);
            return this;
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto, IniFile iniFile, IMyLog logFile)
        {
            var result = new RtuConnectionCheckedDto() { RtuId = dto.RtuId };
            var backward = new RtuWcfServiceBackward();
            var wcfFactory = new WcfFactory(new DoubleAddress() { Main = dto.NetAddress }, iniFile, logFile);
            var rtuConnection = wcfFactory.CreateDuplexRtuConnection(backward);
            result.IsConnectionSuccessfull = rtuConnection != null;
            if (!result.IsConnectionSuccessfull)
                result.IsPingSuccessful = Pinger.Ping(dto.NetAddress.IsAddressSetAsIp ? dto.NetAddress.Ip4Address : dto.NetAddress.HostName);
            return result;
        }

        public async Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return null;

            var result = await rtuDuplexConnection.InitializeAsync(backward, dto);
            return result;
        }

        public async Task<OtauAttachedDto> AttachOtauAsync(AttachOtauDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return null;

            var result = await rtuDuplexConnection.AttachOtauAsync(backward, dto);
            return result;
        }

        public async Task<OtauDetachedDto> DetachOtauAsync(DetachOtauDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return null;

            var result = await rtuDuplexConnection.DetachOtauAsync(backward, dto);
            return result;
        }

    
        public async Task<bool> StopMonitoringAsync(StopMonitoringDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return false;

            var result = await rtuDuplexConnection.StopMonitoringAsync(backward, dto);
            return result;
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return new BaseRefAssignedDto() {ReturnCode = ReturnCode.D2RWcfConnectionError};

            return await rtuDuplexConnection.AssignBaseRefAsync(backward, dto);
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return new MonitoringSettingsAppliedDto() {ReturnCode = ReturnCode.D2RWcfConnectionError};

            return await rtuDuplexConnection.ApplyMonitoringSettingsAsync(backward, dto);
        }

        public async Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return new ClientMeasurementStartedDto() {ReturnCode = ReturnCode.D2RWcfConnectionError};

            return await rtuDuplexConnection.DoClientMeasurementAsync(backward, dto);
        }

        public async Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto)
        {
            var backward = new RtuWcfServiceBackward();
            var rtuDuplexConnection = _wcfFactory.CreateDuplexRtuConnection(backward);
            if (rtuDuplexConnection == null)
                return new OutOfTurnMeasurementStartedDto() {ReturnCode = ReturnCode.D2RWcfConnectionError};

            return await rtuDuplexConnection.StartOutOfTurnMeasurementAsync(backward, dto);
        }
    }
}

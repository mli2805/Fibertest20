﻿using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IClientToRtuTransmitter
    {
        Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto);
        Task<RtuInitializedDto> InitializeAsync(InitializeRtuDto dto);
        Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto);
        Task<bool> StopMonitoringAsync(StopMonitoringDto dto);
        Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto);
        Task<OutOfTurnMeasurementStartedDto> DoOutOfTurnPreciseMeasurementAsync(DoOutOfTurnPreciseMeasurementDto dto);
        Task<BaseRefAssignedDto> TransmitBaseRefsToRtu(AssignBaseRefsDto dto);
    }
}
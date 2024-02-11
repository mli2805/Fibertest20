﻿using Iit.Fibertest.Dto;
using System.Threading.Tasks;

namespace Iit.Fibertest.DataCenterCore
{
    public interface IMakLinuxConnector
    {
        Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto);
        Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto);
        Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, DoubleAddress rtuDoubleAddress);
        Task<RequestAnswer> StopMonitoringAsync(StopMonitoringDto dto, DoubleAddress rtuDoubleAddress);

        Task<ClientMeasurementStartedDto> DoClientMeasurementAsync(DoClientMeasurementDto dto,
            DoubleAddress rtuDoubleAddress);
        Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto, DoubleAddress rtuDoubleAddress);

        Task<RtuCurrentStateDto> GetRtuCurrentState(GetCurrentRtuStateDto dto);
    }
}

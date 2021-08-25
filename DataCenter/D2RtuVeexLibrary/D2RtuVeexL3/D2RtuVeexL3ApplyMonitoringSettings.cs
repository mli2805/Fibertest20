﻿using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<bool> StopMonitoringAsync(DoubleAddress rtuAddresses, string otdrId)
        {
            try
            {
                return await _d2RtuVeexLayer2.SetMonitoringState(rtuAddresses, otdrId, "disabled");
            }
            catch (Exception)
            {
                return false;
            }
        }

        // re-factored
        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, string otdrId, DoubleAddress rtuAddresses)
        {
            try
            {
                if (!await _d2RtuVeexLayer2.ApplyMoniSettingsToEveryTest(rtuAddresses, dto.Timespans.PreciseMeas, dto.Ports))
                    return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };

                var res = await _d2RtuVeexLayer2.SetMonitoringState(rtuAddresses, otdrId, dto.IsMonitoringOn ? "enabled" : "disabled");
                return res ? new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.MonitoringSettingsAppliedSuccessfully }
                           : new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };
            }
            catch (Exception e)
            {
                return new MonitoringSettingsAppliedDto()
                {
                    ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError,
                    ErrorMessage = e.Message
                };
            }
        }

      


    }
}

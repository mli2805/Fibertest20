using System;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<bool> StopMonitoringAsync(DoubleAddress rtuAddresses, string otdrId)
        {
            var proxy = await _d2RtuVeexLayer2.DisableProxyMode(rtuAddresses, otdrId);
            if (!proxy.IsSuccessful)
                return false;
            return await _d2RtuVeexLayer2.ChangeMonitoringState(rtuAddresses, otdrId, "disabled");

        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(
            ApplyMonitoringSettingsDto dto, DoubleAddress rtuAddresses)
        {
            try
            {
                var proxy = await _d2RtuVeexLayer2.DisableProxyMode(rtuAddresses, dto.OtdrId);
                if (!proxy.IsSuccessful)
                    return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };
          
                if (!await _d2RtuVeexLayer2.ApplyMoniSettingsToEveryTest(rtuAddresses, dto))
                    return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };

                var res = await _d2RtuVeexLayer2.ChangeMonitoringState(
                    rtuAddresses, dto.OtdrId, dto.IsMonitoringOn ? "enabled" : "disabled");
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

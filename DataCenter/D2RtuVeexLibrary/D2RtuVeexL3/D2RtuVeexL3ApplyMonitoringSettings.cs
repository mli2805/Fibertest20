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
            return await _d2RtuVeexLayer2.ChangeMonitoringState(rtuAddresses, "disabled");

        }

        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(
            ApplyMonitoringSettingsDto dto, DoubleAddress rtuAddresses)
        {
            try
            {
                var proxy = await _d2RtuVeexLayer2.DisableProxyMode(rtuAddresses, dto.OtdrId);
                if (!proxy.IsSuccessful)
                    return new RequestAnswer() 
                        { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError, ErrorMessage = proxy.ErrorMessage};
          
                if (!await _d2RtuVeexLayer2.ApplyMoniSettingsToEveryTest(rtuAddresses, dto))
                    return new RequestAnswer() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };

                var res = await _d2RtuVeexLayer2.ChangeMonitoringState(
                    rtuAddresses, dto.IsMonitoringOn ? "enabled" : "disabled");
                return res ? new RequestAnswer() { ReturnCode = ReturnCode.MonitoringSettingsAppliedSuccessfully }
                           : new RequestAnswer() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };
            }
            catch (Exception e)
            {
                return new RequestAnswer()
                {
                    ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError,
                    ErrorMessage = e.Message
                };
            }
        }
    }
}

using System;
using System.Linq;
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
                return await _d2RtuVeexLayer2.SetMonitoringMode(rtuAddresses, otdrId, "disabled");
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, string otdrId, DoubleAddress rtuAddresses)
        {
            int periodForPrecise = dto.Timespans.PreciseMeas != TimeSpan.Zero
                            ? (int)dto.Timespans.PreciseMeas.TotalSeconds : -1;
            try
            {
                var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuAddresses);
                foreach (var testLink in listOfTestLinks.items)
                {
                    Test test = await _d2RtuVeexLayer1.GetTest(rtuAddresses, testLink.self);
                    if (test?.otauPort == null) continue;

                    // no matter is test enabled or disabled, set its period, it does not do any harm
                    var period = test.name.ToLower().Contains("fast") ? 0 : periodForPrecise;
                    if (!await _d2RtuVeexLayer2.ChangeTestPeriod(test, rtuAddresses, period))
                        return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };

                    var portInCycle = dto.Ports.FirstOrDefault(p => p.OtauPort.OpticalPort - 1 == test.otauPort.portIndex);
                    var state = portInCycle == null || test.name.ToLower().Contains("additional")
                        ? "disabled" : "enabled";
                    if (!await _d2RtuVeexLayer2.ChangeTestState(test, rtuAddresses, state))
                        return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };
                }

                var res = await _d2RtuVeexLayer2.SetMonitoringMode(rtuAddresses, otdrId, dto.IsMonitoringOn ? "enabled" : "disabled");
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

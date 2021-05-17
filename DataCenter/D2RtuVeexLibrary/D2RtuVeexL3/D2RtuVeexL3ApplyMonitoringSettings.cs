using System;
using System.Linq;
using System.Net;
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
                return await SetMonitoringMode(rtuAddresses, otdrId, "disabled");
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
                var listOfTestLinks = await _d2RtuVeexLayer2.GetTests(rtuAddresses);
                foreach (var testLink in listOfTestLinks.items)
                {
                    Test test = await _d2RtuVeexLayer2.GetTest(rtuAddresses, testLink.self);
                    if (test?.otauPort == null) continue;

                    // no matter is test enabled or disabled, set its period, it does not do any harm
                    var period = test.name.ToLower().Contains("fast") ? 0 : periodForPrecise;
                    if ((await ChangedTestPeriod(test, rtuAddresses, period)).HttpStatusCode != HttpStatusCode.NoContent)
                        return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };

                    var portInCycle = dto.Ports.FirstOrDefault(p => p.OtauPort.OpticalPort - 1 == test.otauPort.portIndex);
                    var state = portInCycle == null || test.name.ToLower().Contains("additional")
                        ? "disabled" : "enabled";
                    if (( await ChangeTestState(test, rtuAddresses, state)).HttpStatusCode != HttpStatusCode.NoContent)
                        return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError };
                }

                var res = await SetMonitoringMode(rtuAddresses, otdrId, dto.IsMonitoringOn ? "enabled" : "disabled");
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

        // monitoring mode could not be changed if otdr in "proxy" mode (for reflect connection)
        // if it is so - proxy mode should be changed
        private async Task<bool> SetMonitoringMode(DoubleAddress rtuAddresses, string otdrId, string mode)
        {
            _logFile.AppendLine("SetMonitoringMode:");
            var httpRequestResult = await _d2RtuVeexLayer2.SetMonitoringMode(rtuAddresses, mode);
            _logFile.AppendLine($"SetMonitoringMode request result status code: { httpRequestResult.HttpStatusCode}");
            if (httpRequestResult.HttpStatusCode == HttpStatusCode.Conflict)
            {
                var proxy = await _d2RtuVeexLayer2.ChangeProxyMode(rtuAddresses, otdrId);
                _logFile.AppendLine($"ChangeProxyMode request result status code: { proxy.HttpStatusCode}");
                if (proxy.HttpStatusCode != HttpStatusCode.NoContent)
                    return false;

                httpRequestResult = await _d2RtuVeexLayer2.SetMonitoringMode(rtuAddresses, mode);
            }

            return httpRequestResult.HttpStatusCode == HttpStatusCode.NoContent;
        }

        private async Task<HttpRequestResult> ChangeTestState(Test test, DoubleAddress rtuAddresses, string state)
        {
            return await _d2RtuVeexLayer2.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { state = state });
        }

        private async Task<HttpRequestResult> ChangedTestPeriod(Test test, DoubleAddress rtuAddresses, int periodForPrecise)
        {
            return await _d2RtuVeexLayer2.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { period = periodForPrecise });
        }


    }
}

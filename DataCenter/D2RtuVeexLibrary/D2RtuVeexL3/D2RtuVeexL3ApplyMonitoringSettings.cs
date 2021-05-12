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

                    var portInCycle = dto.Ports.FirstOrDefault(p => p.OtauPort.OpticalPort - 1 == test.otauPort.portIndex);
                    if (portInCycle == null)
                        DisableTest(test, rtuAddresses);
                    else
                    {
                        if (test.name.ToLower().Contains("additional"))
                            SetPeriod(test, rtuAddresses, -1);
                        if (test.name.ToLower().Contains("precise"))
                            SetPeriod(test, rtuAddresses, periodForPrecise);
                        EnableTest(test, rtuAddresses);
                    }
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
            var httpRequestResult = await _d2RtuVeexLayer2.SetMonitoringMode(rtuAddresses, mode);
            if (httpRequestResult.HttpStatusCode == HttpStatusCode.Conflict)
            {
                var proxy = await _d2RtuVeexLayer2.ChangeProxyMode(rtuAddresses, otdrId);
                if (proxy.HttpStatusCode != HttpStatusCode.NoContent)
                    return false;

                httpRequestResult = await _d2RtuVeexLayer2.SetMonitoringMode(rtuAddresses, mode);
            }

            return httpRequestResult.HttpStatusCode == HttpStatusCode.NoContent;
        }

        private async void DisableTest(Test test, DoubleAddress rtuAddresses)
        {
            var disableResult = await _d2RtuVeexLayer2.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { state = "disabled" });
            if (disableResult.HttpStatusCode != HttpStatusCode.NoContent)
                throw new Exception(disableResult.ErrorMessage);
        }

        private async void EnableTest(Test test, DoubleAddress rtuAddresses)
        {
            var enableTest = await _d2RtuVeexLayer2.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { state = "enabled" });
            if (enableTest.HttpStatusCode != HttpStatusCode.NoContent)
                throw new Exception(enableTest.ErrorMessage);
        }

        private async void SetPeriod(Test test, DoubleAddress rtuAddresses, int periodForPrecise)
        {
            var changePeriod = await _d2RtuVeexLayer2.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { period = periodForPrecise });
            if (changePeriod.HttpStatusCode != HttpStatusCode.NoContent)
                throw new Exception(changePeriod.ErrorMessage);
        }

    }
}

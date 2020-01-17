using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
     public async Task<MonitoringSettingsAppliedDto> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto, DoubleAddress rtuAddresses)
        {
            int periodForPrecise = dto.Timespans.PreciseMeas != TimeSpan.Zero
                            ? (int)dto.Timespans.PreciseMeas.TotalSeconds : -1;
            try
            {
                var listOfTestLinks = await _d2RtuVeexMonitoring.GetTests(rtuAddresses);
                foreach (var testLink in listOfTestLinks.items)
                {
                    Test test = await _d2RtuVeexMonitoring.GetTest(rtuAddresses, testLink.self);
                    if (test?.otauPort == null) continue;

                    var portInCycle = dto.Ports.FirstOrDefault(p => p.OtauPort.OpticalPort - 1 == test.otauPort.portIndex);
                    if (portInCycle == null)
                        DisableTest(test, rtuAddresses);
                    else
                        SetPeriodAndEnableTest(test, rtuAddresses, periodForPrecise);
                }

                SetMonitoringMode(rtuAddresses, dto.IsMonitoringOn ? "enabled" : "disabled");
            }
            catch (Exception e)
            {
                return new MonitoringSettingsAppliedDto()
                {
                    ReturnCode = ReturnCode.RtuMonitoringSettingsApplyError,
                    ExceptionMessage = e.Message
                };
            }

            return new MonitoringSettingsAppliedDto() { ReturnCode = ReturnCode.MonitoringSettingsAppliedSuccessfully };
        }

        private async void SetMonitoringMode(DoubleAddress rtuAddresses, string mode)
        {
            var setMode = await _d2RtuVeexMonitoring.SetMonitoringMode(rtuAddresses, mode);
            if (setMode.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception(setMode.ErrorMessage);
        }

        private async void DisableTest(Test test, DoubleAddress rtuAddresses)
        {
            var disableResult = await _d2RtuVeexMonitoring.ChangeTest(rtuAddresses, test.id, new Test() { state = "disabled" });
            if (disableResult.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception(disableResult.ErrorMessage);
        }

        private async void SetPeriodAndEnableTest(Test test, DoubleAddress rtuAddresses, int periodForPrecise)
        {
            if (test.name == "precise")
            {
                var changePeriod = await _d2RtuVeexMonitoring.ChangeTest(rtuAddresses, test.id, new Test() { period = periodForPrecise });
                if (changePeriod.HttpStatusCode != HttpStatusCode.OK)
                    throw new Exception(changePeriod.ErrorMessage);
            }
            var enableTest = await _d2RtuVeexMonitoring.ChangeTest(rtuAddresses, test.id, new Test() { state = "enabled" });
            if (enableTest.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception(enableTest.ErrorMessage);
        } }
}

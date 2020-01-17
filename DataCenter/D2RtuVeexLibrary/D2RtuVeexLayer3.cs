using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class D2RtuVeexLayer3
    {
        private readonly D2RtuVeexMonitoring _d2RtuVeexMonitoring;

        public D2RtuVeexLayer3(D2RtuVeexMonitoring d2RtuVeexMonitoring)
        {
            _d2RtuVeexMonitoring = d2RtuVeexMonitoring;
        }

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
        }

        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto, DoubleAddress rtuAddresses)
        {
            var testsForPort = await GetTestsForPort(rtuAddresses, dto.OtauPortDto.OpticalPort);
            foreach (var baseRefDto in dto.BaseRefs)
            {
                string testLink;
                Test test = testsForPort.FirstOrDefault(t => t.name.Contains(baseRefDto.BaseRefType.ToString().ToLower()));
                if (test == null) // if there is no test - create it
                {
                    var testName = $@"Port { dto.OtauPortDto.OpticalPort}, {
                        baseRefDto.BaseRefType.ToString().ToLower()}, created {
                            DateTime.Now.ToString(CultureInfo.DefaultThreadCurrentUICulture)}";
                    var result = await CreateTest(rtuAddresses, testName, dto.OtauPortDto.OpticalPort);
                    if (result.HttpStatusCode != HttpStatusCode.Created)
                        return new BaseRefAssignedDto()
                        {
                            ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                            ExceptionMessage = result.ErrorMessage + ";  " + result.ResponseJson,
                        };
                    testLink = result.ResponseJson;
                }
                else testLink = $@"tests/{test.id}";

                // assign Reference to Test
                var assignResult = await _d2RtuVeexMonitoring.SetBaseRef(rtuAddresses, $@"monitoring/{testLink}/references", baseRefDto.SorBytes);
                if (assignResult.HttpStatusCode != HttpStatusCode.Created)
                    return new BaseRefAssignedDto()
                    {
                        ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                        ExceptionMessage = assignResult.ErrorMessage + ";  " + assignResult.ResponseJson,
                    };
            }

            return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
        }

        private async Task<List<Test>> GetTestsForPort(DoubleAddress rtuDoubleAddress, int opticalPort)
        {
            var result = new List<Test>();
            var listOfTestLinks = await _d2RtuVeexMonitoring.GetTests(rtuDoubleAddress);
            if (listOfTestLinks == null) return result;

            foreach (var testLink in listOfTestLinks.items)
            {
                var test = await _d2RtuVeexMonitoring.GetTest(rtuDoubleAddress, testLink.self);
                if (test?.otauPort != null && test.otauPort.portIndex == opticalPort - 1)
                    result.Add(test);
            }

            return result;
        }

        private async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress, string name, int opticalPort)
        {
            var newTest = new CreateTestCmd()
            {
                id = Guid.NewGuid().ToString(),
                name = name,
                state = "disabled",
                otdrId = Guid.Empty.ToString(), //TODO real OTDR id or maybe RTU id ?
                otauPort = new OtauPort() { otauId = Guid.Empty.ToString(), portIndex = opticalPort - 1 }, //
                period = 0, // null in the future
            };
            var result = await _d2RtuVeexMonitoring.CreateTest(rtuDoubleAddress, newTest);
            //            if (result.HttpStatusCode == HttpStatusCode.Created)
            //                result.ResponseJson = $@"monitoring/test/{newTest.id}/references";
            return result;
        }

    }
}
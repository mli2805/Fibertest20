﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<bool> ApplyMoniSettingsToEveryTest(DoubleAddress rtuAddresses, TimeSpan preciseTimeSpan,
            List<PortWithTraceDto> includedPorts)
        {
            var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuAddresses);
            foreach (var testLink in listOfTestLinks.items)
            {
                Test test = await _d2RtuVeexLayer1.GetTest(rtuAddresses, testLink.self);
                if (test?.otauPort == null) continue;

                if (!await ApplyMoniSettingsToOneTest(rtuAddresses, includedPorts, test, (int)preciseTimeSpan.TotalSeconds))
                    return false;
            }

            return true;
        }

        private async Task<bool> ApplyMoniSettingsToOneTest(DoubleAddress rtuAddresses, 
            List<PortWithTraceDto> includedPorts, Test test, int periodForPrecise)
        {
            // no matter is test enabled or disabled, set its period, it does not do any harm
            var isFast = test.name.ToLower().Contains("fast");
            var periodShouldBe = isFast
                ? 0
                : periodForPrecise;
            var failedPeriodShouldBe = isFast
                ? int.MaxValue
                : 0;

            // if included in cycle state should be "enabled"
            var stateShouldBe = includedPorts.All(p => p.OtauPort.OpticalPort - 1 != test.otauPort.portIndex)
                ? "disabled"
                : "enabled";

            if (test.period != periodShouldBe)
                if (!await ChangeTestPeriod(rtuAddresses, test, periodShouldBe, failedPeriodShouldBe))
                    return false;

            if (test.state == "disabled" && stateShouldBe == "enabled" 
                || test.state != "disabled" && stateShouldBe == "disabled")
                if (!await ChangeTestState(rtuAddresses, test, stateShouldBe))
                    return false;

            return true;
        }

        // monitoring mode could not be changed if otdr in "proxy" mode (for reflect connection)
        // if it is so - proxy mode should be changed
        public async Task<bool> SetMonitoringState(DoubleAddress rtuAddresses, string otdrId, string state)
        {
            _logFile.AppendLine("SetMonitoringState:");
            var httpRequestResult = await _d2RtuVeexLayer1.SetMonitoringState(rtuAddresses, state);
            _logFile.AppendLine($"SetMonitoringState request result status code: { httpRequestResult.HttpStatusCode}");
            if (httpRequestResult.HttpStatusCode == HttpStatusCode.Conflict)
            {
                var proxy = await _d2RtuVeexLayer1.ChangeProxyMode(rtuAddresses, otdrId, false);
                _logFile.AppendLine($"ChangeProxyMode request result status code: { proxy.HttpStatusCode}");
                if (proxy.HttpStatusCode != HttpStatusCode.NoContent)
                    return false;

                httpRequestResult = await _d2RtuVeexLayer1.SetMonitoringState(rtuAddresses, state);
            }

            return httpRequestResult.HttpStatusCode == HttpStatusCode.NoContent;
        }
    }
}

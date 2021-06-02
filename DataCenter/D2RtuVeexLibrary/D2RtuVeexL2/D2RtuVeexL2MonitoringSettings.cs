using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        /// <summary>
        /// Временно работаем по схеме Мониторинг по одной базовой (точной)
        /// Потом Леша сделает связку быстрая - точная на рту
        /// </summary>
        /// <param name="rtuAddresses"></param>
        /// <param name="preciseTimeSpan"></param>
        /// <param name="includedPorts"></param>
        /// <returns></returns>
        public async Task<bool> ApplyMoniSettingsToEveryTest(DoubleAddress rtuAddresses, TimeSpan preciseTimeSpan,
            List<PortWithTraceDto> includedPorts)
        {
            var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuAddresses);
            foreach (var testLink in listOfTestLinks.items)
            {
                Test test = await _d2RtuVeexLayer1.GetTest(rtuAddresses, testLink.self);
                if (test?.otauPort == null) continue;
                if (test.name.Contains("fast")) continue;

                if (!await ApplyMoniSettingsToOneTest(rtuAddresses, includedPorts, test, 0, 0))
                    return false;
            }

            return true;
        }


        // public async Task<bool> ApplyMoniSettingsToEveryTest(DoubleAddress rtuAddresses, TimeSpan preciseTimeSpan, List<PortWithTraceDto> includedPorts)
        // {
        //     int periodForFast = 0; // run permanently
        //     int periodForPrecise = preciseTimeSpan != TimeSpan.Zero
        //         ? (int)preciseTimeSpan.TotalSeconds : -1;
        //
        //     var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuAddresses);
        //     foreach (var testLink in listOfTestLinks.items)
        //     {
        //         Test test = await _d2RtuVeexLayer1.GetTest(rtuAddresses, testLink.self);
        //         if (test?.otauPort == null) continue;
        //
        //         if (!await ApplyMoniSettingsToOneTest(rtuAddresses, includedPorts, test, periodForFast, periodForPrecise))
        //             return false;
        //     }
        //
        //     return true;
        // }

        private async Task<bool> ApplyMoniSettingsToOneTest(DoubleAddress rtuAddresses, List<PortWithTraceDto> includedPorts, Test test,
            int periodForFast, int periodForPrecise)
        {
            // no matter is test enabled or disabled, set its period, it does not do any harm
            var period = test.name.ToLower().Contains("fast")
                ? periodForFast
                : periodForPrecise;
            // if included in cycle state should be "enabled"
            var portInCycle =
                includedPorts.FirstOrDefault(p => p.OtauPort.OpticalPort - 1 == test.otauPort.portIndex);
            var state = portInCycle == null || test.name.ToLower().Contains("additional")
                ? "disabled"
                : "enabled";

            if (test.period != period)
                if (!await ChangeTestPeriod(rtuAddresses, test, period))
                    return false;

            if (test.state == "disabled" && state == "enabled" 
                || test.state != "disabled" && state == "disabled")
                if (!await ChangeTestState(rtuAddresses, test, state))
                    return false;

            return true;
        }

        // monitoring mode could not be changed if otdr in "proxy" mode (for reflect connection)
        // if it is so - proxy mode should be changed
        public async Task<bool> SetMonitoringMode(DoubleAddress rtuAddresses, string otdrId, string mode)
        {
            _logFile.AppendLine("SetMonitoringMode:");
            var httpRequestResult = await _d2RtuVeexLayer1.SetMonitoringMode(rtuAddresses, mode);
            _logFile.AppendLine($"SetMonitoringMode request result status code: { httpRequestResult.HttpStatusCode}");
            if (httpRequestResult.HttpStatusCode == HttpStatusCode.Conflict)
            {
                var proxy = await _d2RtuVeexLayer1.ChangeProxyMode(rtuAddresses, otdrId);
                _logFile.AppendLine($"ChangeProxyMode request result status code: { proxy.HttpStatusCode}");
                if (proxy.HttpStatusCode != HttpStatusCode.NoContent)
                    return false;

                httpRequestResult = await _d2RtuVeexLayer1.SetMonitoringMode(rtuAddresses, mode);
            }

            return httpRequestResult.HttpStatusCode == HttpStatusCode.NoContent;
        }
    }
}

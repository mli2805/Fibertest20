using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<bool> ApplyMoniSettingsToEveryTest(DoubleAddress rtuAddresses, ApplyMonitoringSettingsDto dto)
        {
            var getResult = await _d2RtuVeexLayer1.GetTests(rtuAddresses);
            if (!getResult.IsSuccessful)
                return false;

            var listOfTestLinks = (LinkList)getResult.ResponseObject;
            foreach (var testLink in listOfTestLinks.items)
            {
                var getRes = await _d2RtuVeexLayer1.GetTest(rtuAddresses, testLink.self);
                if (!getRes.IsSuccessful)
                    return false;
                Test test = (Test)getRes.ResponseObject;
                if (test.otauPorts == null) continue;

                if (!await ApplyMoniSettingsToOneTest(rtuAddresses, dto, test))
                    return false;
            }

            return true;
        }

        private async Task<bool> ApplyMoniSettingsToOneTest(DoubleAddress rtuAddresses, ApplyMonitoringSettingsDto dto,
            Test test)
        {
            // no matter is test enabled or disabled, set its period, it does not do any harm
            var isFast = test.name.ToLower().Contains("fast");
            var periodShouldBe = isFast
                ? 0
                : (int)dto.Timespans.PreciseMeas.TotalSeconds;
            var failedPeriodShouldBe = isFast
                ? int.MaxValue
                : 0;

            var otauPortsUnderMonitoring = dto.Ports.Select(p => VeexPortExt.Create(p, dto.MainVeexOtau.id)).ToList();

            // if included in cycle state should be "enabled"
            var stateShouldBe = otauPortsUnderMonitoring.Any(o=>o.IsEqual(test.otauPorts))
                ? "enabled"
                : "disabled";

            if (test.period != periodShouldBe)
                if (!await ChangeTestPeriod(rtuAddresses, test, periodShouldBe, failedPeriodShouldBe))
                    return false;

            if (test.state == "disabled" && stateShouldBe == "enabled" 
                || test.state != "disabled" && stateShouldBe == "disabled")
                if (!await ChangeTestState(rtuAddresses, test, stateShouldBe))
                    return false;

            return true;
        }

        public async Task<bool> ChangeMonitoringState(DoubleAddress rtuAddresses, string state)
        {
            return (await _d2RtuVeexLayer1.SetMonitoringProperty(rtuAddresses, "state", state)).IsSuccessful;
        }
    }
}

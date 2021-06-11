using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<bool> DeleteTestsForPort(DoubleAddress rtuDoubleAddress, int opticalPort)
        {
            var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress);
            if (listOfTestLinks == null) return true;
            foreach (var testLink in listOfTestLinks.Items)
            {
                var test = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.Self);
                if (test?.OtauPort != null && test.OtauPort.PortIndex == opticalPort - 1)
                {
                    var deleteResult = await _d2RtuVeexLayer1.DeleteTest(rtuDoubleAddress, testLink.Self);
                    if (!deleteResult)
                        return false;
                }
            }
            return true;
        }

        public async Task<bool> DeleteTestForPortAndBaseType(DoubleAddress rtuDoubleAddress, int opticalPort, string baseType)
        {
            var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress); 
            if (listOfTestLinks == null) return true;

            foreach (var testLink in listOfTestLinks.Items)
            {
                var test = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.Self);
                if (test?.OtauPort != null 
                    && test.OtauPort.PortIndex == opticalPort - 1 
                    && test.Name.Contains(baseType))
                    return await _d2RtuVeexLayer1.DeleteTest(rtuDoubleAddress, testLink.Self);
            }
            return true;
        }

        public async Task<List<Test>> GetTestsForPort(DoubleAddress rtuDoubleAddress, int opticalPort)
        {
            var result = new List<Test>();
            var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress);
            if (listOfTestLinks == null) return result;

            foreach (var testLink in listOfTestLinks.Items)
            {
                var test = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.Self);
                if (test?.OtauPort != null && test.OtauPort.PortIndex == opticalPort - 1)
                    result.Add(test);
            }

            return result;
        }

        public async Task<Test> GetTestForPortAndBaseType(DoubleAddress rtuDoubleAddress, int opticalPort, string baseType)
        {
            var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress); 
            if (listOfTestLinks == null) return null;

            foreach (var testLink in listOfTestLinks.Items)
            {
                var test = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.Self);
                if (test?.OtauPort != null && test.OtauPort.PortIndex == opticalPort - 1 && test.Name.Contains(baseType))
                    return test;
            }

            return null;
        }

        public async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress, string otdrId, string otauId, int portIndex, BaseRefDto dto)
        {
            var testName = $@"Port { portIndex }, {
                    dto.BaseRefType.ToString().ToLower()}, created at {
                    DateTime.Now.ToString(CultureInfo.DefaultThreadCurrentUICulture)}";

            var newTest = new CreateTestCmd()
            {
                Id = Guid.NewGuid().ToString(),
                Name = testName,
                State = "disabled",
                OtdrId = otdrId,
                VeexOtauPort = new VeexOtauPort() { OtauId = otauId, PortIndex = portIndex - 1 }, 
                Period = 0, 
            };
            return await _d2RtuVeexLayer1.CreateTest(rtuDoubleAddress, newTest);
        }

        private async Task<bool> ChangeTestState(DoubleAddress rtuAddresses, Test test, string state)
        {
            return await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.Id}", new Test() { State = state });
        }

        private async Task<bool> ChangeTestPeriod(DoubleAddress rtuAddresses, Test test, int period)
        {
            return await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.Id}", new Test() { Period = period });
        }

    }
}
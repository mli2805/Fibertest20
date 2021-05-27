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
            foreach (var testLink in listOfTestLinks.items)
            {
                var test = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.self);
                if (test?.otauPort != null && test.otauPort.portIndex == opticalPort - 1)
                {
                    var deleteResult = await _d2RtuVeexLayer1.DeleteTest(rtuDoubleAddress, testLink.self);
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

            foreach (var testLink in listOfTestLinks.items)
            {
                var test = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.self);
                if (test?.otauPort != null 
                    && test.otauPort.portIndex == opticalPort - 1 
                    && test.name.Contains(baseType))
                    return await _d2RtuVeexLayer1.DeleteTest(rtuDoubleAddress, testLink.self);
            }
            return true;
        }

        public async Task<List<Test>> GetTestsForPort(DoubleAddress rtuDoubleAddress, int opticalPort)
        {
            var result = new List<Test>();
            var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress);
            if (listOfTestLinks == null) return result;

            foreach (var testLink in listOfTestLinks.items)
            {
                var test = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.self);
                if (test?.otauPort != null && test.otauPort.portIndex == opticalPort - 1)
                    result.Add(test);
            }

            return result;
        }

        public async Task<Test> GetTestForPortAndBaseType(DoubleAddress rtuDoubleAddress, int opticalPort, string baseType)
        {
            var listOfTestLinks = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress); 
            if (listOfTestLinks == null) return null;

            foreach (var testLink in listOfTestLinks.items)
            {
                var test = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.self);
                if (test?.otauPort != null && test.otauPort.portIndex == opticalPort - 1 && test.name.Contains(baseType))
                    return test;
            }

            return null;
        }

        public async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress, string otdrId, string otauId, int portIndex, BaseRefDto dto)
        {
            var testName = $@"Port { portIndex }, {
                    dto.BaseRefType.ToString().ToLower()}, created {
                    DateTime.Now.ToString(CultureInfo.DefaultThreadCurrentUICulture)}";

            var newTest = new CreateTestCmd()
            {
                id = Guid.NewGuid().ToString(),
                name = testName,
                state = "disabled",
                otdrId = otdrId,
                otauPort = new OtauPort() { otauId = otauId, portIndex = portIndex - 1 }, 
                period = 0, 
            };
            return await _d2RtuVeexLayer1.CreateTest(rtuDoubleAddress, newTest);
        }

        private async Task<bool> ChangeTestState(DoubleAddress rtuAddresses, Test test, string state)
        {
            return await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { state = state });
        }

        private async Task<bool> ChangeTestPeriod(DoubleAddress rtuAddresses, Test test, int period)
        {
            return await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { period = period });
        }

    }
}
﻿using System;
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

        public async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress, string otdrId, int portIndex, BaseRefDto dto)
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
                otauPort = new OtauPort() { otauId = otdrId, portIndex = portIndex }, //
                period = 0, // null in the future
            };
            return await _d2RtuVeexLayer1.CreateTest(rtuDoubleAddress, newTest);
        }

        public async Task<bool> ChangeTestState(Test test, DoubleAddress rtuAddresses, string state)
        {
            return await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { state = state });
        }

        public async Task<bool> ChangeTestPeriod(Test test, DoubleAddress rtuAddresses, int periodForPrecise)
        {
            return await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { period = periodForPrecise });
        }

        public async Task<bool> ChangeTestName(Test test, DoubleAddress rtuAddresses, string newName)
        {
            return await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { name = newName });
        }

    }
}
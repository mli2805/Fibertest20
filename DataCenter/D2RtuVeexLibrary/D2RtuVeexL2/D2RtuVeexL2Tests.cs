using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<bool> DeleteAllTests(DoubleAddress rtuDoubleAddress)
        {
            var getResult = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress);
            if (!getResult.IsSuccessful)
                return false;
            var listOfTestLinks = (LinkList)getResult.ResponseObject;
            if (listOfTestLinks == null) return true;
            foreach (var testLink in listOfTestLinks.items)
            {
                var deleteResult = await _d2RtuVeexLayer1.DeleteTest(rtuDoubleAddress, testLink.self);
                if (!deleteResult.IsSuccessful)
                    return false;
            }
            return true;
        }

        public async Task<HttpRequestResult> DeleteTestForPortAndBaseType(DoubleAddress rtuDoubleAddress, List<VeexOtauPort> otauPorts, string baseType)
        {
            var getResult = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress);
            if (!getResult.IsSuccessful)
                return getResult;
            var listOfTestLinks = (LinkList)getResult.ResponseObject;
            if (listOfTestLinks == null || listOfTestLinks.items == null || listOfTestLinks.items.Count == 0)
                return new HttpRequestResult() { IsSuccessful = true };

            foreach (var testLink in listOfTestLinks.items)
            {
                var getRes = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.self);
                if (!getRes.IsSuccessful) continue;

                var test = (Test)getRes.ResponseObject;
                if (test.otauPorts.IsEqual(otauPorts)
                    && test.name.Contains(baseType))
                {
                    var deleteTestRelationsRes = await DeleteTestRelations(rtuDoubleAddress, test);
                    if (!deleteTestRelationsRes.IsSuccessful)
                        return deleteTestRelationsRes;

                    var deleteTestRes = await _d2RtuVeexLayer1.DeleteTest(rtuDoubleAddress, testLink.self);
                    deleteTestRes.ResponseObject = test.id;
                    return deleteTestRes; // there is only one test for this port and base type
                }
            }

            // such a test not found
            return new HttpRequestResult() { IsSuccessful = true };
        }

        public async Task<HttpRequestResult> DeleteTestRelations(DoubleAddress rtuDoubleAddress, Test test)
        {
            foreach (var relation in test.relations.items)
            {
                var deleteRes = await _d2RtuVeexLayer1.DeleteRelation(rtuDoubleAddress, relation.id);
                if (!deleteRes.IsSuccessful)
                    return deleteRes;
            }

            return new HttpRequestResult() { IsSuccessful = true };
        }

        private async Task<Test> GetTestForPortAndBaseType(DoubleAddress rtuDoubleAddress, List<VeexOtauPort> otauPorts, string baseType)
        {
            var getResult = await _d2RtuVeexLayer1.GetTests(rtuDoubleAddress);
            if (!getResult.IsSuccessful)
                return null;
            var listOfTestLinks = (LinkList)getResult.ResponseObject;
            if (listOfTestLinks == null) return null;

            foreach (var testLink in listOfTestLinks.items)
            {
                var getRes = await _d2RtuVeexLayer1.GetTest(rtuDoubleAddress, testLink.self);
                if (!getRes.IsSuccessful) continue;

                var test = (Test)getRes.ResponseObject;
                if (test.otauPorts.IsEqual(otauPorts) && test.name.Contains(baseType))
                    return test;
            }

            return null;
        }

        private async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress,
            string otdrId, List<VeexOtauPort> otauPorts, BaseRefType baseRefType)
        {
            var testName = $@"Port { otauPorts.PortName() }, {
                    baseRefType.ToString().ToLower()}, created at {
                    DateTime.Now.ToString(CultureInfo.DefaultThreadCurrentUICulture)}";

            var newTest = new Test()
            {
                id = Guid.NewGuid().ToString(),
                name = testName,
                state = "disabled",
                otdrId = otdrId,
                otauPorts = otauPorts,
                period = 0,
                failedPeriod = baseRefType == BaseRefType.Fast ? int.MaxValue : 0,
            };
            return await _d2RtuVeexLayer1.CreateTest(rtuDoubleAddress, newTest);
        }

        public async Task<HttpRequestResult> AddRelation(DoubleAddress rtuDoubleAddress, Test fastTest,
            Test preciseTest)
        {
            var relation = new TestsRelation()
            {
                id = Guid.NewGuid().ToString(),
                testAId = fastTest.id,
                testBId = preciseTest.id,
            };

            return await _d2RtuVeexLayer1.AddTestsRelation(rtuDoubleAddress, relation);
        }

        private async Task<bool> ChangeTestState(DoubleAddress rtuAddresses, Test test, string state)
        {
            return (await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.id}", new Test() { state = state })).IsSuccessful;
        }

        private async Task<bool> ChangeTestPeriod(DoubleAddress rtuAddresses, Test test, int period, int failedPeriod)
        {
            return (await _d2RtuVeexLayer1.ChangeTest(rtuAddresses, $@"tests/{test.id}",
                new Test() { period = period, failedPeriod = failedPeriod })).IsSuccessful;
        }

    }
}
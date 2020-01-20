using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<BaseRefAssignedDto> AssignBaseRefAsync(AssignBaseRefsDto dto, DoubleAddress rtuAddresses)
        {
            try
            {
                var testsForPort = await GetTestsForPort(rtuAddresses, dto.OtauPortDto.OpticalPort);
                foreach (var baseRefDto in dto.BaseRefs)
                {
                    Test test = testsForPort.FirstOrDefault(t => t.name.Contains(baseRefDto.BaseRefType.ToString().ToLower()));
                    if (baseRefDto.Id == Guid.Empty) // it is command to delete such a base ref
                    {
                        if (test != null)
                        {
                            var unused = await DeleteTest(rtuAddresses, $@"tests/{test.id}");
                        }
                        continue;
                    }

                    var testName = $@"Port { dto.OtauPortDto.OpticalPort}, {
                            baseRefDto.BaseRefType.ToString().ToLower()}, created {
                            DateTime.Now.ToString(CultureInfo.DefaultThreadCurrentUICulture)}";

                    string testLink = test == null // if there is no test - create it
                        ? await CreateTest(rtuAddresses, testName, dto)
                        : await ChangeTestName(rtuAddresses, $@"tests/{test.id}", testName);

                    // assign Reference to Test
                    SetBaseRef(rtuAddresses, testLink, baseRefDto.SorBytes);
                }

                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
            }
            catch (Exception e)
            {
                return new BaseRefAssignedDto()
                {
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                    ExceptionMessage = e.Message
                };
            }
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

        private async Task<string> CreateTest(DoubleAddress rtuDoubleAddress, string name, AssignBaseRefsDto dto)
        {
            var newTest = new CreateTestCmd()
            {
                id = Guid.NewGuid().ToString(),
                name = name,
                state = "disabled",
                otdrId = dto.OtdrId,
                otauPort = new OtauPort() { otauId = dto.OtauPortDto.OtauId, portIndex = dto.OtauPortDto.OpticalPort - 1 }, //
                period = 0, // null in the future
            };
            var result = await _d2RtuVeexMonitoring.CreateTest(rtuDoubleAddress, newTest);
            if (result.HttpStatusCode != HttpStatusCode.Created)
                throw new Exception(result.ErrorMessage);
            return result.ResponseJson;
        }

        private async Task<string> ChangeTestName(DoubleAddress rtuDoubleAddress, string testUri, string newName)
        {
            var test = new Test() { name = newName, };
            var result = await _d2RtuVeexMonitoring.ChangeTest(rtuDoubleAddress, testUri, test);
            if (result.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception(result.ErrorMessage + ";  " + result.ResponseJson);
            return testUri;
        }

        private async Task<int> DeleteTest(DoubleAddress rtuDoubleAddress, string testUri)
        {
            var result = await _d2RtuVeexMonitoring.DeleteTest(rtuDoubleAddress, testUri);
            if (result.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception(result.ErrorMessage + ";  " + result.ResponseJson);
            return 0;
        }

        private async void SetBaseRef(DoubleAddress rtuDoubleAddress, string testLink, byte[] sorBytes)
        {
            var assignResult = await _d2RtuVeexMonitoring.SetBaseRef(rtuDoubleAddress, $@"monitoring/{testLink}/references", sorBytes);
            if (assignResult.HttpStatusCode != HttpStatusCode.Created)
                throw new Exception(assignResult.ErrorMessage + ";  " + assignResult.ResponseJson);
        } }
}

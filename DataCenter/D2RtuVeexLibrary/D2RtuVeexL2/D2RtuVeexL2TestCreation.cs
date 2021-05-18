using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<BaseRefAssignedDto> FullTestCreation(DoubleAddress rtuDoubleAddress, string otdrId, int portIndex, BaseRefDto dto)
        {
            var thresholds = dto.SorBytes.ExtractThresholds();
            if (thresholds == null) return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignmentNoThresholds };

            var createResult = await CreateTest(rtuDoubleAddress, otdrId, portIndex, dto);
            if (createResult.HttpStatusCode != HttpStatusCode.Created)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = createResult.ErrorMessage };
            var testLink = createResult.ResponseJson;

            var setBaseResult = await SetBaseRef(rtuDoubleAddress, $@"monitoring/{testLink}/references", dto.SorBytes);
            if (setBaseResult.HttpStatusCode != HttpStatusCode.Created)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };

            var setThresholdResult =
                await SetThresholds(rtuDoubleAddress, $@"monitoring/{testLink}/thresholds",
                    thresholds);
            if (setThresholdResult.HttpStatusCode != HttpStatusCode.OK)
                return new BaseRefAssignedDto()
                { ReturnCode = ReturnCode.BaseRefAssignmentFailed, ErrorMessage = setBaseResult.ErrorMessage };

            return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
        }

      
        private async Task<HttpRequestResult> CreateTest(DoubleAddress rtuDoubleAddress, string otdrId, int portIndex, BaseRefDto dto)
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
            return await CreateTest(rtuDoubleAddress, newTest);
        }
    }
}

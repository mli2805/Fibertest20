using System;
using System.Collections.Generic;
using System.Linq;
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
                var res = await RemovalPart(dto, rtuAddresses);
                if (res.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
                    return res;

                if (dto.BaseRefs.Any(b => b.Id != Guid.Empty))
                {
                    var creationRes = await CreationPart(dto, rtuAddresses);
                    creationRes.RemoveVeexTests = res.RemoveVeexTests;
                    return creationRes;
                }

                return res;
            }
            catch (Exception e)
            {
                return new BaseRefAssignedDto()
                {
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                    ErrorMessage = e.Message
                };
            }
        }

        private async Task<BaseRefAssignedDto> CreationPart(AssignBaseRefsDto dto, DoubleAddress rtuDoubleAddress)
        {
            var fast = await GetOrCreateTestWithBase(rtuDoubleAddress, dto, BaseRefType.Fast);
            if (fast.Test == null)
                return fast.ResultWhenFailed;

            var secondRefType = dto.BaseRefs.Any(b => b.BaseRefType == BaseRefType.Additional && b.Id != Guid.Empty)
                ? BaseRefType.Additional
                : BaseRefType.None;
            var precise = await GetOrCreateTestWithBase(rtuDoubleAddress, dto, BaseRefType.Precise, secondRefType);
            if (precise.Test == null)
                return precise.ResultWhenFailed;

            var baseRefAssignedDto = await SetRelationship(rtuDoubleAddress, fast.Test, precise.Test);
            if (baseRefAssignedDto == null)
                return new BaseRefAssignedDto
                {
                    ReturnCode = ReturnCode.BaseRefAssignedSuccessfully,
                    AddVeexTests = new List<VeexTestCreatedDto>() { Create(fast, dto), Create(precise, dto), }
                };

            return baseRefAssignedDto;
        }

        private VeexTestCreatedDto Create(TestCreationResult testCreationResult, AssignBaseRefsDto dto)
        {
            return new VeexTestCreatedDto
            {
                TestId = Guid.Parse(testCreationResult.Test.id),
                BasRefType = testCreationResult.BasRefType,
                TraceId = dto.TraceId,
                IsOnBop = !dto.OtauPortDto.IsPortOnMainCharon,
                OtauId = dto.OtauPortDto.OtauId,
                CreationTimestamp = DateTime.Now,
            };
        }

        private async Task<TestCreationResult> GetOrCreateTestWithBase(DoubleAddress rtuDoubleAddress, AssignBaseRefsDto dto,
            BaseRefType baseRefType, BaseRefType baseRefType2 = BaseRefType.None)
        {
            var test = await _d2RtuVeexLayer2.GetOrCreateTest(rtuDoubleAddress,
                dto.OtdrId, VeexPortExt.Create(dto.OtauPortDto, dto.MainOtauPortDto), baseRefType);

            var result = new TestCreationResult() { Test = test, BasRefType = baseRefType };
            if (test == null)
            {
                result.ResultWhenFailed = new BaseRefAssignedDto()
                {
                    BaseRefType = baseRefType,
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                    ErrorMessage = "Failed to get or create test!"
                };
                return result;
            }

            var baseRefDto = dto.BaseRefs.FirstOrDefault(b => b.BaseRefType == baseRefType);
            var baseRefDto2 = dto.BaseRefs.FirstOrDefault(b => b.BaseRefType == baseRefType2);

            var setBaseResult =
                await _d2RtuVeexLayer2.SetBaseRefsForTest(rtuDoubleAddress, $"tests/{test.id}", baseRefDto, baseRefDto2);
            if (setBaseResult.ReturnCode != ReturnCode.BaseRefAssignedSuccessfully)
            {
                result.Test = null;
                setBaseResult.BaseRefType = baseRefType;
                result.ResultWhenFailed = setBaseResult;
            }

            return result;
        }

        private async Task<BaseRefAssignedDto> SetRelationship(DoubleAddress rtuAddresses, Test fastTest, Test preciseTest)
        {
            if (!(await _d2RtuVeexLayer2.DeleteTestRelations(rtuAddresses, fastTest)).IsSuccessful)
                return new BaseRefAssignedDto()
                {
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                    ErrorMessage = "Failed to delete old relation",
                };

            var relationRes = await _d2RtuVeexLayer2.AddRelation(rtuAddresses, fastTest, preciseTest);
            if (!relationRes.IsSuccessful)
                return new BaseRefAssignedDto()
                {
                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                    ErrorMessage = "Failed to create tests relation" + Environment.NewLine + relationRes.ErrorMessage,
                };

            return null;
        }

        private async Task<BaseRefAssignedDto> RemovalPart(AssignBaseRefsDto dto, DoubleAddress rtuAddresses)
        {
            var testIds = new List<string>();
            foreach (var baseRefDto in dto.BaseRefs.Where(b => b.Id == Guid.Empty))
            {
                var deleteRes = await _d2RtuVeexLayer2
                    .DeleteTestForPortAndBaseType(rtuAddresses,
                        VeexPortExt.Create(dto.OtauPortDto, dto.MainOtauPortDto),
                        baseRefDto.BaseRefType.ToString().ToLower());

                if (deleteRes.IsSuccessful)
                    testIds.Add((string)deleteRes.ResponseObject);
                else
                    return new BaseRefAssignedDto()
                    {
                        BaseRefType = baseRefDto.BaseRefType,
                        ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                        ErrorMessage = "Failed to delete old base ref",
                    };
            }

            return new BaseRefAssignedDto()
            {
                ReturnCode = ReturnCode.BaseRefAssignedSuccessfully,
                RemoveVeexTests = testIds.Select(Guid.Parse).ToList(),
            };
        }
    }
}

using System;
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
                var testsForPort = await _d2RtuVeexLayer2.GetTestsForPort(rtuAddresses, dto.OtauPortDto.OpticalPort);
                foreach (var baseRefDto in dto.BaseRefs)
                {
                    Test test = testsForPort.FirstOrDefault(t => t.name.Contains(baseRefDto.BaseRefType.ToString().ToLower()));
                    if (baseRefDto.Id == Guid.Empty) // it is command to delete such a base ref
                    {
                        if (test != null)
                        {
                            if (! await _d2RtuVeexLayer1.DeleteTest(rtuAddresses, $@"tests/{test.id}"))
                                return new BaseRefAssignedDto()
                                {
                                    ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                                };
                        }
                        continue;
                    }

                    var testName = $@"Port { dto.OtauPortDto.OpticalPort}, {
                            baseRefDto.BaseRefType.ToString().ToLower()}, created {
                            DateTime.Now.ToString(CultureInfo.DefaultThreadCurrentUICulture)}";

                    string testLink;

                    if (test == null) // if there is no test - create it
                    {
                        var createResult = await _d2RtuVeexLayer2.CreateTest(rtuAddresses, dto.OtdrId, dto.OtauPortDto.OpticalPort, baseRefDto);
                        if (createResult.HttpStatusCode == HttpStatusCode.Created)
                            testLink = createResult.ResponseJson;
                        else
                            return new BaseRefAssignedDto()
                            {
                                ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                                ErrorMessage = createResult.ErrorMessage
                            }; 
                    }
                    else
                    {
                        testLink = $@"tests/{test.id}";
                        await _d2RtuVeexLayer2.ChangeTestName(test, rtuAddresses, testName);
                    }

                    // assign Reference to Test
                    var assignResult = await _d2RtuVeexLayer1.SetBaseRef(rtuAddresses, $@"monitoring/{testLink}/references", baseRefDto.SorBytes);
                    if (assignResult.HttpStatusCode != HttpStatusCode.Created)
                        return new BaseRefAssignedDto()
                        {
                            ReturnCode = ReturnCode.BaseRefAssignmentFailed,
                            ErrorMessage = assignResult.ErrorMessage
                        };  
                }

                return new BaseRefAssignedDto() { ReturnCode = ReturnCode.BaseRefAssignedSuccessfully };
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
    }
}

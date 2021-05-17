using System;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<bool> ReSendBaseRefsAsync(DoubleAddress rtuAddresses, ReSendBaseRefsDto dto)
        {
            try
            {
                var oldTests = await GetTestsForPort(rtuAddresses, dto.OtauPortDto.OpticalPort);
                foreach (var oldTest in oldTests)
                {
                    var delResult = await _d2RtuVeexLayer2.DeleteTest(rtuAddresses, $@"tests/{oldTest.id}");
                    if (delResult.HttpStatusCode != HttpStatusCode.NoContent)
                        return false;
                }

                foreach (var baseRefDto in dto.BaseRefDtos)
                {
                    
                }


                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

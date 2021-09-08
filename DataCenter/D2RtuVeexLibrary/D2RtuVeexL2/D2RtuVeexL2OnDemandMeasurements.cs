using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<ClientMeasurementStartedDto> DoMeasurementRequest(DoubleAddress rtuDoubleAddress, VeexMeasurementRequest dto)
        {
            var res = await _d2RtuVeexLayer1.DoMeasurementRequest(rtuDoubleAddress, dto);
            if (res.HttpStatusCode != HttpStatusCode.Created)
                return new ClientMeasurementStartedDto()
                {
                    ReturnCode = ReturnCode.Error,
                    ErrorMessage = res.ErrorMessage + System.Environment.NewLine + res.ResponseJson,
                };
            return new ClientMeasurementStartedDto() { ReturnCode = ReturnCode.Ok };
        }
    }
}

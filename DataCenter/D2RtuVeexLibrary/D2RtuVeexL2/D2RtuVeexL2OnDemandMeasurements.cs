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

        public async Task<ClientMeasurementDto> GetMeasurementClientResult(DoubleAddress rtuDoubleAddress, string measId)
        {
            var veexMeasurementResult = await _d2RtuVeexLayer1.GetMeasurementResult(rtuDoubleAddress, measId);
            if (veexMeasurementResult == null)
                return new ClientMeasurementDto()
                {
                    ReturnCode = ReturnCode.Error,
                    ErrorMessage = "Failed to get measurement result!",
                };

            var result = new ClientMeasurementDto()
            {
                ReturnCode = ReturnCode.Ok,
                VeexMeasurementStatus = veexMeasurementResult.status,
                ErrorMessage = veexMeasurementResult.extendedStatus,
            };

            if (veexMeasurementResult.status == "finished")
            {
                var bytes = await _d2RtuVeexLayer1.GetMeasurementBytes(rtuDoubleAddress, measId);
                if (bytes == null)
                {
                    result.ReturnCode = ReturnCode.Error;
                    result.ErrorMessage = "Failed to get measurement bytes";
                }
                else
                {
                    result.SorBytes = bytes;
                }

            }
            return result;
        }

        public async Task<RequestAnswer> PrepareReflectMeasurementAsync(DoubleAddress rtuDoubleAddress,
            PrepareReflectMeasurementDto dto)
        {
            var otdrRes = await _d2RtuVeexLayer1.ChangeProxyMode(rtuDoubleAddress, dto.OtdrId, true);
            if (otdrRes.HttpStatusCode != HttpStatusCode.NoContent)
                return new RequestAnswer()
                    {ReturnCode = ReturnCode.Error, ErrorMessage = "Failed to enable proxy mode!"};

            var toggleRes = await _d2RtuVeexLayer1.SwitchOtauToPort(rtuDoubleAddress, dto.OtauId, dto.PortNumber);
            if (toggleRes.HttpStatusCode != HttpStatusCode.NoContent)
                return new RequestAnswer()
                    {ReturnCode = ReturnCode.Error, ErrorMessage = "Failed to switch otau to port!"};

            return new RequestAnswer() {ReturnCode = ReturnCode.Ok};
        }
    }
}

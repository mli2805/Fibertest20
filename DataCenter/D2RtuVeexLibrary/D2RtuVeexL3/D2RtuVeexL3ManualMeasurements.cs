using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<ClientMeasurementStartedDto> StartMeasurementClientAsync
                    (DoubleAddress rtuDoubleAddress, DoClientMeasurementDto dto)
        {
            var proxy = await _d2RtuVeexLayer2.DisableProxyMode(rtuDoubleAddress, dto.OtdrId);
            if (!proxy.IsSuccessful)
                return new ClientMeasurementStartedDto
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    ErrorMessage = "Failed to disable proxy mode!" + Environment.NewLine + proxy.ErrorMessage,
                };

            var request = new VeexMeasurementRequest()
            {
                id = Guid.NewGuid().ToString(),
                otdrId = dto.OtdrId,
                otdrParameters = dto.VeexMeasOtdrParameters,
                otauPorts = CreateVeexOtauPortList(dto.OtauPortDto, dto.MainOtauPortDto),
                suspendMonitoring = true,
            };
            var res = await _d2RtuVeexLayer2.DoMeasurementRequest(rtuDoubleAddress, request);
            if (res.ReturnCode == ReturnCode.Ok)
                res.ErrorMessage = request.id; // sorry for this
            return res;
        }

        public async Task<ClientMeasurementVeexResultDto> GetMeasurementClientResultAsync(DoubleAddress rtuDoubleAddress,
            string measId)
        {
            return await _d2RtuVeexLayer2.GetMeasurementClientResult(rtuDoubleAddress, measId);
        }

        public async Task<HttpRequestResult> GetClientMeasurementSorBytesAsync(DoubleAddress rtuDoubleAddress,
                  string measId)
        {
            return await _d2RtuVeexLayer2.GetClientMeasurementSorBytesAsync(rtuDoubleAddress, measId);
        }

        public async Task<RequestAnswer> PrepareReflectMeasurementAsync(DoubleAddress rtuDoubleAddress,
            PrepareReflectMeasurementDto dto)
        {
            var otauPorts = CreateVeexOtauPortList(dto.OtauPortDto, dto.MainOtauPortDto);
            return await _d2RtuVeexLayer2.PrepareReflectMeasurement(rtuDoubleAddress, dto.OtdrId, otauPorts);
        }

        public async Task<RequestAnswer> StartOutOfTurnPreciseMeasurementAsync(DoubleAddress rtuDoubleAddress, string otdrId, string testId)
        {
            var result = await _d2RtuVeexLayer2.StartOutOfTurnPreciseMeasurement(rtuDoubleAddress, otdrId, testId);
            return new RequestAnswer()
            { ReturnCode = result.ReturnCode, ErrorMessage = result.ErrorMessage };
        }


        /// <summary>
        /// IIT RTU Manager expects only dto.OtauPortDto filled
        ///
        /// for VEEX RTU dto.MainOtauPortDto added
        ///
        /// if port is on main otau then MainOtauPortDto is null and OtauPortDto.IsPortOnMainCharon = true
        ///
        /// if port is on additional otau
        ///    OtauPortDto contains additional otau parameters
        ///     and MainOtauPortDto contains main otau parameters
        /// </summary>
        /// <param name="otauPortDto"></param>
        /// <param name="mainOtauPortDto"></param>
        /// <returns></returns>
        private static List<VeexOtauPort> CreateVeexOtauPortList(OtauPortDto otauPortDto, OtauPortDto mainOtauPortDto)
        {
            var otauPorts = new List<VeexOtauPort>();
            if (otauPortDto.IsPortOnMainCharon)
            {
                otauPorts.Add(new VeexOtauPort()
                {
                    otauId = otauPortDto.OtauId,
                    portIndex = otauPortDto.OpticalPort - 1
                });
                return otauPorts;
            }

            // else

            otauPorts.Add(new VeexOtauPort()
            {
                otauId = mainOtauPortDto.OtauId,
                portIndex = mainOtauPortDto.OpticalPort - 1
            });
            otauPorts.Add(new VeexOtauPort()
            {
                otauId = @"S2_" + otauPortDto.OtauId,
                portIndex = otauPortDto.OpticalPort - 1
            });

            return otauPorts;
        }
    }
}
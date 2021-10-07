using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3 : ID2RtuVeexL3111
    {
        private readonly D2RtuVeexLayer2 _d2RtuVeexLayer2;

        public D2RtuVeexLayer3(D2RtuVeexLayer2 d2RtuVeexLayer2)
        {
            _d2RtuVeexLayer2 = d2RtuVeexLayer2;
        }


        public async Task<ClientMeasurementStartedDto> StartMeasurementClient(DoubleAddress rtuDoubleAddress, DoClientMeasurementDto dto)
        {
            var veexOtauPort = new VeexOtauPort()
            {
                otauId = dto.OtauPortDto.OtauId,
                portIndex = dto.OtauPortDto.OpticalPort - 1
            };
            var otauPorts = new List<VeexOtauPort>();
            if (!dto.OtauPortDto.IsPortOnMainCharon)
            {
                otauPorts.Add(new VeexOtauPort()
                {
                    otauId = dto.MainOtauPortDto.OtauId,
                    portIndex = dto.MainOtauPortDto.OpticalPort - 1
                });
            }
            otauPorts.Add(veexOtauPort);
            var request = new VeexMeasurementRequest()
            {
                id = Guid.NewGuid().ToString(),
                otdrId = dto.OtdrId,
                otdrParameters = dto.VeexMeasOtdrParameters,
                otauPorts = otauPorts,
                analysisParameters = new AnalysisParameters()
                {
                    lasersParameters = new List<LasersParameter> { new LasersParameter() }
                },
                spanParameters = new SpanParameters(),
                generalParameters = new GeneralParameters(),

                suspendMonitoring = true,
            };

            var clientMeasurementStartedDto = await _d2RtuVeexLayer2.DoMeasurementRequest(rtuDoubleAddress, request);
            if (clientMeasurementStartedDto.ReturnCode == ReturnCode.Ok)
                clientMeasurementStartedDto.ErrorMessage = request.id; // sorry for this
            return clientMeasurementStartedDto;
        }

        public async Task<ClientMeasurementDto> GetMeasurementClientResult(DoubleAddress rtuDoubleAddress, string measId)
        {
            return await _d2RtuVeexLayer2.GetMeasurementClientResult(rtuDoubleAddress, measId);
        }

        public async Task<RequestAnswer> PrepareReflectMeasurementAsync(DoubleAddress rtuDoubleAddress,
            PrepareReflectMeasurementDto dto)
        {
            return await _d2RtuVeexLayer2.PrepareReflectMeasurementAsync(rtuDoubleAddress, dto);

        }
    }
}
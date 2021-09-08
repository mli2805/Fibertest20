using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        private readonly D2RtuVeexLayer2 _d2RtuVeexLayer2;

        public D2RtuVeexLayer3(D2RtuVeexLayer2 d2RtuVeexLayer2)
        {
            _d2RtuVeexLayer2 = d2RtuVeexLayer2;
        }


        public async Task<ClientMeasurementStartedDto> StartMeasurementClient(DoubleAddress rtuDoubleAddress, DoClientMeasurementDto dto)
        {
            var request = new VeexMeasurementRequest()
            {
                id = Guid.NewGuid().ToString(),
                otdrId = dto.OtdrId,
                otdrParameters = dto.VeexMeasOtdrParameters,
                otauPorts = new List<VeexOtauPort>()
                {
                    new VeexOtauPort()
                    {
                        otauId = dto.OtauPortDto.OtauId,
                        portIndex = dto.OtauPortDto.OpticalPort - 1
                    }
                },
                analysisParameters = new AnalysisParameters(){ lasersParameters = new List<LasersParameter>()},
                spanParameters = new SpanParameters(),
            };

            return await _d2RtuVeexLayer2.DoMeasurementRequest(rtuDoubleAddress, request);
        }
    }
}
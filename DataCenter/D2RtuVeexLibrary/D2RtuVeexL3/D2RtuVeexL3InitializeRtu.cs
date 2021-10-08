using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer3
    {
        public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            try
            {
                var rtuAddresses = (DoubleAddress)dto.RtuAddresses.Clone();
                var rtuInitializedDto = await _d2RtuVeexLayer2.GetSettings(rtuAddresses, dto);
                if (!rtuInitializedDto.IsInitialized)
                    return rtuInitializedDto;

                var otauRes = await _d2RtuVeexLayer2.InitializeOtaus(rtuAddresses, dto);
                if (otauRes.IsSuccessful)
                    FillInOtau((VeexOtauInfo)otauRes.ResponseObject, dto, rtuInitializedDto);
                else
                    return new RtuInitializedDto
                    {
                        ReturnCode = ReturnCode.OtauInitializationError,
                        ErrorMessage = otauRes.ErrorMessage,
                    };

                var initRes = await _d2RtuVeexLayer2.InitializeMonitoringProperties(rtuAddresses);
                if (initRes != null)
                {
                    rtuInitializedDto.ErrorMessage = initRes.ErrorMessage;
                    rtuInitializedDto.ReturnCode = initRes.ReturnCode;
                    return rtuInitializedDto;
                }

                rtuInitializedDto.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
                return rtuInitializedDto;
            }
            catch (Exception e)
            {
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    ErrorMessage = e.Message,
                };
            }
        }

        private void FillInOtau(VeexOtauInfo otauInfo, InitializeRtuDto dto, RtuInitializedDto result)
        {
            if (otauInfo.OtauList.Count == 0)
            {
                result.OwnPortCount = 1;
                result.FullPortCount = 1;
                result.Children = new Dictionary<int, OtauDto>();
                return;
            }

            var mainOtauId = otauInfo.OtauScheme.rootConnections[0].inputOtauId;
            var mainOtau = otauInfo.OtauList.First(o => o.id == mainOtauId);

            result.OtauId = mainOtau.id;
            result.OwnPortCount = mainOtau.portCount;
            result.FullPortCount = mainOtau.portCount;
            result.Children = new Dictionary<int, OtauDto>();

            foreach (var childConnection in otauInfo.OtauScheme.connections)
            {
                var pair = dto.Children.First(c => "S2_" + c.Value.OtauId == childConnection.inputOtauId);
                result.Children.Add(pair.Key, pair.Value);

                result.FullPortCount += pair.Value.OwnPortCount - 1;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
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
                var rtuInitializedDto = await _d2RtuVeexLayer2.GetPlatformInfo(rtuAddresses, dto);
                if (!rtuInitializedDto.IsInitialized)
                    return rtuInitializedDto;

                var vesionRes = await _d2RtuVeexLayer2.DisableVesionIntegration(rtuAddresses);
                if (!vesionRes.IsSuccessful)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = vesionRes.ErrorMessage
                    };

                var otdrRes = await _d2RtuVeexLayer2.InitializeOtdrs(rtuAddresses);
                if (otdrRes.IsSuccessful)
                    FillInOtdr((VeexOtdr)otdrRes.ResponseObject, rtuInitializedDto);
                else
                    return new RtuInitializedDto()
                        { ReturnCode = ReturnCode.OtdrInitializationFailed, ErrorMessage = otdrRes.ErrorMessage };

                var otauRes = await _d2RtuVeexLayer2.InitializeOtaus(rtuAddresses, dto);
                if (otauRes.IsSuccessful)
                    FillInOtau((VeexOtauInfo)otauRes.ResponseObject, dto, rtuInitializedDto);
                else
                    return new RtuInitializedDto
                    {
                        ReturnCode = ReturnCode.OtauInitializationError,
                        ErrorMessage = otauRes.ErrorMessage,
                    };

                var proxy = await _d2RtuVeexLayer2.DisableProxyMode(rtuAddresses, rtuInitializedDto.OtdrId);
                if (!proxy.IsSuccessful)
                    return new RtuInitializedDto
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = "Failed to disable proxy mode!" + Environment.NewLine +  proxy.ErrorMessage,
                    };

                var getMoniPropResult = await _d2RtuVeexLayer2.GetMonitoringProperties(rtuAddresses);
                if (!getMoniPropResult.IsSuccessful)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = "Failed to get monitoring properties"
                    };

                var monitoringProperties = (VeexMonitoringDto)getMoniPropResult.ResponseObject;

                if (monitoringProperties.type != "fibertest" || dto.IsFirstInitialization)
                {
                    var initRes = await _d2RtuVeexLayer2.InitializeMonitoringProperties(rtuAddresses, monitoringProperties);
                    if (initRes != null)
                    {
                        rtuInitializedDto.ErrorMessage = initRes.ErrorMessage;
                        rtuInitializedDto.ReturnCode = initRes.ReturnCode;
                        return rtuInitializedDto;
                    }

                    monitoringProperties.state = "disabled";
                }

                rtuInitializedDto.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
                rtuInitializedDto.IsMonitoringOn = monitoringProperties.state == "enabled";
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

        private void FillInOtdr(VeexOtdr otdr, RtuInitializedDto result)
        {
            result.OtdrId = otdr.id;
            result.Omid = otdr.mainframeId;
            result.Omsn = otdr.opticalModuleSerialNumber;
            result.AcceptableMeasParams = new TreeOfAcceptableMeasParams();
            foreach (var laserUnitPair in otdr.supportedMeasurementParameters.laserUnits)
            {
                var branch = new BranchOfAcceptableMeasParams
                {
                    BackscatteredCoefficient = -81,
                    RefractiveIndex = 1.4682
                };
                foreach (var distancePair in laserUnitPair.Value.distanceRanges)
                {
                    var leaf = new LeafOfAcceptableMeasParams
                    {
                        Resolutions = distancePair.Value.resolutions,
                        PulseDurations = distancePair.Value.pulseDurations,
                        MeasCountsToAverage = distancePair.Value.fastAveragingTimes,
                        PeriodsToAverage = distancePair.Value.averagingTimes
                    };
                    branch.Distances.Add(distancePair.Key.ToString(CultureInfo.InvariantCulture), leaf);
                }

                result.AcceptableMeasParams.Units.Add(laserUnitPair.Key, branch);
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

            result.MainVeexOtau = mainOtau;

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

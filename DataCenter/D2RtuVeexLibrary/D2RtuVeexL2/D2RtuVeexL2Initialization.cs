using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<RtuInitializedDto> GetSettings(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var result = new RtuInitializedDto();

            var platformResponse = await _d2RtuVeexLayer1.GetPlatform(rtuDoubleAddress);
            if (!platformResponse.IsSuccessful)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };
            FillInPlatform((VeexPlatformInfo)platformResponse.ResponseObject, result);

            var otdrResponse = await GetOtdrSettings(rtuDoubleAddress);
            if (!otdrResponse.IsSuccessful)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };
            FillInOtdr((VeexOtdr)otdrResponse.ResponseObject, result);

            var otauRes = await GetOtauSettings(rtuDoubleAddress);
            if (otauRes.IsSuccessful)
                FillInOtau((VeexOtauInfo)otauRes.ResponseObject, result);
            else
                return new RtuInitializedDto
                {
                    ReturnCode = ReturnCode.OtauInitializationError,
                    ErrorMessage = otauRes.ErrorMessage,
                };

            result.RtuId = dto.RtuId;
            result.RtuAddresses = dto.RtuAddresses;
            result.OtdrAddress = (NetAddress)result.RtuAddresses.Main.Clone();
            result.Maker = RtuMaker.VeEX;
            result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;

            result.IsInitialized = true;
            return result;
        }

        private void FillInPlatform(VeexPlatformInfo info, RtuInitializedDto result)
        {
            result.Mfid = info.platform.name;
            result.Mfsn = info.platform.serialNumber;
            result.Serial = info.platform.serialNumber;
            result.Version = info.components.api;
            result.Version2 = info.components.otdrEngine.iit_otdr;
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

        private void FillInOtau(VeexOtauInfo otauInfo, RtuInitializedDto result)
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
                var childOtau = otauInfo.OtauList.First(c => c.id == childConnection.outputOtauId);
                result.Children.Add(childConnection.outputOtauPort, new OtauDto()
                {
                    OtauId = childOtau.id,
                    IsOk = true,
                });
                result.FullPortCount += childOtau.portCount - 1;
            }
        }

        public async Task<RtuInitializedDto> InitializeMonitoringProperties(DoubleAddress rtuDoubleAddress)
        {
            var getResult = await _d2RtuVeexLayer1.GetMonitoringProperties(rtuDoubleAddress);
            if (!getResult.IsSuccessful)
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    ErrorMessage = "Failed to get monitoring properties"
                };

            var monitoringProperties = (VeexMonitoringDto)getResult.ResponseObject;

            if (monitoringProperties.type == "fibertest") return null;

            if (monitoringProperties.state == "enabled")
            {
                var setStateRes = await SetMonitoringState(rtuDoubleAddress, "disabled");
                if (!setStateRes.IsSuccessful)
                    return new RtuInitializedDto()
                    {
                        ReturnCode = ReturnCode.RtuInitializationError,
                        ErrorMessage = setStateRes.ErrorMessage,
                    };
            }

            if (!await DeleteAllTests(rtuDoubleAddress))
            {
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    ErrorMessage = "Failed to delete existing tests",
                };
            }

            var setResult = await SetMonitoringTypeToFibertest(rtuDoubleAddress);
            if (!setResult.IsSuccessful)
            {
                return new RtuInitializedDto()
                {
                    ReturnCode = ReturnCode.RtuInitializationError,
                    ErrorMessage = setResult.ErrorMessage + System.Environment.NewLine + setResult.ResponseJson,
                };
            }
            return null;
        }

        public async Task<HttpRequestResult> SetServerNotificationSettings(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var serverNotificationSettings = new ServerNotificationSettings()
            {
                state = "enabled",
                eventTypes = new List<string>()
                {
                    "monitoring_test_failed",
                    "monitoring_test_passed"
                },
                url = $@"http://{dto.ServerAddresses.Main.ToStringA()}/veex/notify?rtuId={dto.RtuId}",
            };

            return await _d2RtuVeexLayer1.SetServerNotificationSettings(rtuDoubleAddress, serverNotificationSettings);
        }
    }
}

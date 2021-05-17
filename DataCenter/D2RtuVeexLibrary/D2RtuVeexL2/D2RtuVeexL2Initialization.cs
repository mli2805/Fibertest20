using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer2
    {
        public async Task<RtuInitializedDto> GetSettings(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
        {
            var result = new RtuInitializedDto();

            var platformResponse = await GetPlatformSettings(rtuDoubleAddress, result);
            if (!platformResponse)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };

            var otdrResponse = await GetOtdrSettings(rtuDoubleAddress, result);
            if (!otdrResponse)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };

            var otauResponse = await GetOtauSettings(rtuDoubleAddress, result);
            if (!otauResponse)
                return new RtuInitializedDto { ReturnCode = ReturnCode.OtauInitializationError };

            result.RtuId = dto.RtuId;
            result.RtuAddresses = dto.RtuAddresses;
            result.OtdrAddress = (NetAddress)result.RtuAddresses.Main.Clone();
            result.Maker = RtuMaker.VeEX;
            result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;

            var monitoringResponse = await GetMonitoringMode(rtuDoubleAddress, result);
            if (!monitoringResponse)
                return new RtuInitializedDto { ReturnCode = ReturnCode.RtuInitializationError };

            result.IsInitialized = true;
            return result;
        }

        private async Task<bool> GetMonitoringMode(DoubleAddress rtuDoubleAddress, RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "monitoring", "get");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var monitoring = JsonConvert.DeserializeObject<MonitoringVeexDto>(httpResult.ResponseJson);
            result.IsMonitoringOn = monitoring.state == "enabled";
            return true;
        }

        private async Task<bool> GetOtauSettings(DoubleAddress rtuDoubleAddress, RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "otaus", "get");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otaus = JsonConvert.DeserializeObject<Otaus>(httpResult.ResponseJson);
            if (otaus.total == 0)
                return true;

            var httpResult2 = await _httpExt.RequestByUrl(rtuDoubleAddress, $"{otaus.items[0].self}", "get");
            if (httpResult2.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otau = JsonConvert.DeserializeObject<Otau>(httpResult2.ResponseJson);
            result.OtauId =otau.id;
            result.OwnPortCount = otau.portCount;
            result.FullPortCount = otau.portCount;
            result.Children = new Dictionary<int, OtauDto>(); // empty, no children
            return true;
        }

        private async Task<bool> GetOtdrSettings(DoubleAddress rtuDoubleAddress, RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "otdrs", "get");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otdrs = JsonConvert.DeserializeObject<Otdrs>(httpResult.ResponseJson);
            if (otdrs.total == 0)
                return true;

            var httpResult2 = await _httpExt.RequestByUrl(rtuDoubleAddress, $"{otdrs.items[0].self}", "get");
            if (httpResult2.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otdr = JsonConvert.DeserializeObject<Otdr>(httpResult2.ResponseJson);
            result.OtdrId = otdr.id;
            result.Omid = otdr.mainframeId;
            result.Omsn = otdr.opticalModuleSerialNumber;
            result.AcceptableMeasParams = new TreeOfAcceptableMeasParams();
            foreach (var laserUnitPair in otdr.supportedMeasurementParameters.laserUnits)
            {
                var branch = new BranchOfAcceptableMeasParams();
                foreach (var distancePair in laserUnitPair.Value.distanceRanges)
                {
                    var leaf = new LeafOfAcceptableMeasParams();
                    leaf.Resolutions = distancePair.Value.resolutions;
                    leaf.PulseDurations = distancePair.Value.pulseDurations;
                    leaf.MeasCountsToAverage = distancePair.Value.fastAveragingTimes;
                    leaf.PeriodsToAverage = distancePair.Value.averagingTimes;
                    branch.Distances.Add(distancePair.Key, leaf);
                }

                result.AcceptableMeasParams.Units.Add(laserUnitPair.Key, branch);
            }
            return true;
        }

        private async Task<bool> GetPlatformSettings(DoubleAddress rtuDoubleAddress, RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "info", "get");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
                return false;
            var info = JsonConvert.DeserializeObject<Info>(httpResult.ResponseJson);
            result.Mfid = info.platform.name;
            result.Mfsn = info.platform.serialNumber;
            result.Serial = info.platform.serialNumber;
            result.Version = info.platform.firmwareVersion;
            result.Version2 = info.platform.moduleFirmwareVersion;
            return true;
        }

        public async Task<HttpRequestResult> SetServerNotificationUrl(DoubleAddress rtuDoubleAddress, InitializeRtuDto dto)
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
            var jsonData = JsonConvert.SerializeObject(serverNotificationSettings);
            return await _httpExt.RequestByUrl(rtuDoubleAddress,
                $@"/notification/settings", "patch", "application/merge-patch+json", jsonData);
        }

    }
}

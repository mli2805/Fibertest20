using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<MonitoringVeexDto> GetMonitoringProperties(DoubleAddress rtuDoubleAddress)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "monitoring", "get");
            return httpResult.HttpStatusCode == HttpStatusCode.OK 
                ? JsonConvert.DeserializeObject<MonitoringVeexDto>(httpResult.ResponseJson) 
                : null;
        }

        public async Task<bool> GetOtauSettings(DoubleAddress rtuDoubleAddress, RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "otaus", "get");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otaus = JsonConvert.DeserializeObject<Otaus>(httpResult.ResponseJson);
            if (otaus == null || otaus.total == 0) return false;
            if (otaus.total == 0)
                return true;

            var httpResult2 = await _httpExt.RequestByUrl(rtuDoubleAddress, $"{otaus.items[0].self}", "get");
            if (httpResult2.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otau = JsonConvert.DeserializeObject<VeexOtau>(httpResult2.ResponseJson);
            if (otau == null) return false;
            result.OtauId = otau.id;
            result.OwnPortCount = otau.portCount;
            result.FullPortCount = otau.portCount;
            result.Children = new Dictionary<int, OtauDto>(); // empty, no children
            return true;
        }

        public async Task<bool> GetOtdrSettings(DoubleAddress rtuDoubleAddress, RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "otdrs", "get");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otdrs = JsonConvert.DeserializeObject<Otdrs>(httpResult.ResponseJson);
            if (otdrs == null) return false;
            if (otdrs.total == 0)
                return true;

            var httpResult2 = await _httpExt.RequestByUrl(rtuDoubleAddress, $"{otdrs.items[0].self}", "get");
            if (httpResult2.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otdr = JsonConvert.DeserializeObject<VeexOtdr>(httpResult2.ResponseJson);
            if (otdr == null) return false;
            result.OtdrId = otdr.id;
            result.Omid = otdr.mainframeId;
            result.Omsn = otdr.opticalModuleSerialNumber;
            result.AcceptableMeasParams = new TreeOfAcceptableMeasParams();
            foreach (var laserUnitPair in otdr.supportedMeasurementParameters.laserUnits)
            {
                var branch = new BranchOfAcceptableMeasParams();
                foreach (var distancePair in laserUnitPair.Value.distanceRanges)
                {
                    var leaf = new LeafOfAcceptableMeasParams
                    {
                        Resolutions = distancePair.Value.resolutions,
                        PulseDurations = distancePair.Value.pulseDurations,
                        MeasCountsToAverage = distancePair.Value.fastAveragingTimes,
                        PeriodsToAverage = distancePair.Value.averagingTimes
                    };
                    branch.Distances.Add(distancePair.Key, leaf);
                }

                result.AcceptableMeasParams.Units.Add(laserUnitPair.Key, branch);
            }
            return true;
        }

        public async Task<bool> GetPlatformSettings(DoubleAddress rtuDoubleAddress, RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "info", "get");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
                return false;
            var info = JsonConvert.DeserializeObject<Info>(httpResult.ResponseJson);
            if (info == null) return false;
            result.Mfid = info.platform.name;
            result.Mfsn = info.platform.serialNumber;
            result.Serial = info.platform.serialNumber;
            result.Version = info.components.api;
            result.Version2 = info.components.otdrEngine.iit_otdr;
            return true;
        }
    }
}

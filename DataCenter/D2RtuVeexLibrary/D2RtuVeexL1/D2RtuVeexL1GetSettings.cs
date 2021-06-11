using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public partial class D2RtuVeexLayer1
    {
        public async Task<bool> GetMonitoringMode(DoubleAddress rtuDoubleAddress, RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(rtuDoubleAddress, "monitoring", "get");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var monitoring = JsonConvert.DeserializeObject<MonitoringVeexDto>(httpResult.ResponseJson);
            if (monitoring == null) return false;
            result.IsMonitoringOn = monitoring.State == "enabled";
            return true;
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
            if (otaus == null) return false;
            if (otaus.Total == 0)
                return true;

            var httpResult2 = await _httpExt.RequestByUrl(rtuDoubleAddress, $"{otaus.Items[0].Self}", "get");
            if (httpResult2.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otau = JsonConvert.DeserializeObject<VeexOtau>(httpResult2.ResponseJson);
            if (otau == null) return false;
            result.OtauId = otau.Id;
            result.OwnPortCount = otau.PortCount;
            result.FullPortCount = otau.PortCount;
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
            if (otdrs.Total == 0)
                return true;

            var httpResult2 = await _httpExt.RequestByUrl(rtuDoubleAddress, $"{otdrs.Items[0].Self}", "get");
            if (httpResult2.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otdr = JsonConvert.DeserializeObject<VeexOtdr>(httpResult2.ResponseJson);
            if (otdr == null) return false;
            result.OtdrId = otdr.Id;
            result.Omid = otdr.MainframeId;
            result.Omsn = otdr.OpticalModuleSerialNumber;
            result.AcceptableMeasParams = new TreeOfAcceptableMeasParams();
            foreach (var laserUnitPair in otdr.SupportedMeasurementParameters.LaserUnits)
            {
                var branch = new BranchOfAcceptableMeasParams();
                foreach (var distancePair in laserUnitPair.Value.DistanceRanges)
                {
                    var leaf = new LeafOfAcceptableMeasParams
                    {
                        Resolutions = distancePair.Value.Resolutions,
                        PulseDurations = distancePair.Value.PulseDurations,
                        MeasCountsToAverage = distancePair.Value.FastAveragingTimes,
                        PeriodsToAverage = distancePair.Value.AveragingTimes
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
            result.Mfid = info.Platform.Name;
            result.Mfsn = info.Platform.SerialNumber;
            result.Serial = info.Platform.SerialNumber;
            result.Version = info.Platform.FirmwareVersion;
            result.Version2 = info.Platform.ModuleFirmwareVersion;
            return true;
        }
    }
}

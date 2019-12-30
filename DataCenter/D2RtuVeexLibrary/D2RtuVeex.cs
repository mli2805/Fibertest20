using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Newtonsoft.Json;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class D2RtuVeex
    {
        private readonly HttpExt _httpExt;
        private DoubleAddress _rtuDoubleAddress;

        public D2RtuVeex(HttpExt httpExt)
        {
            _httpExt = httpExt;
        }

        public async Task<RtuInitializedDto> GetSettings(InitializeRtuDto dto)
        {
            _rtuDoubleAddress = (DoubleAddress)dto.RtuAddresses.Clone();
            var result = new RtuInitializedDto();

            var platformResponse = await GetPlatformSettings(result);
            if (!platformResponse)
                return result;

            var otdrResponse = await GetOtdrSettings(result);
            if (!otdrResponse)
                return result;

            var otauResponse = await GetOtauSettings(result);
            if (!otauResponse)
                return result;

            result.RtuId = dto.RtuId;
            result.RtuAddresses = dto.RtuAddresses;
            result.OtdrAddress = (NetAddress)result.RtuAddresses.Main.Clone();
            result.Maker = RtuMaker.VeEX;
            result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
            result.IsInitialized = true;
            return result;
        }

        private async Task<bool> GetOtauSettings(RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(_rtuDoubleAddress, "get", "otaus");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otaus = JsonConvert.DeserializeObject<Otaus>(httpResult.ResponseJson);
            if (otaus.total == 0)
                return true;

            var httpResult2 = await _httpExt.RequestByUrl(_rtuDoubleAddress, "get", $"{otaus.items[0].self}");
            if (httpResult2.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otau = JsonConvert.DeserializeObject<Otau>(httpResult2.ResponseJson);
            result.OwnPortCount = otau.portCount;
            result.FullPortCount = otau.portCount;
            result.Children = new Dictionary<int, OtauDto>(); // empty, no children
            return true;
        }

        private async Task<bool> GetOtdrSettings(RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(_rtuDoubleAddress, "get", "otdrs");
            if (httpResult.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otdrs = JsonConvert.DeserializeObject<Otdrs>(httpResult.ResponseJson);
            if (otdrs.total == 0)
                return true;

            var httpResult2 = await _httpExt.RequestByUrl(_rtuDoubleAddress, "get", $"{otdrs.items[0].self}");
            if (httpResult2.HttpStatusCode != HttpStatusCode.OK)
            {
                result.ErrorMessage = httpResult.ErrorMessage;
                return false;
            }

            var otdr = JsonConvert.DeserializeObject<Otdr>(httpResult2.ResponseJson);
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

        private async Task<bool> GetPlatformSettings(RtuInitializedDto result)
        {
            var httpResult = await _httpExt.RequestByUrl(_rtuDoubleAddress, "get", "info");
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
    }
}

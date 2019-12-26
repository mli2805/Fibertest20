using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HttpLib;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace D2RtuVeexManager
{
    public class D2RtuVeex
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly IMyLog _logFile;
        private DoubleAddress _rtuDoubleAddress;

        public D2RtuVeex(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<RtuInitializedDto> GetSettings(InitializeRtuDto dto)
        {
            _rtuDoubleAddress = (DoubleAddress)dto.RtuAddresses.Clone();

            var result = new RtuInitializedDto();
            var rootUrl = $"http://{_rtuDoubleAddress.Main.ToStringA()}/api/v1";

            var platformResponse = await GetPlatformSettings(rootUrl, result);
            if (!platformResponse)
                return result;

            var otdrResponse = await GetOtdrSettings(rootUrl, result);
            if (!otdrResponse)
                return result;

            var otauResponse = await GetOtauSettings(rootUrl, result);
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

        private async Task<bool> GetOtauSettings(string rootUrl, RtuInitializedDto result)
        {
            try
            {
                var responseMessage = await _httpClient.GetAsync($"{rootUrl}/otaus");
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    result.ReturnCode = ReturnCode.OtauInitializationError;
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                    return false;
                }

                var strOtaus = await responseMessage.Content.ReadAsStringAsync();

                _logFile.AppendLine(strOtaus);
                var otaus = JsonConvert.DeserializeObject<Otaus>(strOtaus);

                if (otaus.total == 0)
                    return true;

                responseMessage = await _httpClient.GetAsync($"{rootUrl}/{otaus.items[0].self}"); // could be more than one
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    result.ReturnCode = ReturnCode.OtauInitializationError;
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                    return false;
                }
                var strOtau = await responseMessage.Content.ReadAsStringAsync(); 
                _logFile.AppendLine(strOtau);
                var otau = JsonConvert.DeserializeObject<Otau>(strOtau);
                result.OwnPortCount = otau.portCount;
                result.FullPortCount = otau.portCount;
                result.Children = new Dictionary<int, OtauDto>(); // empty, no children
                return true;
            }
            catch (Exception e)
            {
                result.ReturnCode = ReturnCode.OtauInitializationError;
                result.ErrorMessage = e.Message;
            }
            return false;
        }

        private async Task<bool> GetOtdrSettings(string rootUrl, RtuInitializedDto result)
        {
            try
            {
                var responseMessage = await _httpClient.GetAsync($"{rootUrl}/otdrs");
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    result.ReturnCode = ReturnCode.RtuInitializationError;
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                    return false;
                } 
                var strOtdrs = await responseMessage.Content.ReadAsStringAsync();
                _logFile.AppendLine(strOtdrs);
                var otdrs = JsonConvert.DeserializeObject<Otdrs>(strOtdrs);

                if (otdrs.total == 0)
                    return true;

                responseMessage = await _httpClient.GetAsync($"{rootUrl}/{otdrs.items[0].self}"); // could be more than one
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    result.ReturnCode = ReturnCode.RtuInitializationError;
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                    return false;
                } 
                var strOtdr = await responseMessage.Content.ReadAsStringAsync();
                _logFile.AppendLine(strOtdr);
                var otdr = JsonConvert.DeserializeObject<Otdr>(strOtdr);
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
                    return true;
                }
            }
            catch (Exception e)
            {
                result.ReturnCode = ReturnCode.RtuInitializationError;
                result.ErrorMessage = e.Message;
            }

            return false;
        }

        private async Task<bool> GetPlatformSettings(string rootUrl, RtuInitializedDto result)
        {
            try
            {
                var responseMessage = await _httpClient.GetAsync($"{rootUrl}/info");
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    result.ReturnCode = ReturnCode.RtuInitializationError;
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                    return false;
                } 
                var strInfo = await responseMessage.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<Info>(strInfo);
                _logFile.AppendLine(strInfo);
                result.Mfid = info.platform.name;
                result.Mfsn = info.platform.serialNumber;
                result.Serial = info.platform.serialNumber;
                result.Version = info.platform.firmwareVersion;
                result.Version2 = info.platform.moduleFirmwareVersion;
                return true;
            }
            catch (Exception e)
            {
                result.ReturnCode = ReturnCode.RtuInitializationError;
                result.ErrorMessage = e.Message;
            }
            return false;
        }

    }

}

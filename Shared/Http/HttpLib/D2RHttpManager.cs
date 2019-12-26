using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace HttpLib
{

    public class D2RHttpManager
    {
        private readonly D2RHttpClient _d2RHttpClient;
        private DoubleAddress _rtuAddress;
        private IMyLog _logFile;

        public D2RHttpManager(D2RHttpClient d2RHttpClient)
        {
            _d2RHttpClient = d2RHttpClient;
        }


        public void Initialize(DoubleAddress rtuAddress, IMyLog logFile)
        {
            _rtuAddress = rtuAddress;
            _logFile = logFile;
        }

        public async Task<RtuInitializedDto> GetSettings(InitializeRtuDto dto)
        {
            var result = new RtuInitializedDto();

            var rootUrl = $"http://{_rtuAddress.Main.ToStringA()}/api/v1";
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
                var strOtaus = await _d2RHttpClient.GetAsync($"{rootUrl}/otaus");
                _logFile.AppendLine(strOtaus);
                var otaus = JsonConvert.DeserializeObject<Otaus>(strOtaus);

                if (otaus.total == 0)
                    return true;

                var strOtau = await _d2RHttpClient.GetAsync($"{rootUrl}/{otaus.items[0].self}"); // could be more than one
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
                var strOtdrs = await _d2RHttpClient.GetAsync($"{rootUrl}/otdrs");
                _logFile.AppendLine(strOtdrs);
                var otdrs = JsonConvert.DeserializeObject<Otdrs>(strOtdrs);

                if (otdrs.total == 0)
                    return true;

                var strOtdr = await _d2RHttpClient.GetAsync($"{rootUrl}/{otdrs.items[0].self}"); // could be more than one
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
              //  var strInfo = await GetAsync($"{rootUrl}/info");
                var strInfo = await _d2RHttpClient.GetAsync($"{rootUrl}/info");
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

        private async Task<string> GetAsync(string uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            {
                if (stream == null) return null;
                using (StreamReader reader = new StreamReader(stream))
                {
                    return await reader.ReadToEndAsync();
                }

            }
        }

    }

    public class D2RHttpClient
    {
        private static readonly HttpClient client = new HttpClient();
        public async Task<string> GetAsync(string uri)
        {
            return await client.GetStringAsync(uri);
        }

        public async Task<string> PostAsync(string uri)
        {
            var values = new Dictionary<string, string>
            {
                { "thing1", "hello" },
                { "thing2", "world" }
            };

            var content = new FormUrlEncodedContent(values);

            var response = await client.PostAsync(uri, content);

            var responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
    }
}

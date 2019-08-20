using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace HttpLib
{

    public class D2RHttpManager
    {
        private DoubleAddress _rtuAddress;
        private IMyLog _logFile;
        public D2RHttpManager Initialize(DoubleAddress rtuAddress, IMyLog logFile)
        {
            _rtuAddress = rtuAddress;
            _logFile = logFile;

            return this;
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

            result.ReturnCode = ReturnCode.RtuInitializedSuccessfully;
            result.RtuId = dto.RtuId;
            result.RtuAddresses = dto.RtuAddresses;
            result.Maker = RtuMaker.VeEX;
            result.IsInitialized = true;
            return result;
        }

        private async Task<bool> GetOtauSettings(string rootUrl, RtuInitializedDto result)
        {
            try
            {
                var strOtaus = await GetAsync($"{rootUrl}/otaus");
                _logFile.AppendLine(strOtaus);
                var otaus = JsonConvert.DeserializeObject<Otaus>(strOtaus);

                var strOtau = await GetAsync($"{rootUrl}/{otaus.items[0].self}"); // could be more than one
                _logFile.AppendLine(strOtau);
                var otau = JsonConvert.DeserializeObject<Otau>(strOtau);
                result.Children = new Dictionary<int, OtauDto>();
                result.Children.Add(0, new OtauDto()
                {
                    IsOk = true,
                    OwnPortCount = otau.portCount,
                });
                result.OwnPortCount = otau.portCount;
                result.FullPortCount = otau.portCount;
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
                var strOtdrs = await GetAsync($"{rootUrl}/otdrs");
                _logFile.AppendLine(strOtdrs);
                var otdrs = JsonConvert.DeserializeObject<Otdrs>(strOtdrs);

                var strOtdr = await GetAsync($"{rootUrl}/{otdrs.items[0].self}"); // could be more than one
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
                var strInfo = await GetAsync($"{rootUrl}/info");
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
}

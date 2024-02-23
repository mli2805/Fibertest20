using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;
using Newtonsoft.Json;

namespace Iit.Fibertest.DataCenterCore
{
    public class MakLinuxHttpManager : IMakLinuxConnector
    {
        private readonly IMyLog _logFile;
        private static readonly HttpClient HttpClient = new HttpClient();

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public MakLinuxHttpManager(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<RtuConnectionCheckedDto> CheckRtuConnection(CheckRtuConnectionDto dto)
        {
            var result = new RtuConnectionCheckedDto()
            {
                ClientIp = dto.ClientIp,
                NetAddress = dto.NetAddress.Clone(),
                RtuId = dto.RtuId,
            };

            var uri = dto.NetAddress.GetMakLinuxBaseUri() + "rtu/current-state";
            var request = new HttpRequestMessage(new HttpMethod("GET"), uri);
            try
            {
                var _ = await HttpClient.SendAsync(request);
                result.IsConnectionSuccessfull = true;
            }
            catch (Exception e)
            {
                result.IsConnectionSuccessfull = false;
                _logFile.AppendLine($"CheckRtuConnection: {e.Message}");
            }

            return result;
        }

        public async Task<RtuCurrentStateDto> GetRtuCurrentState(GetCurrentRtuStateDto dto)
        {
            var uri = dto.RtuDoubleAddress.Main.GetMakLinuxBaseUri() + "rtu/current-state";
            var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            var request = CreateRequestMessage(uri, "post", "application/merge-patch+json", json);
            try
            {
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    return new RtuCurrentStateDto(ReturnCode.D2RHttpError)
                    {
                        ErrorMessage = response.ReasonPhrase
                    };
                }

                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<RtuCurrentStateDto>(responseJson, JsonSerializerSettings);
                return result;
            }
            catch (Exception e)
            {
                return new RtuCurrentStateDto(ReturnCode.D2RHttpError)
                {
                    ErrorMessage = e.Message
                };
            }
        }

        public async Task<TResult> SendCommand<T, TResult>(T dto, DoubleAddress rtuDoubleAddress) where TResult : RequestAnswer, new()
        {
            var uri = rtuDoubleAddress.Main.GetMakLinuxBaseUri() + "rtu/do-operation";
            var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            var request = CreateRequestMessage(uri, "post", "application/merge-patch+json", json);
            try
            {
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new TResult()
                    {
                        ReturnCode = ReturnCode.D2RHttpError,
                        ErrorMessage = $"StatusCode: {response.StatusCode}; " + response.ReasonPhrase
                    };

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<TResult>(responseJson);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"SendCommand: {e.Message}");
                return new TResult() { ReturnCode = ReturnCode.D2RHttpError, ErrorMessage = e.Message };
            }
        }

        private HttpRequestMessage CreateRequestMessage(string url, string method,
            string contentRepresentationType = null, string jsonData = null)
        {
            var request = new HttpRequestMessage(new HttpMethod(method.ToUpper()), url);
            if (jsonData != null)
                request.Content = new StringContent(jsonData, Encoding.UTF8, contentRepresentationType);
            return request;
        }
    }
}
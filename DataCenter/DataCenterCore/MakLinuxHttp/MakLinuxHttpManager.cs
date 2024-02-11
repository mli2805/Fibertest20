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

        public async Task<RequestAnswer> ApplyMonitoringSettingsAsync(ApplyMonitoringSettingsDto dto,
            DoubleAddress rtuDoubleAddress)
        {
            var uri = rtuDoubleAddress.Main.GetMakLinuxBaseUri() + "rtu/do-operation";
            var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            var request = CreateRequestMessage(uri, "post", "application/merge-patch+json", json);
            try
            {
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new RequestAnswer(ReturnCode.D2RHttpError)
                        { ErrorMessage = $"StatusCode: {response.StatusCode}; " + response.ReasonPhrase };

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RequestAnswer>(responseJson);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"ApplyMonitoringSettingsAsync: {e.Message}");
                return new RequestAnswer(ReturnCode.D2RHttpError) { ErrorMessage = e.Message };
            }
        }

        public async Task<RequestAnswer> StopMonitoringAsync(StopMonitoringDto dto, DoubleAddress rtuDoubleAddress)
        {
            var uri = rtuDoubleAddress.Main.GetMakLinuxBaseUri() + "rtu/do-operation";
            var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            var request = CreateRequestMessage(uri, "post", "application/merge-patch+json", json);
            try
            {
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new RequestAnswer(ReturnCode.D2RHttpError)
                        { ErrorMessage = $"StatusCode: {response.StatusCode}; " + response.ReasonPhrase };

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RequestAnswer>(responseJson);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"StopMonitoringAsync: {e.Message}");
                return new RequestAnswer(ReturnCode.D2RHttpError) { ErrorMessage = e.Message };
            }
        }

        // Full dto with base refs (sorBytes) is serialized into json here and de-serialized on RTU
        public async Task<BaseRefAssignedDto> TransmitBaseRefsToRtuAsync(AssignBaseRefsDto dto,
            DoubleAddress rtuDoubleAddress)
        {
            var uri = rtuDoubleAddress.Main.GetMakLinuxBaseUri() + "rtu/do-operation";
            var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            var request = CreateRequestMessage(uri, "post", "application/merge-patch+json", json);
            try
            {
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new BaseRefAssignedDto(ReturnCode.D2RHttpError)
                    { ErrorMessage = $"StatusCode: {response.StatusCode}; " + response.ReasonPhrase };

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<BaseRefAssignedDto>(responseJson);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"TransmitBaseRefsToRtuAsync: {e.Message}");
                return new BaseRefAssignedDto(ReturnCode.D2RHttpError) { ErrorMessage = e.Message };
            }
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

        private static readonly JsonSerializerSettings JsonSerializerSettings =
            new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

        public async Task<RtuInitializedDto> InitializeRtu(InitializeRtuDto dto)
        {
            var uri = dto.RtuAddresses.Main.GetMakLinuxBaseUri() + "rtu/do-operation";
            var json = JsonConvert.SerializeObject(dto, JsonSerializerSettings);
            var request = CreateRequestMessage(uri, "post", "application/merge-patch+json", json);
            try
            {
                var response = await HttpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    return new RtuInitializedDto(ReturnCode.D2RHttpError)
                    { ErrorMessage = $"StatusCode: {response.StatusCode}; " + response.ReasonPhrase };

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<RtuInitializedDto>(responseJson);
            }
            catch (Exception e)
            {
                _logFile.AppendLine($"InitializeRtu: {e.Message}");
                return new RtuInitializedDto(ReturnCode.D2RHttpError) { ErrorMessage = e.Message };
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
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class HttpExt
    {
        private readonly IMyLog _logFile;
        private static readonly HttpClient _httpClient = new HttpClient();

        public HttpExt(IMyLog logFile)
        {
            _logFile = logFile;
        }

        private async Task<HttpResponseMessage> MadeRequest(string httpMethod, string url, string jsonData)
        {
            switch (httpMethod.ToLower())
            {
                case "get": return await _httpClient.GetAsync(url);
            
                case "post": var content = new StringContent(
                        jsonData, Encoding.UTF8, "application/json");
                            return await _httpClient.PostAsync(url, content);

                case "patch": var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    request.Content = new StringContent(
                        jsonData, Encoding.UTF8, "application/merge-patch+json");
                    return await _httpClient.SendAsync(request);

                case "delete": return await _httpClient.DeleteAsync(url);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }

        public async Task<HttpRequestResult> RequestByUrl(DoubleAddress rtuDoubleAddress,
            string httpMethod, string relativeUri, string jsonData = null)
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var result = new HttpRequestResult();
            var url = $"http://{rtuDoubleAddress.Main.ToStringA()}/api/v1/{relativeUri}";
            try
            {
                var responseMessage = await MadeRequest(httpMethod, url, jsonData);
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                else
                    result.ResponseJson = await responseMessage.Content.ReadAsStringAsync();
                result.HttpStatusCode = responseMessage.StatusCode;
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                _logFile.AppendLine(e.Message);
            }

            return result;
        }
    }
}
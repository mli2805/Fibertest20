using System;
using System.IO;
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

        public async Task<HttpRequestResult> GetFile(DoubleAddress rtuDoubleAddress, string relativeUri)
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var result = new HttpRequestResult();
            var url = $"http://{rtuDoubleAddress.Main.ToStringA()}/api/v1/{relativeUri}";
            try
            {
                var responseMessage = await _httpClient.GetAsync(url);
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                else
                {
                    var filename = Guid.NewGuid() + ".zip";
                    using (
                        Stream contentStream = await responseMessage.Content.ReadAsStreamAsync(),
                        stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
                    result.ResponseJson = await responseMessage.Content.ReadAsStringAsync();
                }

                result.HttpStatusCode = responseMessage.StatusCode; 
            }
            catch (Exception e)
            {
                result.ErrorMessage = e.Message;
                _logFile.AppendLine(e.Message);
            }

            return result;
        }

        public async Task<HttpRequestResult> PostFile(DoubleAddress rtuDoubleAddress, string relativeUri, byte[] bytes)
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var result = new HttpRequestResult();
            var url = $"http://{rtuDoubleAddress.Main.ToStringA()}/api/v1/{relativeUri}";
            try
            {
                MultipartFormDataContent dataContent = new MultipartFormDataContent {new ByteArrayContent(bytes)};
                HttpResponseMessage responseMessage = await _httpClient.PostAsync(url, dataContent);
                if (responseMessage.StatusCode != HttpStatusCode.Created)
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

        public async Task<HttpRequestResult> RequestByUrl(DoubleAddress rtuDoubleAddress, string relativeUri, 
            string httpMethod, string contentRepresentation = null, string jsonData = null)
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var result = new HttpRequestResult();
            var url = $"http://{rtuDoubleAddress.Main.ToStringA()}/api/v1/{relativeUri}";
            try
            {
                var responseMessage = await MadeRequest(url, httpMethod, contentRepresentation, jsonData);
                var statusShouldBe = httpMethod.ToLower() == "post" ? HttpStatusCode.Created : HttpStatusCode.OK;
                if (responseMessage.StatusCode != statusShouldBe)
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

        private async Task<HttpResponseMessage> MadeRequest(
            string url, string httpMethod, string contentRepresentation, string jsonData)
        {
            switch (httpMethod.ToLower())
            {
                case "get": return await _httpClient.GetAsync(url);
            
                case "post": var content = new StringContent(
                        jsonData, Encoding.UTF8, contentRepresentation);
                            return await _httpClient.PostAsync(url, content);

                case "patch": var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    request.Content = new StringContent(
                        jsonData, Encoding.UTF8, contentRepresentation);
                    return await _httpClient.SendAsync(request);

                case "delete": return await _httpClient.DeleteAsync(url);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
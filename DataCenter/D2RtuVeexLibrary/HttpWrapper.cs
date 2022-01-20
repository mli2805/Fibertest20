using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.D2RtuVeexLibrary
{

    public class HttpWrapper
    {
        private readonly IMyLog _logFile;
        private readonly IHttpClientThinWrap _httpClientThinWrap;

        public HttpWrapper(IMyLog logFile, IHttpClientThinWrap httpClientThinWrap)
        {
            _logFile = logFile;
            _httpClientThinWrap = httpClientThinWrap;
        }

        private string BaseUri(string address) { return $"http://{address}/api/v1/"; }

        public async Task<HttpRequestResult> GetByteArray(DoubleAddress rtuDoubleAddress, string relativeUri)
        {
            var result = new HttpRequestResult();
            var url = BaseUri(rtuDoubleAddress.Main.ToStringA()) + relativeUri;
            try
            {
                var myArr = await _httpClientThinWrap.GetByteArrayAsync(url);
                result.ResponseBytesArray = myArr;
                result.HttpStatusCode = HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                result.HttpStatusCode = HttpStatusCode.BadRequest;
                result.ErrorMessage = e.Message;
                _logFile.AppendLine(e.Message);
            }

            return result;
        }

        public async Task<HttpRequestResult> PostByteArray(DoubleAddress rtuDoubleAddress, string relativeUri,
            byte[] bytes, byte[] bytes2 = null)
        {
            var result = new HttpRequestResult();
            var url = BaseUri(rtuDoubleAddress.Main.ToStringA()) + relativeUri;

            try
            {
                MultipartFormDataContent dataContent =
                    new MultipartFormDataContent(Guid.NewGuid().ToString());

                var byteArrayContent = new ByteArrayContent(bytes);
                byteArrayContent.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse("form-data; name=\"0\"; filename=\"\"");
                byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
                dataContent.Add(byteArrayContent);

                if (bytes2 != null)
                {
                    var byteArrayContent2 = new ByteArrayContent(bytes2);
                    byteArrayContent2.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse("form-data; name=\"2\"; filename=\"\"");
                    byteArrayContent2.Headers.ContentType = MediaTypeHeaderValue.Parse("text/plain");
                    dataContent.Add(byteArrayContent2);
                }

                HttpResponseMessage responseMessage = await _httpClientThinWrap.PostAsync(url, dataContent);
                if (responseMessage.StatusCode != HttpStatusCode.Created)
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                result.ResponseJson = await responseMessage.Content.ReadAsStringAsync(); // if error - it could be explanation
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
            var result = new HttpRequestResult();
            var url = BaseUri(rtuDoubleAddress.Main.ToStringA()) + relativeUri;
            try
            {
                var responseMessage = await MadeRequest(url, httpMethod, contentRepresentation, jsonData);
                if (!responseMessage.IsSuccessStatusCode)
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                if (responseMessage.StatusCode == HttpStatusCode.Created)
                    result.ResponseJson = responseMessage.Headers.Location.ToString();
                else
                    result.ResponseJson = await responseMessage.Content.ReadAsStringAsync(); // if error - it could be explanation
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
                case "get": return await _httpClientThinWrap.GetAsync(url);

                case "post":
                    var content = new StringContent(
               jsonData, Encoding.UTF8, contentRepresentation);
                    return await _httpClientThinWrap.PostAsync(url, content);

                case "patch":
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    request.Content = new StringContent(
                        jsonData, Encoding.UTF8, contentRepresentation);
                    return await _httpClientThinWrap.SendAsync(request);

                case "delete": return await _httpClientThinWrap.DeleteAsync(url);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
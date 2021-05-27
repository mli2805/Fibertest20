using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private string BaseUri(string address) { return $"http://{address}/api/v1/"; }

        /// <summary>
        /// Optical line can have more than 1 Test
        /// Test name could be precise/fast/additional
        /// Test has 1 reference (sor-file)
        /// </summary>
        /// <param name="rtuDoubleAddress"></param>
        /// <param name="relativeUri">
        /// base ref for the Test could be obtained by test's id
        /// monitoring/tests/10b2e984-14b2-444d-a76e-ad52d64dd07c/references/current/traces</param>
        /// <returns></returns>
        public async Task<HttpRequestResult> GetFile(DoubleAddress rtuDoubleAddress, string relativeUri)
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var result = new HttpRequestResult();
            var url = BaseUri(rtuDoubleAddress.Main.ToStringA()) + relativeUri;
            try
            {
                var responseMessage = await _httpClient.GetAsync(url);
                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    result.ErrorMessage = responseMessage.ReasonPhrase;
                    result.ResponseJson = await responseMessage.Content.ReadAsStringAsync(); // if error - it could be explanation
                }
                else
                {
                    var filename = @"..\temp\" + Guid.NewGuid() + ".zip";
                    using (Stream contentStream = await responseMessage.Content.ReadAsStreamAsync(),
                            stream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await contentStream.CopyToAsync(stream);
                    }
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

        public async Task<HttpRequestResult> GetByteArray(DoubleAddress rtuDoubleAddress, string relativeUri)
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            var result = new HttpRequestResult();
            var url = BaseUri(rtuDoubleAddress.Main.ToStringA()) + relativeUri;
            try
            {
                var myArr = await _httpClient.GetByteArrayAsync(url);
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



        public async Task<HttpRequestResult> PostFile(DoubleAddress rtuDoubleAddress, string relativeUri, byte[] bytes)
        {
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
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
                HttpResponseMessage responseMessage = await _httpClient.PostAsync(url, dataContent);
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
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
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
                case "get": return await _httpClient.GetAsync(url);

                case "post":
                    var content = new StringContent(
               jsonData, Encoding.UTF8, contentRepresentation);
                    return await _httpClient.PostAsync(url, content);

                case "patch":
                    var request = new HttpRequestMessage(new HttpMethod("PATCH"), url);
                    request.Content = new StringContent(
                        jsonData, Encoding.UTF8, contentRepresentation);
                    return await _httpClient.SendAsync(request);

                case "delete": return await _httpClient.DeleteAsync(url);
            }
            return new HttpResponseMessage(HttpStatusCode.BadRequest);
        }
    }
}
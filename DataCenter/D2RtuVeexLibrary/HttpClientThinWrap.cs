using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class HttpClientThinWrap : IHttpClientThinWrap
    {
        private int _nc = 1;
        private readonly IMyLog _logFile;

        private static readonly HttpClient HttpClient = new HttpClient()
        {
            DefaultRequestHeaders = { ExpectContinue = false },
            Timeout = TimeSpan.FromSeconds(400)
        };

        public HttpClientThinWrap(IMyLog logFile)
        {
            _logFile = logFile;
        }

        public async Task<byte[]> GetByteArrayAsync(string url)
        {
            return await HttpClient.GetByteArrayAsync(url);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, StringContent stringContent)
        {
            return await HttpClient.PostAsync(url, stringContent);
        }

        public async Task<HttpResponseMessage> PostAsync(string url, MultipartFormDataContent dataContent)
        {
            return await HttpClient.PostAsync(url, dataContent);
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return await HttpClient.SendAsync(request);
        }

        public async Task<HttpResponseMessage> GetAsync(string url)
        {
            var httpResponseMessage = await HttpClient.GetAsync(url);
            if (httpResponseMessage.StatusCode != HttpStatusCode.Unauthorized) 
                return httpResponseMessage;

            _logFile.AppendLine($"Unauthorized: {httpResponseMessage.Headers.WwwAuthenticate}");

            var authorization = DigestAuth.GetAuthorizationString(httpResponseMessage, url, "*10169~", _nc++);
            _logFile.AppendLine($"Authorization header: {authorization}");

            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Digest", authorization);
            httpResponseMessage = await HttpClient.GetAsync(url);
            return httpResponseMessage;
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await HttpClient.DeleteAsync(url);
        }
    }
}
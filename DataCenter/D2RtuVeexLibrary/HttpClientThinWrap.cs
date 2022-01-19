using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class HttpClientThinWrap : IHttpClientThinWrap
    {
        private static readonly HttpClient HttpClient = new HttpClient()
        {
            DefaultRequestHeaders = { ExpectContinue = false },
            Timeout = TimeSpan.FromSeconds(4)
        };

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
            return await HttpClient.GetAsync(url);
        }

        public async Task<HttpResponseMessage> DeleteAsync(string url)
        {
            return await HttpClient.DeleteAsync(url);
        }
    }
}
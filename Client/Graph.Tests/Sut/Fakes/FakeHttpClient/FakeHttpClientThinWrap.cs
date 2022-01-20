using System.Net.Http;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.UtilsLib;

namespace Graph.Tests
{
    public class FakeHttpClientThinWrap : IHttpClientThinWrap
    {
        private readonly FakeVeexRtuManager _fakeVeexRtuManager;
        public FakeVeexRtuModel FakeVeexRtuModel { get; set; }

        public FakeHttpClientThinWrap(FakeVeexRtuModel fakeVeexRtuModel, FakeVeexRtuManager fakeVeexRtuManager)
        {
            _fakeVeexRtuManager = fakeVeexRtuManager;
            FakeVeexRtuModel = fakeVeexRtuModel;
        }

        public Task<HttpResponseMessage> DeleteAsync(string url)
        {
            var pos = url.IndexOfNth("/", 4);
            var relativeUrl = url.Substring(pos + 1);
            return _fakeVeexRtuManager.RequestByUrl(relativeUrl, "delete");
        }

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            var pos = url.IndexOfNth("/", 4);
            var relativeUrl = url.Substring(pos + 1);
            return _fakeVeexRtuManager.RequestByUrl(relativeUrl, "get");
        }

        public Task<byte[]> GetByteArrayAsync(string url)
        {
            return Task.FromResult(FakeVeexRtuModel.SorBytesToReturn);
        }

        public Task<HttpResponseMessage> PostAsync(string url, StringContent stringContent)
        {
            var pos = url.IndexOfNth("/", 4);
            var relativeUrl = url.Substring(pos + 1);

            string representation = stringContent.Headers.ContentType.ToString();
            string jsonData = stringContent.ReadAsStringAsync().Result;

            return _fakeVeexRtuManager.RequestByUrl(relativeUrl, "post", representation, jsonData);
        }

        public Task<HttpResponseMessage> PostAsync(string url, MultipartFormDataContent dataContent)
        {
            return _fakeVeexRtuManager.PostByteArray();
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var url = request.RequestUri.ToString();
            var pos = url.IndexOfNth("/", 4);
            var relativeUrl = url.Substring(pos + 1);
            var method = request.Method.ToString().ToLower();

            var requestContent = (StringContent)request.Content;
            string representation = requestContent.Headers.ContentType.ToString();
            string jsonData = requestContent.ReadAsStringAsync().Result;

            return _fakeVeexRtuManager.RequestByUrl(relativeUrl, method, representation, jsonData);
        }
    }
}
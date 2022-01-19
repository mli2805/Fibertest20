using System.Net.Http;
using System.Threading.Tasks;
using Iit.Fibertest.D2RtuVeexLibrary;
using Iit.Fibertest.UtilsLib;

namespace Graph.Tests
{
    public class FakeHttpClientThinWrap : IHttpClientThinWrap
    {
        private readonly FakeHttpWrapper _fakeHttpWrapper;
        public FakeVeexRtuModel FakeVeexRtuModel { get; set; }

        public FakeHttpClientThinWrap(FakeVeexRtuModel fakeVeexRtuModel, FakeHttpWrapper fakeHttpWrapper)
        {
            _fakeHttpWrapper = fakeHttpWrapper;
            FakeVeexRtuModel = fakeVeexRtuModel;
        }

        public Task<HttpResponseMessage> DeleteAsync(string url)
        {
            var pos = url.IndexOfNth("/", 4);
            var relativeUrl = url.Substring(pos + 1);
            return _fakeHttpWrapper.RequestByUrl(relativeUrl, "delete");
        }

        public Task<HttpResponseMessage> GetAsync(string url)
        {
            var pos = url.IndexOfNth("/", 4);
            var relativeUrl = url.Substring(pos + 1);
            return _fakeHttpWrapper.RequestByUrl(relativeUrl, "get");
        }

        public Task<byte[]> GetByteArrayAsync(string url)
        {
            return Task.FromResult(FakeVeexRtuModel.SorBytesToReturn);
        }

        public Task<HttpResponseMessage> PostAsync(string url, StringContent stringContent)
        {
            var pos = url.IndexOfNth("/", 4);
            var relativeUrl = url.Substring(pos + 1); 
            return _fakeHttpWrapper.RequestByUrl(relativeUrl, "post");
        }

        public Task<HttpResponseMessage> PostAsync(string url, MultipartFormDataContent dataContent)
        {
            return Task.FromResult(new HttpResponseMessage());
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

            return _fakeHttpWrapper.RequestByUrl(relativeUrl, method, representation, jsonData);
        }
    }
}
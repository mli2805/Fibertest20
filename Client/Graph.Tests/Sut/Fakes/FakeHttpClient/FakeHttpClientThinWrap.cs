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

        public Task<byte[]> GetByteArrayAsync(string url)
        {
            return Task.FromResult(FakeVeexRtuModel.SorBytesToReturn);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var url = request.RequestUri.ToString();
            var pos = url.IndexOfNth("/", 4);
            var relativeUrl = url.Substring(pos + 1);
            var method = request.Method.ToString().ToLower();

            if (method == "post" && request.Content is MultipartFormDataContent)
                return _fakeVeexRtuManager.PostByteArray();

            var requestContent = (StringContent)request.Content;
            string representation = requestContent?.Headers.ContentType.ToString();
            string jsonData = requestContent?.ReadAsStringAsync().Result;
            return _fakeVeexRtuManager.RequestByUrl(relativeUrl, method, representation, jsonData);
        }
    }
}
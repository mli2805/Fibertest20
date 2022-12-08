using System.Net.Http;
using System.Threading.Tasks;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public interface IHttpClientThinWrap
    {
        Task<byte[]> GetByteArrayAsync(string url);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
    }
}
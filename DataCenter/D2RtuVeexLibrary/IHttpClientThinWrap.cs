using System.Net.Http;
using System.Threading.Tasks;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public interface IHttpClientThinWrap
    {
        Task<byte[]> GetByteArrayAsync(string url);
        Task<HttpResponseMessage> PostAsync(string url, StringContent stringContent);
        Task<HttpResponseMessage> PostAsync(string url, MultipartFormDataContent dataContent);
        Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
        Task<HttpResponseMessage> GetAsync(string url);
        Task<HttpResponseMessage> DeleteAsync(string url);
    }
}
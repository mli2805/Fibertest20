using System.Threading.Tasks;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public interface IHttpWrapper
    {
        Task<HttpRequestResult> GetByteArray(DoubleAddress rtuDoubleAddress, string relativeUri);

        Task<HttpRequestResult> PostByteArray(DoubleAddress rtuDoubleAddress, string relativeUri, byte[] bytes,
            byte[] bytes2 = null);

        Task<HttpRequestResult> RequestByUrl(DoubleAddress rtuDoubleAddress, string relativeUri,
            string httpMethod, string contentRepresentation = null, string jsonData = null);
    }
}
using System.Net;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class HttpRequestResult
    {
        public string ResponseJson;
        public byte[] ResponseBytesArray;
        public HttpStatusCode HttpStatusCode;
        public string ErrorMessage;
    }
}
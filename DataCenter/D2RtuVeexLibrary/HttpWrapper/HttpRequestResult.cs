using System.Net;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class HttpRequestResult
    {
        public string ResponseJson;
        public byte[] ResponseBytesArray;
        public object ResponseObject;
        public HttpStatusCode HttpStatusCode;
        public bool IsSuccessful;
        public string ErrorMessage;
    }
}
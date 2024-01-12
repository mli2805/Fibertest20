using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public class HttpClientForVeex : IHttpClientThinWrap
    {
        private readonly IMyLog _logFile;
        private readonly VeexRtuAuthorizationDict _veexRtuAuthorizationDict;

        private static readonly HttpClient HttpClient = new HttpClient()
        {
            DefaultRequestHeaders = { ExpectContinue = false },
            Timeout = TimeSpan.FromSeconds(400)
        };

        public HttpClientForVeex(IMyLog logFile, VeexRtuAuthorizationDict veexRtuAuthorizationDict)
        {
            _logFile = logFile;
            _veexRtuAuthorizationDict = veexRtuAuthorizationDict;
        }

        public async Task<byte[]> GetByteArrayAsync(string url)
        {
            byte[] result;
            try
            {
                var rtuData = _veexRtuAuthorizationDict.Dict[new Uri(url).Host];
                if (rtuData.IsAuthorizationOn)
                {
                    var authorization = rtuData.CreateAuthorizationString("GET", url);
                    _logFile.AppendLine($"Authorization header: {authorization}");
                    HttpClient.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Digest", authorization);
                }

                result = await HttpClient.GetByteArrayAsync(url);
            }
            catch (Exception e)
            {
                _logFile.AppendLine(e.Message);
                return null;
            }
            finally
            {
                HttpClient.DefaultRequestHeaders.Authorization = null;
            }
            return result;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var cloneBeforeSend = request.Clone();
            var rtuData = _veexRtuAuthorizationDict.Dict[request.Headers.Host];

            var httpResponseMessage = await HttpClient.SendAsync(request);
            if (httpResponseMessage.StatusCode != HttpStatusCode.Unauthorized
                || string.IsNullOrEmpty(rtuData.Serial)) // Unauthorized && Serial is not set yet
            {
                rtuData.IsAuthorizationOn = false;
                return httpResponseMessage;
            }
           
            if (!rtuData.IsAuthorizationOn)
            {
                rtuData.IsAuthorizationOn = true;
                rtuData.Nc = 1;
            }
            rtuData.AuthenticationHeaderParts =
                DigestAuth.ParseAuthHeader(httpResponseMessage.Headers.WwwAuthenticate.ToString());
            var authorization = rtuData.CreateAuthorizationString(request.Method.Method, request.RequestUri.ToString());
            cloneBeforeSend.Headers.Authorization = new AuthenticationHeaderValue("Digest", authorization);

            var httpResponseMessage2 = await HttpClient.SendAsync(cloneBeforeSend);
            if (!httpResponseMessage2.IsSuccessStatusCode)
            {
                _logFile.AppendLine($"Unauthorized {request.Method.Method} to {request.RequestUri};  " +
                                    $"WwwAuthenticate {httpResponseMessage.Headers.WwwAuthenticate}");
                _logFile.AppendLine($"Authorization header: {authorization}");
                _logFile.AppendLine($"Auth request result: {httpResponseMessage2.StatusCode}");
            }
            return httpResponseMessage2;
        }
    }
}
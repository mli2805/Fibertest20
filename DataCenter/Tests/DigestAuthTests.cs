using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using FluentAssertions;
using Iit.Fibertest.D2RtuVeexLibrary;
using Xunit;

namespace Tests
{
    public class DigestAuthTests
    {
        [Fact]
        public void GetResponse()
        {
            // server responce 401
            var realm = "http-auth@example.org";
            var qop = "auth";
            // var algorithm = "SHA-256";
            var nonce = "7ypf/xlj9XXwfDPEoM4URrv/xwf94BcCAzFZH4GiTo0v";
            // var opaque = "FQhe/qaU925kfnzjCev0ciny7QMkPqMAFRtzCUYo5tdS";

            // let it be
            var username = "Mufasa";
            var password = "Circle of Life";
            var method = "GET";
            //var url = "http://www.example.org/dir/index.html";
            var url = "/dir/index.html";

            var cnonce = "f2/wE4q74E6zIJEtWaHKaf5wv/H5QzzpXusqGemxURZJ";
            var nc = 1;

            // then
            var ha1 = DigestAuth.GetHa1(username, realm, password);
            var ha2 = DigestAuth.GetHa2(method, url);
            var response = DigestAuth.GetHashResponse(ha1, nonce, $"{nc:X8}", cnonce, qop, ha2);

            response.Should().Be("753927fa0e85d155564e2e272a28d1802ca10daf4496794697cf8db5856cb6c1");
        }


        private readonly HttpClient _httpClient = new HttpClient()
        {
            DefaultRequestHeaders = { ExpectContinue = false },
            Timeout = TimeSpan.FromSeconds(400)
        };

      //  [Fact]  real http request
        public async void GetInfo2()
        {
            var url = "http://172.16.4.30/api/v1/info";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var httpResponseMessage = await _httpClient.SendAsync(request);

            httpResponseMessage.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var newRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            var nc = 1;
            var authorization = DigestAuth.GetAuthorizationString(httpResponseMessage, url, "*10169~", nc);
            Debug.WriteLine(authorization);
            newRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Digest", authorization);


            var authRes = await _httpClient.SendAsync(newRequestMessage);
            _httpClient.Dispose();

            authRes.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
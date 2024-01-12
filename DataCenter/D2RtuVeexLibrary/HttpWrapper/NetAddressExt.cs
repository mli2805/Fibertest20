using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.D2RtuVeexLibrary
{
    public static class NetAddressExt
    {
        public static string GetVeexRtuUriHost(this NetAddress netAddress)
        {
            var url = $"http://{netAddress.ToStringA()}/api/v1/info";
            var uri = new Uri(url);
            return uri.Host;
        }

        public static string GetVeexRtuBaseUri(this NetAddress netAddress)
        {
            return $"http://{netAddress.ToStringA()}/api/v1/";
        }
    }
}
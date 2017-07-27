using System.Net.NetworkInformation;
using System.Text;

namespace Iit.Fibertest.Utils35
{
    public static class Pinger
    {
        public static bool Ping(NetAddress address)
        {
            return address.IsAddressSetAsIp ? Ping(address.Ip4Address) : Ping(address.HostName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="address">can be an IPaddress or host name</param>
        /// <returns></returns>
        public static bool Ping(string address)
        {
            var pingSender = new Ping();
            var options = new PingOptions { DontFragment = true };
            byte[] buffer = Encoding.ASCII.GetBytes(@"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
            int timeout = 120; // in ms
            PingReply reply = pingSender.Send(address, timeout, buffer, options);
            return reply?.Status == IPStatus.Success;
        }

    }
}
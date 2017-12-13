using System.Net;
using System.Net.Sockets;

namespace Iit.Fibertest.Client
{
    public static class LocalAddressResearcher
    {
        public static string GetLocalAddressToConnectServer(string serverAddress)
        {
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect(serverAddress, 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                return endPoint?.Address.ToString();
            }
        }

        /*
        private static void GetAllLocalAddresses()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                var family = "";
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    family = @"IPv4";
                if (ip.AddressFamily == AddressFamily.InterNetworkV6)
                    family = @"IPv6";
                Console.WriteLine($@"{family}:   {ip}");
            }
        }
        */
    }
}

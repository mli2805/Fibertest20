namespace DirectCharonLibrary
{
    public class TcpAddress
    {
        public string Ip { get; set; }
        public int TcpPort { get; set; }

        public TcpAddress()
        {
        }

        public TcpAddress(string ip, int tcpPort)
        {
            Ip = ip;
            TcpPort = tcpPort;
        }

        public override string ToString()
        {
            return $"{Ip}:{TcpPort}";
        }
    }
}
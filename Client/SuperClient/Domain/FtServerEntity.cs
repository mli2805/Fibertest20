namespace Iit.Fibertest.SuperClient
{
    public class FtServerEntity
    {
        public int Id { get; set; }
        public int Postfix { get; set; } // used for ini and log file names
        public string ServerTitle { get; set; }
        public string ServerIp { get; set; }
        public int ServerTcpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
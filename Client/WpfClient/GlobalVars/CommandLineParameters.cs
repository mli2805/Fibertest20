using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class CommandLineParameters
    {
        public bool IsUnderSuperClientStart { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public NetAddress ServerNetAddress { get; set; }
        public int ClientOrdinal { get; set; }
    }
}
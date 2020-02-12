using System;

namespace Iit.Fibertest.Dto
{
    public class ClientStation
    {
        public Guid ClientGuid { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Role UserRole { get;set; }
        public string ClientAddress { get; set; }
        public int ClientAddressPort { get; set; }
        public DateTime LastConnectionTimestamp { get; set; }

        public bool IsUnderSuperClient { get; set; }
        public bool IsWebClient { get; set; }
        public bool IsDesktopClient { get; set; }
    }
}
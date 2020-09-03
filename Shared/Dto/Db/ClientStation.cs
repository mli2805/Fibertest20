using System;

namespace Iit.Fibertest.Dto
{
    public class ClientStation
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Role UserRole { get;set; }

        // for desktop clients only
        public string ClientIp { get; set; }
        public int ClientAddressPort { get; set; }

        // for web clients
        public string SignalRconnectinoId { get; set; }

        public bool IsUnderSuperClient { get; set; }
        public bool IsWebClient { get; set; }
        public bool IsDesktopClient { get; set; }

        public DateTime LastConnectionTimestamp { get; set; }

    }
}
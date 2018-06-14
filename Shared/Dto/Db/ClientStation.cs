using System;

namespace Iit.Fibertest.Dto
{
    public class ClientStation
    {
        public int Id { get; set; }
        public Guid ClientGuid { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string ClientAddress { get; set; }
        public int ClientAddressPort { get; set; }
        public DateTime LastConnectionTimestamp { get; set; }
    }
}
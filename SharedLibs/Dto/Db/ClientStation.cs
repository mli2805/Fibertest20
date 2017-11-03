using System;

namespace Iit.Fibertest.Dto
{
    public class ClientStation
    {
        public int Id { get; set; }
        public Guid StationId { get; set; }
        public string Username { get; set; }
        public DateTime LastConnectionTimestamp { get; set; }
    }
}
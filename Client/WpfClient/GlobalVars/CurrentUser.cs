using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public class CurrentUser
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public Role Role { get; set; }
        public Guid ZoneId { get; set; }
        public string ZoneTitle { get; set; }

        public bool IsDefaultZoneUser => ZoneId == Guid.Empty;
    }
}
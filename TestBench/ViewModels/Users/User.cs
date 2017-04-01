using System;

namespace Iit.Fibertest.TestBench
{
    public class User
    {
        public string Name { get; set; }
        public Role Role { get; set; }

        public string Password { get; set; }

        public string Email { get; set; }
        public bool IsEmailActivated { get; set; }

        public bool IsDefaultZoneUser { get; set; }
        public Guid ZoneId { get; set; }
    }
}
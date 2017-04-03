using System;

namespace Iit.Fibertest.TestBench
{
    [Serializable]
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Role Role { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public bool IsEmailActivated { get; set; }
        public Guid ZoneId { get; set; }
        public bool IsDefaultZoneUser { get; set; }
    }
}
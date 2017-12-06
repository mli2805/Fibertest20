using System;

namespace Iit.Fibertest.Dto
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string EncodedPassword { get; set; }
        public string Email { get; set; }
        public bool IsEmailActivated { get; set; }
        public Role Role { get; set; }
        public Guid ZoneId { get; set; }
        public bool IsDefaultZoneUser { get; set; }
    }
  
}
using System;
using System.Collections.Generic;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class UserAdded
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string EncodedPassword { get; set; }
        public string Email { get; set; }
        public bool IsEmailActivated { get; set; }
        public Role Role { get; set; }
        public Guid ZoneId { get; set; }

        public List<Guid> HiddenRtus { get; set; }
    }
}
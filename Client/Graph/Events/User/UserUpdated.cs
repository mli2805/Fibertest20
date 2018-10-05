using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class UserUpdated
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string EncodedPassword { get; set; }
        public EmailReceiver Email { get; set; } = new EmailReceiver();
        public SmsReceiver Sms { get; set; } = new SmsReceiver();
        public Role Role { get; set; }
        public Guid ZoneId { get; set; }
        public bool IsDefaultZoneUser { get; set; }
    }
}
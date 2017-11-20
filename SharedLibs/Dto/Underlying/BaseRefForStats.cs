using System;

namespace Iit.Fibertest.Dto
{
    public class BaseRefForStats
    {
        public BaseRefType BaseRefType { get; set; }
        public DateTime AssignedAt { get; set; }
        public string AssignedBy { get; set; }
        public Guid BaseRefId { get; set; }
    }
}
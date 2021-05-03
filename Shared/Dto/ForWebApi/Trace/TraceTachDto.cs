using System;

namespace Iit.Fibertest.Dto
{
    public class TraceTachDto
    {
        public Guid TraceId;
        public FiberState TraceState;
        public int SorFileId;
        public bool Attach;
    }
}
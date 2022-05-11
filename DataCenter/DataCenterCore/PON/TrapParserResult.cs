using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public class TrapParserResult
    {
        public Guid TceId { get; set; }
        public int Slot { get; set; }
        public int GponInterface { get; set; }
        public FiberState State { get; set; }
        public string ZteEventId { get; set; }
    }
}
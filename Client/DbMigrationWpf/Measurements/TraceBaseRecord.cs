using System;
using Iit.Fibertest.Dto;

namespace DbMigrationWpf.Measurements
{
    public class TraceBaseRecord
    {
        public int TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public DateTime BaseTimestamp { get; set; }
        public int FileId { get; set; }
    }
}
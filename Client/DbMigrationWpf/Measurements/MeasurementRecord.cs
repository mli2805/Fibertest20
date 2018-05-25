using System;
using Iit.Fibertest.Dto;

namespace DbMigrationWpf.Measurements
{
    public class MeasurementRecord
    {
        public int TraceId { get; set; }
        public BaseRefType BaseRefType { get; set; }
        public FiberState TraceState { get; set; }
        public DateTime MeasurementTimestamp { get; set; }

        public int FileId { get; set; }
    }
}
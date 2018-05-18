using System;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public class UpdateMeasurement
    {
        public int SorFileId { get; set; }

        public EventStatus EventStatus { get; set; }
        public DateTime StatusChangedTimestamp { get; set; }
        public string StatusChangedByUser { get; set; }

        public string Comment { get; set; }
    }
}
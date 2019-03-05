using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Client
{
    public static class EventsClosingTimeProvider
    {
        public static Dictionary<int, DateTime> Calculate(List<OpticalEventModel> events)
        {
            var allAccidents = events.Where(r => r.EventStatus > EventStatus.EventButNotAnAccident).ToList();
            var result = new Dictionary<int, DateTime>();
            foreach (var opticalEventModel in allAccidents)
            {
                var okEvent = events.FirstOrDefault(e =>
                    e.TraceId == opticalEventModel.TraceId && e.TraceState == FiberState.Ok
                                                           && e.MeasurementTimestamp >= opticalEventModel.MeasurementTimestamp);
                if (okEvent != null)
                    result.Add(opticalEventModel.SorFileId, okEvent.MeasurementTimestamp);
            }
            return result;
        }
    }
}
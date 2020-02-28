using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph;

namespace Iit.Fibertest.DataCenterCore
{
    public static class MeasExt
    {
        public static bool Filter(this Measurement measurement, string filterRtu, string filterTrace, Model writeModel, User user)
        {
            if (measurement.EventStatus == EventStatus.JustMeasurementNotAnEvent)
                return false;

            var rtu = writeModel.Rtus.FirstOrDefault(r => r.Id == measurement.RtuId);
            if (rtu == null
                || !rtu.ZoneIds.Contains(user.ZoneId)
                || (!string.IsNullOrEmpty(filterRtu) && !rtu.Title.Contains(filterRtu)))
            {
                return false;
            }

            var trace = writeModel.Traces.FirstOrDefault(t => t.TraceId == measurement.TraceId);
            if (trace == null
                || !trace.ZoneIds.Contains(user.ZoneId)
                || (!string.IsNullOrEmpty(filterTrace) && !trace.Title.Contains(filterTrace)))
            {
                return false;
            }
            return true;
        }
        public static IEnumerable<Measurement> Sort(this IEnumerable<Measurement> input, string param)
        {
            return param == "asc" ? input.OrderBy(o => o.SorFileId) : input.OrderByDescending(o => o.SorFileId);
        }

    }
}
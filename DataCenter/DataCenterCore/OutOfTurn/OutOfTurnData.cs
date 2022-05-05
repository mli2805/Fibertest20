using System;
using System.Collections.Concurrent;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class OutOfTurnRequest
    {
        public DoOutOfTurnPreciseMeasurementDto Dto;
        public DateTime Timestamp; // Time when request placed into queue
    }
    public class OutOfTurnData
    {
        // RTU Id - Time when request sent to RTU
        private ConcurrentDictionary<Guid, DateTime> BusyRtus = new ConcurrentDictionary<Guid, DateTime>();

        // RTU Id - < Trace Id - Request >
        private ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, OutOfTurnRequest>> Requests =
            new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, OutOfTurnRequest>>();


        public void SetRtuIsBusy(Guid rtuId)
        {
            BusyRtus.AddOrUpdate(rtuId, DateTime.Now, (guid, time) => time);
        }

        public void AddNewRequest(DoOutOfTurnPreciseMeasurementDto dto, IMyLog logFile)
        {
            var newDict = new ConcurrentDictionary<Guid, OutOfTurnRequest>();
            newDict.TryAdd(dto.PortWithTraceDto.TraceId, new OutOfTurnRequest() { Dto = dto, Timestamp = DateTime.Now }); // no problem here

            Requests.AddOrUpdate(dto.RtuId, newDict,
                (guid, oneRtuDict) =>
                {
                    oneRtuDict.AddOrUpdate(
                    // if there is no request for this trace/port - add it
                        dto.PortWithTraceDto.TraceId, new OutOfTurnRequest() { Dto = dto, Timestamp = DateTime.Now },
                    // else - change time
                        (guid1, request) => { request.Timestamp = DateTime.Now; return request; });
                    return oneRtuDict;
                });

            logFile.AppendLine($"Request added or updated, Queue of RTU {dto.RtuId.First6()} contains {Requests[dto.RtuId].Count} requests");
        }

        public DoOutOfTurnPreciseMeasurementDto GetNextRequest()
        {
            // local copy
            var requests = Requests.ToArray();

            foreach (var oneRtuDict in requests)
            {
                if (BusyRtus.ContainsKey(oneRtuDict.Key))
                    continue;

                if (oneRtuDict.Value.IsEmpty)
                    continue;

                var oneRtuRequests = oneRtuDict.Value.Values.OrderBy(r => r.Timestamp);
                var dto = oneRtuRequests.First().Dto;

                Requests[oneRtuDict.Key]
                    .TryRemove(dto.PortWithTraceDto.TraceId, out OutOfTurnRequest _);

                return dto;
            }

            return null;
        }

    }
}

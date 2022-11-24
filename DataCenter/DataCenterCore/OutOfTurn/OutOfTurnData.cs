using System;
using System.Collections.Concurrent;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.Graph.RtuOccupy;
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
        // RTU Id - < Trace Id - Request >
        public readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, OutOfTurnRequest>> Requests =
            new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, OutOfTurnRequest>>();

        public void AddNewRequest(DoOutOfTurnPreciseMeasurementDto dto, IMyLog logFile)
        {
            var newDict = new ConcurrentDictionary<Guid, OutOfTurnRequest>();
            newDict.TryAdd(dto.PortWithTraceDto.TraceId, new OutOfTurnRequest() { Dto = dto, Timestamp = DateTime.Now }); // no problem could be with empty dict

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

        public DoOutOfTurnPreciseMeasurementDto GetNextRequest(IMyLog logFile, RtuOccupations rtuOccupations, string trapSenderUser, out int count)
        {
            // local copy
            var requests = Requests.ToArray();
            count = requests.Length;

            foreach (var oneRtuDict in requests)
            {
                if (oneRtuDict.Value.IsEmpty)
                    continue;
                
                if (!rtuOccupations.TrySetOccupation(oneRtuDict.Key, RtuOccupation.PreciseMeasurementOutOfTurn,
                                      trapSenderUser, out RtuOccupationState _))
                {
                    logFile.AppendLine($"RTU {oneRtuDict.Key.First6()} is busy");
                    continue;
                }
                logFile.AppendLine("RTU is OK");

                var oneRtuRequests = oneRtuDict.Value.Values.OrderBy(r => r.Timestamp);
                var dto = oneRtuRequests.First().Dto;

                Requests[oneRtuDict.Key]
                    .TryRemove(dto.PortWithTraceDto.TraceId, out OutOfTurnRequest _);
                logFile.AppendLine($"Request for RTU {dto.RtuId.First6()} / Trace {dto.PortWithTraceDto.TraceId.First6()} found.");
                logFile.AppendLine($"  Now queue of RTU {dto.RtuId.First6()} contains {Requests[dto.RtuId].Count} requests");

                return dto;
            }

            return null;
        }
    }
}

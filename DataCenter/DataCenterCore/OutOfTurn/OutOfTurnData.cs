using System;
using System.Collections.Concurrent;
using Iit.Fibertest.Dto;
using Iit.Fibertest.UtilsLib;

namespace Iit.Fibertest.DataCenterCore
{
    public class OutOfTurnData
    {
        // RTU id - Time when request sent
        private ConcurrentDictionary<Guid, DateTime> BusyRtus = new ConcurrentDictionary<Guid, DateTime>();

        public ConcurrentDictionary<Guid, ConcurrentQueue<DoOutOfTurnPreciseMeasurementDto>> Requests =
            new ConcurrentDictionary<Guid, ConcurrentQueue<DoOutOfTurnPreciseMeasurementDto>>();

        public void SetRtuIsBusy(Guid rtuId)
        {
            BusyRtus.AddOrUpdate(rtuId, DateTime.Now, (guid, time) => time);
        }

        public void AddNewRequest(DoOutOfTurnPreciseMeasurementDto dto, IMyLog logFile)
        {
            var newQueue = new ConcurrentQueue<DoOutOfTurnPreciseMeasurementDto>();
            newQueue.Enqueue(dto);
            Requests.AddOrUpdate(dto.RtuId, newQueue,
                (guid, queue) =>
                {
                    queue.Enqueue(dto); 
                    return queue;
                });

            var rtuQueue = Requests[dto.RtuId];
            logFile.AppendLine($"Request added or updated, Queue of RTU {dto.RtuId.First6()} contains {rtuQueue.Count} requests");
        }

        public DoOutOfTurnPreciseMeasurementDto GetNextRequest()
        {
            var requests = Requests.ToArray();
            foreach (var oneRtuQueue in requests)
            {
                if (BusyRtus.ContainsKey(oneRtuQueue.Key))
                    continue;

                if (oneRtuQueue.Value.TryDequeue(out DoOutOfTurnPreciseMeasurementDto dto))
                    return dto;
            }

            return null;
        }

    }
}

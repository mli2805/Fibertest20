using System.Collections.Concurrent;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.DataCenterCore
{
    public class OutOfTurnQueue
    {
        public ConcurrentQueue<DoOutOfTurnPreciseMeasurementDto> Requests = new ConcurrentQueue<DoOutOfTurnPreciseMeasurementDto>();
    }
}

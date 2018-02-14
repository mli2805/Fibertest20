using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;
using Iit.Fibertest.IitOtdrLibrary;
using Optixsoft.SorExaminer.OtdrDataFormat;

namespace Iit.Fibertest.Client
{
    public class BreakNotifier
    {
        private readonly GraphReadModel _graphReadModel;

        public BreakNotifier(GraphReadModel graphReadModel)
        {
            _graphReadModel = graphReadModel;
        }

        public void NotifyAboutMonitoringResult(MeasurementWithSor measurementWithSor)
        {
            var traceVm = _graphReadModel.Traces.FirstOrDefault(t => t.Id == measurementWithSor.Measurement.TraceId);
            if (traceVm == null) return;

            var sorData = SorData.FromBytes(measurementWithSor.SorBytes);
            var newEvents = sorData.GetNewEvents().ToList();

            traceVm.State = measurementWithSor.Measurement.TraceState;
        }

        
    }

    public static class SorDataBreaksExtractorExt
    {
        public static IEnumerable<BreakAsNewEvent> GetNewEvents(this OtdrDataKnownBlocks sorData)
        {
            for (int i = 1; i < sorData.KeyEvents.KeyEventsCount-1; i++)
            {
                if (sorData.RftsEvents.Events[i].EventTypes == RftsEventTypes.IsNew)
                {
                    var newEvent = new BreakAsNewEvent()
                    {
                        LeftNodeOwt = sorData.KeyEvents.KeyEvents[i-1].EventPropagationTime,
                        BreakOwt = sorData.KeyEvents.KeyEvents[i].EventPropagationTime,
                        RightNodeOwt = sorData.KeyEvents.KeyEvents[i+1].EventPropagationTime,
                        BreakType = 0,
                    };
                    yield return newEvent;
                }
            }
        }
    }

    public class BreakAsNewEvent
    {
        public int LeftNodeOwt { get; set; }
        public int BreakOwt { get; set; }
        public int RightNodeOwt { get; set; }

        public int BreakType { get; set; }
    }
}

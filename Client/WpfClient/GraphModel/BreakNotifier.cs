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
            var accidents = sorData.GetAccidents().ToList();

            traceVm.State = measurementWithSor.Measurement.TraceState;
        }


    }

    public static class SorDataBreaksExtractorExt
    {
        public static IEnumerable<AccidentOnTrace> GetAccidents(this OtdrDataKnownBlocks sorData)
        {
            for (int i = 0; i < sorData.RftsEvents.EventsCount; i++)
            {
                var rftsEvent = sorData.RftsEvents.Events[i];
                if (rftsEvent.EventTypes == RftsEventTypes.IsFailed ||
                    rftsEvent.EventTypes == RftsEventTypes.IsFiberBreak)
                {
                    var brokenLandmark = sorData.LinkParameters.LandmarkBlocks.FirstOrDefault(l => l.RelatedEventNumber == i + 1);
                    if (brokenLandmark == null) continue;

                    var accident = new BreakInOldEvent()
                    {
                        EventNumber = brokenLandmark.Number,
                        BreakOwt = brokenLandmark.Location,
                        AccidentSeriousness = rftsEvent.EventTypes == RftsEventTypes.IsFiberBreak ? FiberState.FiberBreak : FiberState.Critical, //TODO
                        BreakType = OpticalAccidentType.Reflectance, // TODO
                    };

                    yield return accident;
                }

                if (sorData.RftsEvents.Events[i].EventTypes == RftsEventTypes.IsNew)
                {
                    var newEvent = new BreakAsNewEvent()
                    {
                        LeftNodeOwt = sorData.KeyEvents.KeyEvents[i - 1].EventPropagationTime,
                        BreakOwt = sorData.KeyEvents.KeyEvents[i].EventPropagationTime,
                        RightNodeOwt = sorData.KeyEvents.KeyEvents[i + 1].EventPropagationTime,
                        AccidentSeriousness = FiberState.FiberBreak, //TODO
                        BreakType = OpticalAccidentType.Break, //TODO
                    };
                     
                    yield return newEvent;
                }
            }
        }
    }




    public class BreakInOldEvent : AccidentOnTrace
    {
        public int EventNumber { get; set; }
        public int RelatedLandmarkNumber { get; set; }
    }

    public class BreakAsNewEvent : AccidentOnTrace
    {
        public int LeftNodeOwt { get; set; }
        public int RightNodeOwt { get; set; }

    }

    public class AccidentOnTrace
    {
        public int BreakOwt { get; set; } // km?
        public FiberState AccidentSeriousness { get; set; } 

        public OpticalAccidentType BreakType { get; set; }

    }

    public enum OpticalAccidentType
    {
        Break,                   // B,  обрыв
        Loss,                    // L,  превышение порога затухания
        Reflectance,             // R,  превышение порога коэффициента отражения 
        LossCoeff,               // C,  превышение порога коэффициента затухания
    }
}

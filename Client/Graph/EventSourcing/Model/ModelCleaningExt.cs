using System;
using System.Collections.Generic;
using System.Linq;
using Iit.Fibertest.Dto;

namespace Iit.Fibertest.Graph
{
    public static class ModelCleaningExt
    {
        public static List<Measurement> GetMeasurementsForDeletion(
            this Model model, DateTime upTo, bool isMeasurementsNotEvents, bool isOpticalEvents)
        {
            var result = model.Measurements.Where(m =>
                m.EventRegistrationTimestamp.Date <= upTo.Date
                && (m.EventStatus == EventStatus.JustMeasurementNotAnEvent && isMeasurementsNotEvents
                    || m.EventStatus != EventStatus.JustMeasurementNotAnEvent && isOpticalEvents)).ToList();

            if (!isOpticalEvents)
                return result;

            // if optical events checked and 
            //   last event for any trace falls within selected period 
            //       it should be excluded from result
            foreach (var trace in model.Traces)
            {
                var lastEvent = model.Measurements.
                    LastOrDefault(m => m.TraceId == trace.TraceId && m.EventStatus > EventStatus.JustMeasurementNotAnEvent);
                if (lastEvent != null && lastEvent.EventRegistrationTimestamp.Date <= upTo.Date)
                    result.Remove(lastEvent);
            }
            return result;
        }

        public static List<NetworkEvent> GetNetworkEventsForDeletion(this Model model, DateTime upTo)
        {
            var result = model.NetworkEvents.Where(n => n.EventTimestamp.Date <= upTo).ToList();
            foreach (var rtu in model.Rtus)
            {
                var lastEventOnMainChannel =
                    model.NetworkEvents.LastOrDefault(n => n.RtuId == rtu.Id && n.OnMainChannel != ChannelEvent.Nothing);
                if (lastEventOnMainChannel != null && lastEventOnMainChannel.EventTimestamp.Date <= upTo)
                    result.Remove(lastEventOnMainChannel);
                var lastEventOnReserveChannel = model.NetworkEvents.LastOrDefault(n =>
                    n.RtuId == rtu.Id && n.OnReserveChannel != ChannelEvent.Nothing);
                if (lastEventOnReserveChannel != null && lastEventOnReserveChannel.EventTimestamp.Date <= upTo)
                    result.Remove(lastEventOnReserveChannel);
            }

            return result;
        }

        public static List<BopNetworkEvent> GetBopNetworkEventsForDeletion(this Model model, DateTime upTo)
                  
        {
            var result = model.BopNetworkEvents.Where(n => n.EventTimestamp.Date <= upTo).ToList();
            foreach (var otau in model.Otaus)
            {
                var lastEvent = model.BopNetworkEvents.LastOrDefault(b => b.Serial == otau.Serial);
                if (lastEvent != null && lastEvent.EventTimestamp.Date <= upTo)
                    result.Remove(lastEvent);
            }

            return result;
        }

        public static List<RtuAccident> GetRtuStatusEventsForDeletion(this Model model, DateTime upTo)
        {
            var result = model.RtuAccidents.Where(a=>a.EventRegistrationTimestamp <= upTo).ToList();

            foreach (var rtu in model.Rtus)
            {
                var lastEvent = model.RtuAccidents.LastOrDefault(a=>a.RtuId == rtu.Id && !a.IsMeasurementProblem);
                if (lastEvent != null && !lastEvent.IsGoodAccident && lastEvent.EventRegistrationTimestamp <= upTo)
                    result.Remove(lastEvent);
            }

            foreach (var trace in model.Traces)
            {
                var lastEvent = model.RtuAccidents.LastOrDefault(a=>a.TraceId == trace.TraceId && a.IsMeasurementProblem);
                if (lastEvent != null && !lastEvent.IsGoodAccident && lastEvent.EventRegistrationTimestamp <= upTo)
                    result.Remove(lastEvent); }

            return result;
        }
    }
}
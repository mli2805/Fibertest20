using System;
using System.Collections.Generic;
// ReSharper disable InconsistentNaming

namespace Iit.Fibertest.Dto
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<VeexNotification>(myJsonResponse); 
    public class Change
    {
        public double changeLocation { get; set; }
        public string changeType { get; set; }
        public int currentEventIndex { get; set; }
        public double currentEventLeadingLossCoefficient { get; set; }
        public double currentEventReflectance { get; set; }
        public string currentEventType { get; set; }
        public int referenceEventIndex { get; set; }
        public bool referenceEventMapsToCurrentEvent { get; set; }
        public string referenceEventType { get; set; }
    }

    public class TraceChange
    {
        public double changeLocation { get; set; }
        public string changeType { get; set; }
        public List<Change> changes { get; set; }
        public int currentEventIndex { get; set; }
        public double currentEventLeadingLossCoefficient { get; set; }
        public double currentEventReflectance { get; set; }
        public string currentEventType { get; set; }
        public string levelName { get; set; }
        public List<VeexMeasurementLevel> levels { get; set; }
        public int referenceEventIndex { get; set; }
        public bool referenceEventMapsToCurrentEvent { get; set; }
        public string referenceEventType { get; set; }
    }

    public class VeexMeasurementLevel
    {
        public List<Change> changes { get; set; }
        public string levelName { get; set; }
    }

    public class Data
    {
        public string result { get; set; }
        public List<VeexOtauPort> OtauPorts { get; set; }
        public DateTime started { get; set; }
        public string testId { get; set; }
        public string testName { get; set; }
        public string type { get; set; }
        public string extendedResult { get; set; }
        public TraceChange traceChange { get; set; }
    }

    public class VeexNotificationEvent
    {
        public Data data { get; set; }
        public DateTime time { get; set; }
        public string type { get; set; }
    }

    public class VeexNotification
    {
        public List<VeexNotificationEvent> events { get; set; }
        public string type { get; set; }
    }

}

using System;
using System.Collections.Generic;

namespace Iit.Fibertest.Dto
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<VeexNotification>(myJsonResponse); 
    public class Change
    {
        public double ChangeLocation { get; set; }
        public string ChangeType { get; set; }
        public int CurrentEventIndex { get; set; }
        public double CurrentEventLeadingLossCoefficient { get; set; }
        public double CurrentEventReflectance { get; set; }
        public string CurrentEventType { get; set; }
        public int ReferenceEventIndex { get; set; }
        public bool ReferenceEventMapsToCurrentEvent { get; set; }
        public string ReferenceEventType { get; set; }
    }

    public class TraceChange
    {
        public double ChangeLocation { get; set; }
        public string ChangeType { get; set; }
        public List<Change> Changes { get; set; }
        public int CurrentEventIndex { get; set; }
        public double CurrentEventLeadingLossCoefficient { get; set; }
        public double CurrentEventReflectance { get; set; }
        public string CurrentEventType { get; set; }
        public string LevelName { get; set; }
        public int ReferenceEventIndex { get; set; }
        public bool ReferenceEventMapsToCurrentEvent { get; set; }
        public string ReferenceEventType { get; set; }
    }

    public class Data
    {
        public string Result { get; set; }
        public List<VeexOtauPort> OtauPorts { get; set; }
        public DateTime Started { get; set; }
        public string TestId { get; set; }
        public string TestName { get; set; }
        public string Type { get; set; }
        public string ExtendedResult { get; set; }
        public TraceChange TraceChange { get; set; }
    }

    public class VeexNotificationEvent
    {
        public Data Data { get; set; }
        public DateTime Time { get; set; }
        public string Type { get; set; }
    }

    public class VeexNotification
    {
        public List<VeexNotificationEvent> Events { get; set; }
        public string Type { get; set; }
    }

}

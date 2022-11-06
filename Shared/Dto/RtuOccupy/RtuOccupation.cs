using System;

namespace Iit.Fibertest.Dto
{
    public enum RtuOccupation
    {
        None, 
        RemoveRtu, CleanOrRemoveTrace, AttachOrDetachOtau, AttachTrace,
        AutoBaseMeasurement, MeasurementClient, PreciseMeasurementOutOfTurn,  
        Initialization, MonitoringSettings, DetachTraces, AssignBaseRefs,
    }

    public class RtuOccupationState
    {
        // public Guid RtuId { get; set; }
        public RtuOccupation RtuOccupation;
        public string UserName; // who started occupation
        public DateTime Expired;
    }


}

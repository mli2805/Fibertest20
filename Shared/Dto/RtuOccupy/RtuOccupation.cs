using System;

namespace Iit.Fibertest.Dto
{
    public enum RtuOccupation
    {
        None, 
        AutoBaseMeasurement, MeasurementClient, PreciseMeasurementOutOfTurn, MeasurementReflect, 
        Initialization, MonitoringSettings, DetachTraces, AssignBaseRefs,
    }

    public class RtuOccupationState
    {
        public Guid RtuId;
        public RtuOccupation RtuOccupation;
        public string UserName; // who started occupation
        public DateTime Expired;
    }


}

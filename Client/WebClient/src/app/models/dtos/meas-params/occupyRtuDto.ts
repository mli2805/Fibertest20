export class OccupyRtuDto{
    connectionId: string;
    clientIp: string;
    username: string;
    rtuId: string;
    state: RtuOccupationState;
}

export class RtuOccupationState {
    rtuId: string;
    rtuOccupation: RtuOccupation;
    username: string;
    expired: Date;
}

export enum RtuOccupation {
    None, 
    AutoBaseMeasurement, MeasurementClient, PreciseMeasurementOutOfTurn, MeasurementReflect, 
    Initialization, MonitoringSettings, DetachTraces, AssignBaseRefs,
}
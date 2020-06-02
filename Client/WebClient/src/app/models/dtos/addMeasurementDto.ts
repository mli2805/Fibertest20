import { BaseRefType } from "../enums/baseRefType";
import { FiberState } from "../enums/fiberState";
import { EventStatus } from "../enums/eventStatus";

export class AddMeasurementDto {
  sorFileId: number;
  baseRefType: BaseRefType;
  eventRegistrationTimestamp: Date;
  eventStatus: EventStatus;
  traceId: string;
  traceState: FiberState;
  accidents: AccidentOnTraceV2[];
}

export class AccidentOnTraceV2 {
  brokenRftsEventNumber: number;
  accidentSeriousness: FiberState;
}

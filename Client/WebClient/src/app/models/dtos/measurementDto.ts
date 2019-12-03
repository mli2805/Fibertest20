import { BaseRefType } from "../enums/baseRefType";
import { FiberState } from "../enums/fiberState";

export class MeasurementDto {
  sorFileId: number;
  baseRefType: BaseRefType;
  eventRegistrationTimestamp: Date;
  isEvent: boolean;
  traceState: FiberState;
}

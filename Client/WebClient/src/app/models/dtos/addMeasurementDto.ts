import { BaseRefType } from "../enums/baseRefType";
import { FiberState } from "../enums/fiberState";

export class AddMeasurementDto {
  sorFileId: number;
  baseRefType: BaseRefType;
  eventRegistrationTimestamp: Date;
  isEvent: boolean;
  traceId: string;
  traceState: FiberState;
}

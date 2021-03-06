import { TraceHeaderDto } from "./traceHeaderDto";
import { BaseRefType } from "../../enums/baseRefType";
import { EventStatus } from "../../enums/eventStatus";
import { FiberState } from "../../enums/fiberState";
import { AccidentLineDto } from "./accidentLineDto";

export class TraceStateDto {
  header: TraceHeaderDto = new TraceHeaderDto();
  rtuId: string;
  traceId: string;
  traceState: FiberState;
  baseRefType: BaseRefType;
  eventStatus: EventStatus;
  comment: string;
  measurementTimestamp: Date;
  registrationTimestamp: Date;
  sorFileId: number;
  accidents: AccidentLineDto[];
  isLastStateForThisTrace: boolean;
  isLastAccidentForThisTrace: boolean;
}

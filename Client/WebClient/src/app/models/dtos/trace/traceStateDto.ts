import { TraceHeaderDto } from "./traceHeaderDto";
import { BaseRefType } from "../../enums/baseRefType";
import { EventStatus } from "../../enums/eventStatus";
import { FiberState } from "../../enums/fiberState";
import { AccidentOnTraceV2Dto } from "./accidentOnTraceV2Dto";

export class TraceStateDto {
  header: TraceHeaderDto;
  traceState: FiberState;
  baseRefType: BaseRefType;
  eventStatus: EventStatus;
  comment: string;
  measurementTimestamp: Date;
  registrationTimestamp: Date;
  sorFileId: number;
  accidents: AccidentOnTraceV2Dto[];
  isLastStateForThisTrace: boolean;
  isLastAccidentForThisTrace: boolean;
}

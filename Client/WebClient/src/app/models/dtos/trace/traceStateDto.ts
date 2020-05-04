import { TraceHeaderDto } from "./traceHeaderDto";
import { BaseRefType } from "../../enums/baseRefType";
import { EventStatus } from "../../enums/eventStatus";
import { FiberState } from "../../enums/fiberState";
import { AccidentLineDto } from "./accidentLineDto";

export class TraceStateDto {
  header: TraceHeaderDto = new TraceHeaderDto();
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

  get stateAt(): string {
    return `State at ${this.registrationTimestamp} (ID ${this.sorFileId})`;
  }
}

export class Foo {
  a: Date;
  b: number;

  get c() {
    return `a = ${this.a}, b = ${this.b}`;
  }
}

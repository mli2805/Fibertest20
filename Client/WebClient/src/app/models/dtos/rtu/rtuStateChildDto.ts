import { FiberState } from "../../enums/fiberState";

export class RtuStateChildDto {
  port: string;
  traceId: string;
  traceTitle: string;
  traceState: FiberState;
  lastMeasId: string;
  lastMeasTime: string;
}

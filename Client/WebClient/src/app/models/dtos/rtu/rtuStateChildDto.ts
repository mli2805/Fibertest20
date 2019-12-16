import { ChildType } from "../../enums/childType";

import { FiberState } from "../../enums/fiberState";

export class RtuStateChildDto {
  port: string;
  traceTitle: string;
  traceState: FiberState;
  lastMeasId: string;
  lastMeasTime: string;
}

import { FiberState } from "../../enums/fiberState";

export class TraceTachDto {
  traceId: string;
  traceState: FiberState;
  sorFileId: number;
  attach: boolean; // false - detach
}

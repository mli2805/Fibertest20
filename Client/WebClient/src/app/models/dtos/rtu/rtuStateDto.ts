import { FiberState } from "../../enums/fiberState";
import { MonitoringMode } from "../../enums/monitoringMode";
import { RtuPartState } from "../../enums/rtuPartState";
import { RtuStateChildDto } from "./rtuStateChildDto";

export class RtuStateDto {
  rtuTitle: string;

  mainChannel: string;
  mainChannelState: RtuPartState;
  reserveChannel: string;
  reserveChannelState: RtuPartState;
  isReserveChannelSet: boolean;

  bopState: RtuPartState;
  monitoringMode: MonitoringMode;
  tracesState: FiberState;

  ownPortCount: number;
  fullPortCount: number;
  bopCount: number;
  traceCount: number;

  children: RtuStateChildDto[];
}

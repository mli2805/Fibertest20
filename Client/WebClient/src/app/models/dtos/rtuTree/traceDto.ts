import { OtauPortDto } from "../../underlying/otauPortDto";
import { FiberState } from "../../enums/fiberState";
import { ChildDto } from "./childDto";
import { MonitoringMode } from "../../enums/monitoringMode";

export class TraceDto extends ChildDto {
  traceId: string;
  rtuId: string;
  title: string;

  otauPort: OtauPortDto;
  isAttached: boolean;
  port: number;

  state: FiberState;

  hasEnoughBaseRefsToPerformMonitoring: boolean;
  isIncludedInMonitoringCycle: boolean;

  rtuMonitoringMode: MonitoringMode;
}

import { PortMonitoringMode } from "src/app/models/enums/portMonitoringMode";

export interface PortLineVm {
  portMonitoringMode: PortMonitoringMode;
  port: string;
  disabled: boolean;
  traceTitle: string;
  durationOfPreciseBase: number;
  durationOfFastBase: number;
  duration: string;
}

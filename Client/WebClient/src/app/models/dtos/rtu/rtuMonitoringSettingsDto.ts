import { MonitoringMode } from "../../enums/monitoringMode";
import { PortMonitoringMode } from "../../enums/portMonitoringMode";
import { Frequency } from "../../enums/frequency";

export class RtuMonitoringSettingsDto {
  rtuTitle: string;
  monitoringMode: MonitoringMode;

  preciseMeas: Frequency;
  preciseSave: Frequency;
  fastSave: Frequency;

  lines: RtuMonitoringPortDto[];
}

export class RtuMonitoringPortDto {
  port: string;
  traceId: string;
  otauPortDto: OtauPortDto;
  traceTitle: string;
  portMonitoringMode: PortMonitoringMode;
  durationOfPreciseBase: number;
  durationOfFastBase: number;
}

export class OtauPortDto {
  opticalPort: number;
  isPortOnMainCharon: boolean;
  serial: string;
  otauId: string;
}

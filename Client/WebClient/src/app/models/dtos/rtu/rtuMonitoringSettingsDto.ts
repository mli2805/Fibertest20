import { MonitoringMode } from "../../enums/monitoringMode";
import { PortMonitoringMode } from "../../enums/portMonitoringMode";
import { Frequency } from "../../enums/frequency";
import { RtuMaker } from "../../enums/rtuMaker";
import { OtauPortDto } from "../../underlying/otauPortDto";

export class RtuMonitoringSettingsDto {
  rtuTitle: string;
  rtuMaker: RtuMaker;
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

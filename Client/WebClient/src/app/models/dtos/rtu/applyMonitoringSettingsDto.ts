import { RtuMaker } from "../../enums/rtuMaker";

export class ApplyMonitoringSettingsDto {
  clientId: string;
  rtuId: string;
  rtuMaker: RtuMaker;

  isMonitoringOn: boolean;
  timespans: MonitoringTimespansDto;
}

export class MonitoringTimespansDto {}

import { RtuMaker } from "../../enums/rtuMaker";

export class StopMonitoringDto {
  connectionId: string;
  rtuId: string;
  rtuMaker: RtuMaker;
}

export class MonitoringStoppedDto {
  rtuId: string;
}

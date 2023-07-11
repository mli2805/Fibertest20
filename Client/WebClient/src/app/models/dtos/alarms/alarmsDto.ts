import { NetworkAlarm } from "./networkAlarm";
import { OpticalAlarm } from "./opticalAlarm";
import { BopAlarm } from "./bopAlarm";
import { RtuDto } from "../rtuTree/rtuDto";
import { RtuStateAlarm } from "./rtuStateAlarm";

export class AlarmsDto {
  networkAlarms: NetworkAlarm[];
  opticalAlarms: OpticalAlarm[];
  bopAlarms: BopAlarm[];
  rtuStateAlarms: RtuStateAlarm[];
}

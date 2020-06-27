import { NetworkAlarm } from "./networkAlarm";
import { OpticalAlarm } from "./opticalAlarm";
import { BopAlarm } from "./bopAlarm";

export class AlarmsDto {
  networkAlarms: NetworkAlarm[];
  opticalAlarms: OpticalAlarm[];
  bopAlarms: BopAlarm[];
}

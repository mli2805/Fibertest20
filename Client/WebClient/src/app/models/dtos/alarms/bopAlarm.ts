import { BopEventDto } from "../bopEventDto";
import { IAlarm, AlarmIndicator } from "./alarm";

export class BopAlarm implements IAlarm {
  id: number;
  serial: string;
  hasBeenSeen: boolean;
}

export class BopAlarmIndicator extends AlarmIndicator {
  public BopEventReceived(signal: BopEventDto): string {
    const alarmsJson = sessionStorage.getItem(this.inStorageName);
    this.list = JSON.parse(alarmsJson) as BopAlarm[];

    const alarm = this.list.find((a) => a.serial === signal.serial);
    if (alarm === undefined && signal.bopState === false) {
      const newAlarm = new BopAlarm();
      newAlarm.id = signal.eventId;
      newAlarm.serial = signal.serial;
      newAlarm.hasBeenSeen = false;
      this.list.push(newAlarm);
    } else {
      if (signal.bopState) {
        const index = this.list.indexOf(alarm);
        this.list.splice(index, 1);
      } else {
        alarm.hasBeenSeen = false;
      }
    }
    sessionStorage.setItem(this.inStorageName, JSON.stringify(this.list));

    return this.GetIndicator();
  }
}

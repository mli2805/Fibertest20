import { BopEventDto } from "../bopEventDto";

export class BopAlarm {
  eventId: number;
  serial: string;
  hasBeenSeen: boolean;
}

export class BopAlarmIndicator {
  public list: BopAlarm[] = [];

  public BopEventReceived(signal: BopEventDto): string {
    const alarm = this.list.find((a) => a.serial === signal.serial);
    if (alarm === undefined && signal.bopState === false) {
      const newAlarm = new BopAlarm();
      newAlarm.eventId = signal.eventId;
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

    return this.GetIndicator();
  }

  public AlarmHasBeenSeen(id: number) {
    const alarm = this.list.find((a) => a.eventId === id);
    if (alarm !== undefined) {
      alarm.hasBeenSeen = true;
    }
    return this.GetIndicator();
  }

  public GetIndicator(): string {
    if (this.list.length === 0) {
      return "ok";
    }
    const hasNotSeenAlarms = this.list.filter((a) => a.hasBeenSeen === false);
    console.log(
      `events that has not been seen yet: ${hasNotSeenAlarms.length}`
    );
    if (hasNotSeenAlarms.length > 0) {
      return "alarmExclamation";
    } else {
      return "alarm";
    }
  }
}

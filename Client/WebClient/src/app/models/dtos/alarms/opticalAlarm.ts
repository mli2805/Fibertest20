import { TraceStateDto } from "../trace/traceStateDto";
import { FiberState } from "../../enums/fiberState";

export class OpticalAlarm {
  id: number; // sorFileId or eventId
  traceId: string;
  hasBeenSeen: boolean;
}

export class OpticalAlarmIndicator {
  list: OpticalAlarm[] = [];

  public TraceStateChanged(signal: TraceStateDto): string {
    const alarm = this.list.find((a) => a.traceId === signal.traceId);
    if (alarm === undefined && signal.traceState !== FiberState.Ok) {
      const newAlarm = new OpticalAlarm();
      newAlarm.traceId = signal.traceId;
      newAlarm.id = signal.sorFileId;
      newAlarm.hasBeenSeen = false;
      this.list.push(newAlarm);
    } else {
      if (signal.traceState === FiberState.Ok) {
        const index = this.list.indexOf(alarm);
        this.list.splice(index, 1);
      } else {
        alarm.id = signal.sorFileId;
        alarm.hasBeenSeen = false;
      }
    }
    return this.GetIndicator();
  }

  public AlarmHasBeenSeen(id: number): string {
    const alarm = this.list.find((a) => a.id === id);
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
    console.log(`unseen events: ${this.list.length}`);
    if (hasNotSeenAlarms.length > 0) {
      return "alarmExclamation";
    } else {
      return "alarm";
    }
  }
}

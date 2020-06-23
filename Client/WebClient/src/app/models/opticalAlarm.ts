import { TraceStateDto } from "./dtos/trace/traceStateDto";
import { FiberState } from "./enums/fiberState";

export class OpticalAlarm {
  sorFileId: number;
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
      newAlarm.sorFileId = signal.sorFileId;
      newAlarm.hasBeenSeen = false;
      this.list.push(newAlarm);
    } else {
      if (signal.traceState === FiberState.Ok) {
        const index = this.list.indexOf(alarm);
        this.list.splice(index, 1);
      } else {
        alarm.hasBeenSeen = false;
      }
    }
    return this.GetIndicator();
  }

  public AlarmHasBeenSeen(sorFileId: number): string {
    const alarm = this.list.find((a) => a.sorFileId === sorFileId);
    if (alarm !== undefined) {
      alarm.hasBeenSeen = true;
    }
    return this.GetIndicator();
  }

  public GetIndicator(): string {
    if (this.list.length === 0) {
      return "ok";
    }
    const hasNotSeenAlarms = this.list.some((a) => a.hasBeenSeen === false);
    if (hasNotSeenAlarms) {
      return "alarmExclamation";
    } else {
      return "alarm";
    }
  }
}

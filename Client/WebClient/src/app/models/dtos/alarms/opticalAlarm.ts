import { TraceStateDto } from "../trace/traceStateDto";
import { FiberState } from "../../enums/fiberState";
import { IAlarm, AlarmIndicator } from "./alarm";
import { TraceTachDto } from "../trace/traceTachDto";

export class OpticalAlarm implements IAlarm {
  id: number;
  traceId: string;
  hasBeenSeen: boolean;
}

export class OpticalAlarmIndicator extends AlarmIndicator {
  public TraceStateChanged(signal: TraceStateDto): string {
    const alarmsJson = sessionStorage.getItem(this.inStorageName);
    this.list = JSON.parse(alarmsJson) as OpticalAlarm[];

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
    sessionStorage.setItem(this.inStorageName, JSON.stringify(this.list));

    return this.GetIndicator();
  }

  public TraceTached(signal: TraceTachDto): string {
    console.log(signal);
    const alarmsJson = sessionStorage.getItem(this.inStorageName);
    this.list = JSON.parse(alarmsJson) as OpticalAlarm[];
    const alarm = this.list.find((a) => a.traceId === signal.traceId);

    if (signal.attach) {
      if (signal.traceState !== FiberState.Ok) {
        const newAlarm = new OpticalAlarm();
        newAlarm.traceId = signal.traceId;
        newAlarm.id = signal.sorFileId;
        newAlarm.hasBeenSeen = true;
        this.list.push(newAlarm);
      }
    } else {
      if (alarm !== undefined) {
        const index = this.list.indexOf(alarm);
        this.list.splice(index, 1);
      }
    }

    sessionStorage.setItem(this.inStorageName, JSON.stringify(this.list));
    return this.GetIndicator();
  }
}

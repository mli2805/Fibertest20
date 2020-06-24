import { NetworkEventDto } from "../networkEventDto";
import { ChannelEvent } from "../../enums/channelEvent";

export class NetworkAlarm {
  eventId: number;
  rtuId: string;
  channel: string; // Main or Reserve
  hasBeenSeen: boolean;
}

export class NetworkAlarmIndicator {
  list: NetworkAlarm[] = [];

  public NetworkEventReceived(signal: NetworkEventDto): string {
    if (signal.onMainChannel === ChannelEvent.Broken) {
      const oldAlarm = this.list.find(
        (a) => a.rtuId === signal.rtuId && a.channel === "Main"
      );
      if (oldAlarm === undefined) {
        const newAlarm = new NetworkAlarm();
        newAlarm.rtuId = signal.rtuId;
        newAlarm.channel = "Main";
        newAlarm.eventId = signal.eventId;
        newAlarm.hasBeenSeen = false;
        this.list.push(newAlarm);
      } else {
        oldAlarm.eventId = signal.eventId;
        oldAlarm.hasBeenSeen = false;
      }
    }
    if (signal.onMainChannel === ChannelEvent.Repaired) {
      const oldAlarm = this.list.find(
        (a) => a.rtuId === signal.rtuId && a.channel === "Main"
      );
      if (oldAlarm !== undefined) {
        const index = this.list.indexOf(oldAlarm);
        this.list.splice(index, 1);
      }
    }

    if (signal.onReserveChannel === ChannelEvent.Broken) {
      const oldAlarm = this.list.find(
        (a) => a.rtuId === signal.rtuId && a.channel === "Reserve"
      );
      if (oldAlarm === undefined) {
        const newAlarm = new NetworkAlarm();
        newAlarm.rtuId = signal.rtuId;
        newAlarm.channel = "Reserve";
        newAlarm.eventId = signal.eventId;
        newAlarm.hasBeenSeen = false;
        this.list.push(newAlarm);
      } else {
        oldAlarm.eventId = signal.eventId;
        oldAlarm.hasBeenSeen = false;
      }
    }
    if (signal.onReserveChannel === ChannelEvent.Repaired) {
      const oldAlarm = this.list.find(
        (a) => a.rtuId === signal.rtuId && a.channel === "Reserve"
      );
      if (oldAlarm !== undefined) {
        const index = this.list.indexOf(oldAlarm);
        this.list.splice(index, 1);
      }
    }

    return this.GetIndicator();
  }

  public AlarmHasBeenSeen(eventId: number): string {
    const alarm = this.list.find((a) => a.eventId === eventId);
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

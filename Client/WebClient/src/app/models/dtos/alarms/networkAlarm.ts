import { NetworkEventDto } from "../networkEventDto";
import { ChannelEvent } from "../../enums/channelEvent";
import { IAlarm, AlarmIndicator } from "./alarm";

export class NetworkAlarm implements IAlarm {
  id: number;
  rtuId: string;
  channel: string; // Main or Reserve
  hasBeenSeen: boolean;
}

export class NetworkAlarmIndicator extends AlarmIndicator {
  public NetworkEventReceived(signal: NetworkEventDto): string {
    const alarmsJson = sessionStorage.getItem(this.inStorageName);
    this.list = JSON.parse(alarmsJson) as NetworkAlarm[];

    if (signal.onMainChannel === ChannelEvent.Broken) {
      const oldAlarm = this.list.find(
        (a) => a.rtuId === signal.rtuId && a.channel === "Main"
      );
      if (oldAlarm === undefined) {
        const newAlarm = new NetworkAlarm();
        newAlarm.rtuId = signal.rtuId;
        newAlarm.channel = "Main";
        newAlarm.id = signal.eventId;
        newAlarm.hasBeenSeen = false;
        this.list.push(newAlarm);
      } else {
        oldAlarm.id = signal.eventId;
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
        newAlarm.id = signal.eventId;
        newAlarm.hasBeenSeen = false;
        this.list.push(newAlarm);
      } else {
        oldAlarm.id = signal.eventId;
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
    sessionStorage.setItem(this.inStorageName, JSON.stringify(this.list));

    return this.GetIndicator();
  }
}

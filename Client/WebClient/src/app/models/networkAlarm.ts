import { NetworkEventDto } from "./dtos/networkEventDto";

export class NetworkAlarm {
  rtuId: string;
  channel: string; // Main or Reserve
  hasBeenSeen: boolean;
}

export class NetworkAlarmIndicator {
  list: NetworkAlarm[] = [];

  public NetworkEventReceived(signal: NetworkEventDto): string {
    const alarm = this.list.find((a) => a.rtuId === signal.rtuId);
    return "ok";
  }
}

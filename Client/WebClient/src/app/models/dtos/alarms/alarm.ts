export interface IAlarm {
  id: number;
  hasBeenSeen: boolean;
}

export class AlarmIndicator {
  protected list;

  public constructor(protected inStorageName: string) {}

  public ClearList() {
    this.list = undefined;
  }

  public MarkAlarmHasBeenSeen(id: number): string {
    const alarmsJson = sessionStorage.getItem(this.inStorageName);
    this.list = JSON.parse(alarmsJson) as IAlarm[];
    const alarm = this.list.find((a) => a.id === id);
    if (alarm !== undefined) {
      alarm.hasBeenSeen = true;
    }
    sessionStorage.setItem(this.inStorageName, JSON.stringify(this.list));
    return this.GetIndicator();
  }

  public GetIndicator(): string {
    if (this.list === undefined) {
      const alarmsJson = sessionStorage.getItem(this.inStorageName);
      console.log(`${this.inStorageName} ${alarmsJson}`);
      if (alarmsJson === null) {
        return "";
      } else {
        this.list = JSON.parse(alarmsJson) as IAlarm[];
      }
    }
    if (this.list.length === 0) {
      return "ok";
    }
    const hasNotSeenAlarms = this.list.filter((a) => a.hasBeenSeen === false);
    if (hasNotSeenAlarms.length > 0) {
      return "alarmExclamation";
    } else {
      return "alarm";
    }
  }
}

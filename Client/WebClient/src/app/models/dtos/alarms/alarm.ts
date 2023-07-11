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

  public AlarmHasBeenSeen(id: number): string {
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
      if (alarmsJson === null) {
        return "";
      } else {
        this.list = JSON.parse(alarmsJson) as IAlarm[];
      }
    }
    if (this.list.length === 0) {
      console.log(`${this.inStorageName} has no accidents`)
      return "ok";
    }
    const hasNotSeenAlarms = this.list.filter((a) => a.hasBeenSeen === false);
    console.log(
      `${this.inStorageName} that has not been seen yet: ${hasNotSeenAlarms.length}`
    );
    if (hasNotSeenAlarms.length > 0) {
      console.log(hasNotSeenAlarms);
      return "alarmExclamation";
    } else {
      return "alarm";
    }
  }
}

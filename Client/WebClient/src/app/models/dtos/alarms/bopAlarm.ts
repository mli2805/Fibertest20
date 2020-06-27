export class BopAlarm {}

export class BopAlarmIndicator {
  public list: BopAlarm[] = [];

  public GetIndicator(): string {
    return "ok";
  }
}

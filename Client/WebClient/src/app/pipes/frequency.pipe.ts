import { PipeTransform, Pipe } from "@angular/core";
import { Frequency } from "../models/enums/frequency";
import { TranslateService } from "@ngx-translate/core";

@Pipe({ name: "frequencyToLocalizedStringPipe" })
export class FrequencyPipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: Frequency, isSave: boolean): string {
    switch (value) {
      case Frequency.DoNot:
        return this.ts.instant(
          isSave ? "SID_Do_not_save" : "SID_Do_not_measure"
        );
      case Frequency.EveryHour:
        return this.ts.instant("SID_Every_hour");
      case Frequency.Every6Hours:
        return this.ts.instant("SID_Every_6_hours");
      case Frequency.Every12Hours:
        return this.ts.instant("SID_Every_12_hours");
      case Frequency.EveryDay:
        return this.ts.instant("SID_Every_day");
      case Frequency.Every2Days:
        return this.ts.instant("SID_Every_2_days");
      case Frequency.Every7Days:
        return this.ts.instant("SID_Every_7_days");
      case Frequency.Every30Days:
        return this.ts.instant("SID_Every_30_days");
    }
  }
}

import { Pipe, PipeTransform } from "@angular/core";
import { MonitoringMode } from "../models/enums/monitoringMode";
import { TranslateService } from "@ngx-translate/core";

@Pipe({
  name: "monitoringModeToLocalizedStringPipe"
})
export class MonitoringModePipe implements PipeTransform {
  constructor(private ts: TranslateService) {}

  transform(value: MonitoringMode): string {
    switch (value) {
      case MonitoringMode.Off:
        return this.ts.instant("SID_Manual");
      case MonitoringMode.On:
        return this.ts.instant("SID_Auto");
      default:
        return this.ts.instant("SID_Unknown");
    }
  }
}
